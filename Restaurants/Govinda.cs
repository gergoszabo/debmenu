using System.Text;
using System.Web;
using debmenu.Caching;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

/// <summary>
/// Scraper for the Govinda étterem weekly menu. The page is dissected down to plain text
/// (headings preserved as markers) before being fed to the text extraction workflow, since
/// the raw HTML overflows the model's context window.
/// </summary>
public class Govinda(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger,
    IHttpResourceStateStore stateStore,
    IRestaurantResultCache resultCache) : Restaurant(
        "https://govindadebrecen.hu/heti-menu/",
        httpClientFactory,
        inferenceProvider,
        logger,
        [],
        stateStore,
        resultCache)
{
    public override async Task<Dictionary<string, List<string>>> GetOffersAsync()
    {
        return await GetOffersWithCachingAsync(() => TextWorkflow());
    }

    protected override string ExtractTask => GovindaPrompts.ExtractTask;

    /// <summary>
    /// The raw page is ~164 KB but only ~2 KB of it is the actual menu; the rest is
    /// inline JS/CSS, 37 &lt;noscript&gt; fallbacks and &lt;img srcset&gt; soup. Feeding the raw
    /// HTML to the model overflows its context window (llama.cpp rejects with 400), so we
    /// dissect it down to plain text here before TextWorkflow hands it to inference.
    /// </summary>
    protected override async Task<string> GetContentFromUrl()
    {
        var raw = await base.GetContentFromUrl();
        return DissectMenuHtml(raw);
    }

    internal static string DissectMenuHtml(string html)
    {
        if (html is null) throw new ArgumentNullException(nameof(html));

        // Keep only the body; it holds all the menu content.
        var bodyMatch = System.Text.RegularExpressions.Regex.Match(html, @"<body[^>]*>(.*)</body>",
            System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var text = bodyMatch.Success ? bodyMatch.Groups[1].Value : html;

        // Drop non-content blocks entirely (scripts, styles, noscript fallbacks).
        text = System.Text.RegularExpressions.Regex.Replace(text, @"<script.*?</script>|<style.*?</style>|<noscript.*?</noscript>", "",
            System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Images carry no offer info; their srcset attributes are the single biggest waste.
        text = System.Text.RegularExpressions.Regex.Replace(text, @"<img[^>]*>", "");

        // Preserve headings as markers (day names, section names).
        text = System.Text.RegularExpressions.Regex.Replace(text, @"<(h[1-6])[^>]*>(.*?)</\1>",
            m => $"\n## {m.Groups[2].Value}\n",
            System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Strip the remaining markup and unescape entities.
        text = System.Text.RegularExpressions.Regex.Replace(text, @"<[^>]+>", "\n");
        text = HttpUtility.HtmlDecode(text);

        var lines = text.Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        // Trim navigation boilerplate before the menu and footer/contact info after it.
        var start = lines.FindIndex(l => l.StartsWith("Hétfő"));
        var end = lines.FindIndex(l => l.StartsWith("Gyere be ebédelni"));
        if (start >= 0 && end > start)
            lines = lines.GetRange(start, end - start);

        return string.Join('\n', lines);
    }
}

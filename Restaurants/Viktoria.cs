using System.Text.RegularExpressions;
using debmenu.Caching;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

/// <summary>
/// Scraper for the Viktória étterem weekly menu. The page is dissected down to the menu
/// section before being fed to the text extraction workflow.
/// </summary>
public class Viktoria(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger,
    IHttpResourceStateStore stateStore,
    IRestaurantResultCache resultCache) : Restaurant(
        "https://www.viktoriaetterem.hu/menu",
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

    /// <summary>
    /// The raw page is ~150 KB but most of it is inline JS/CSS and site boilerplate. Keep only
    /// the menu section and drop non-content tags; the rest (day headers, &lt;h5&gt; dish titles)
    /// is left as-is for the model to interpret.
    /// </summary>
    protected override async Task<string> GetContentFromUrl()
    {
        var raw = await base.GetContentFromUrl();
        return DissectMenuHtml(raw);
    }

    internal static string DissectMenuHtml(string html)
    {
        if (html is null) throw new ArgumentNullException(nameof(html));

        // Keep only the menu section; fall back to the body, then the whole page.
        var scope = html;
        var mainStart = Regex.Match(html, @"<div[^>]*site-main[^>]*>");
        if (mainStart.Success)
        {
            var end = html.IndexOf("site-main end", mainStart.Index, StringComparison.Ordinal);
            scope = end > 0 ? html[mainStart.Index..end] : html[mainStart.Index..];
        }
        else
        {
            var body = Regex.Match(html, @"<body[^>]*>(.*)</body>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (body.Success) scope = body.Groups[1].Value;
        }

        // Scripts, styles and images carry no menu info.
        scope = Regex.Replace(scope, @"<(script|style|noscript)\b.*?</\1>|<img[^>]*>", " ",
            RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return scope;
    }
}

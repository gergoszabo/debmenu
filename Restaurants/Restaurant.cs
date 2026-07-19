using System.Text.Json;
using debmenu.Logging;
using debmenu.Providers.Inference;
using debmenu.Utils;
using Serilog;

namespace debmenu.Restaurants;

internal abstract class Restaurant(Uri uri,
    IHttpClientFactory httpClientFactory,
    IInferenceProvider inferenceProvider,
    ILogger logger) : IRestaurant
{
    public required Uri Uri { get; init; } = uri;
    public required IHttpClientFactory HttpClientFactory { get; init; } = httpClientFactory;
    public required IInferenceProvider InferenceProvider { get; init; } = inferenceProvider;
    public required ILogger Logger { get; init; } = logger;

    public virtual async Task<Dictionary<string, List<string>>> GetOffersAsync()
    {
        return await ImageWorkflow();
    }

    protected async Task<Dictionary<string, List<string>>> ImageWorkflow()
    {
        using var op = CreateTimedOperation(nameof(ImageWorkflow));
        string imageLink = await GetImageLinkFromUrl() ?? throw new ArgumentNullException("Failed to extract image link from HTML.");
        byte[] imageBytes = await GetImageBytesFromLink(new Uri(imageLink));
        string offersJson = await ExtractOffersFromImage(imageBytes, imageLink) ?? throw new FailedToExtractOffersFromImageException();

        var offers = ParseInferenceResponseAsOffers(offersJson);

        return offers;
    }

    protected async Task<Dictionary<string, List<string>>> HtmlWorkflow()
    {
        using var op = CreateTimedOperation(nameof(HtmlWorkflow));
        string html = await GetHtmlFromUrl();
        string offersJson = await ExtractOffersFromHtml(html) ?? throw new FailedToExtractOffersFromPageException();

        var offers = ParseInferenceResponseAsOffers(offersJson);

        return offers;
    }

    protected virtual async Task<string> GetImageLinkFromUrl()
    {
        using var _ = CreateTimedOperation(nameof(GetImageLinkFromUrl));
        string html = await GetHtmlFromUrl();
        string imageLink = await GetImageLinkFromHtml(html) ?? throw new ArgumentNullException("Failed to extract image link from HTML.");
        return imageLink;
    }

    protected async Task<string> GetHtmlFromUrl()
    {
        using var _ = CreateTimedOperation(nameof(GetHtmlFromUrl), Uri);
        using var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return await httpClient.GetStringAsync(Uri);
    }

    protected async Task<byte[]> GetImageBytesFromLink(Uri link)
    {
        using var _ = CreateTimedOperation(nameof(GetImageBytesFromLink), link);
        using var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return await httpClient.GetByteArrayAsync(link);
    }

    protected async Task<string?> GetImageLinkFromHtml(string html)
    {
        using var _ = CreateTimedOperation(nameof(GetImageLinkFromHtml), [$"{html.Length} bytes"]);
        InferenceProvider.AddContent($"{PromptConstants.ExtractImageLinkTask} {html}");
        return await InferenceProvider.Inference();
    }

    protected async Task<string?> ExtractOffersFromImage(byte[] imageBytes, string imageLink)
    {
        using var _ = CreateTimedOperation(nameof(ExtractOffersFromImage), $"{imageBytes.Length} bytes", imageLink);
        string mimeType = Utils.StringUtils.GetMimeTypeFromFilePath(imageLink);
        InferenceProvider.AddImage(imageBytes, mimeType);
        InferenceProvider.AddContent(PromptConstants.ExtractInstruction);
        return await InferenceProvider.Inference();
    }

    protected async Task<string?> ExtractOffersFromHtml(string html)
    {
        using var _ = CreateTimedOperation(nameof(ExtractOffersFromHtml), $"{html.Length} bytes");
        InferenceProvider.AddContent($"{PromptConstants.ExtractInstruction} {html}");
        return await InferenceProvider.Inference();
    }

    protected Dictionary<string, List<string>> ParseInferenceResponseAsOffers(string json)
    {
        using var _ = CreateTimedOperation(nameof(ParseInferenceResponseAsOffers), json.Length);
        return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? throw new UnableToParseInfereceResponseException();
    }

    protected TimedOperation CreateTimedOperation(string methodName, params object[] args)
    {
        return new TimedOperation("[{Class}] {Method} {args}", [GetType().Name, methodName, string.Join(' ', args)], Logger);
    }
}

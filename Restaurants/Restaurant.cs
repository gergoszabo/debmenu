using System.Runtime.CompilerServices;
using System.Text.Json;
using debmenu.Logging;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

public abstract class Restaurant(string url,
    IHttpClientFactory httpClientFactory,
    IInferenceProvider inferenceProvider,
    ILogger logger) : IRestaurant
{
    public required string Url { get; init; } = url;
    public required IHttpClientFactory HttpClientFactory { get; init; } = httpClientFactory;
    public required IInferenceProvider InferenceProvider { get; init; } = inferenceProvider;
    public required ILogger Logger { get; init; } = logger;

    public virtual async Task<Dictionary<string, List<string>>> GetOffers()
    {
        return await ImageWorkflow();
    }

    protected async Task<Dictionary<string, List<string>>> ImageWorkflow()
    {
         using var op = CreateTimedOperation()([]);
        var imageLink = await GetImageLinkFromUrl() ?? throw new ArgumentNullException("Failed to extract image link from HTML.");
        var imageBytes = await GetImageBytesFromLink(imageLink);
        var offersJson = await ExtractOffersFromImage(imageBytes, imageLink) ?? throw new Exception("Failed to extract offers from image.");

        var offers = ParseInferenceResponseAsOffers(offersJson);

        return offers;
    }

    protected async Task<Dictionary<string, List<string>>> HtmlWorkflow()
    {
        using var op = CreateTimedOperation()([]);
        var html = await GetHtmlFromUrl();
        var offersJson = await ExtractOffersFromHtml(html) ?? throw new Exception("Failed to extract offers from page");

        var offers = ParseInferenceResponseAsOffers(offersJson);

        return offers;
    }

    protected virtual async Task<string> GetImageLinkFromUrl()
    {
        using var _ = CreateTimedOperation()([]);
        var html = await GetHtmlFromUrl();
        var imageLink = await GetImageLinkFromHtml(html) ?? throw new ArgumentNullException("Failed to extract image link from HTML.");
        return imageLink;
    }

    protected async Task<string> GetHtmlFromUrl()
    {
        using var _ = CreateTimedOperation()([Url]);
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return  await httpClient.GetStringAsync(Url);
    }

    protected async Task<byte[]> GetImageBytesFromLink(string link)
    {
        using var _ = CreateTimedOperation()([link]);
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return await httpClient.GetByteArrayAsync(link);
    }

    protected async Task<string?> GetImageLinkFromHtml(string html)
    {
        using var _ = CreateTimedOperation()([$"{html.Length} bytes"]);
        InferenceProvider.AddContent($"{PromptConstants.ExtractImageLinkTask} {html}");
        return await InferenceProvider.Inference();
    }

    protected async Task<string?> ExtractOffersFromImage(byte[] imageBytes, string imageLink)
    {
        using var _ = CreateTimedOperation()([$"{imageBytes.Length} bytes", imageLink]);
        var mimeType = Utils.StringUtils.GetMimeTypeFromFilePath(imageLink);
        InferenceProvider.AddImage(imageBytes, mimeType);
        InferenceProvider.AddContent(PromptConstants.ExtractInstruction);
        return await InferenceProvider.Inference();
    }

    protected async Task<string?> ExtractOffersFromHtml(string html)
    {
        using var _ = CreateTimedOperation()([$"{html.Length} bytes"]);
        InferenceProvider.AddContent($"{PromptConstants.ExtractInstruction} {html}");
        return await InferenceProvider.Inference();
    }

    protected Dictionary<string, List<string>> ParseInferenceResponseAsOffers(string json)
    {
        using var _ = CreateTimedOperation()([json.Length]);
        return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? throw new Exception("Unable to parse json");
    }

    protected Func<object[], TimedOperation> CreateTimedOperation([CallerMemberName] string methodName = "")
    {
        return args => new TimedOperation("[{Class}] {Method} {args}", [GetType().Name, methodName, string.Join(' ', args)], Logger);
    }
}

public interface IRestaurant
{
    Task<Dictionary<string, List<string>>> GetOffers();
}

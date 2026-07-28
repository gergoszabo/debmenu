using System.Text.Json;
using debmenu.Logging;
using debmenu.Providers.Inference;
using debmenu.Utils;
using Serilog;

namespace debmenu.Restaurants;

public abstract class Restaurant(string url,
    IHttpClientFactory httpClientFactory,
    IInferenceProvider inferenceProvider,
    ILogger logger,
    List<string> extraInstructions) : IRestaurant
{
    public required string Url { get; init; } = url;
    public required IHttpClientFactory HttpClientFactory { get; init; } = httpClientFactory;
    public required IInferenceProvider InferenceProvider { get; init; } = inferenceProvider;
    public required ILogger Logger { get; init; } = logger;
    protected List<string> ExtractInstructions { get; set; } = [PromptConstants.ResponseExtractTask, PromptConstants.ResponseStructure, PromptConstants.DateGrounding, PromptConstants.YearGrounding];
    protected List<string> ExtraInstructions { get; set; } = extraInstructions;

    public InferenceResult? TotalInferenceCost { get; private set; }

    private int _totalPromptTokens;
    private int _totalCandidatesTokens;
    private int _totalTokens;

    public virtual async Task<Dictionary<string, List<string>>> GetOffersAsync()
    {
        return await ImageWorkflow();
    }

    protected void TrackInference(InferenceResult result)
    {
        _totalPromptTokens += result.PromptTokenCount;
        _totalCandidatesTokens += result.CandidatesTokenCount;
        _totalTokens += result.TotalTokenCount;
        TotalInferenceCost = new InferenceResult(null, _totalPromptTokens, _totalCandidatesTokens, _totalTokens);
    }

    private void LogInferenceCost()
    {
        if (TotalInferenceCost is not null)
            Logger.Information("[{Class}] Inference cost: {PromptTokenCount} prompt + {CandidatesTokenCount} response = {TotalTokenCount} total tokens",
                GetType().Name,
                TotalInferenceCost.PromptTokenCount,
                TotalInferenceCost.CandidatesTokenCount,
                TotalInferenceCost.TotalTokenCount);
    }

    protected async Task<Dictionary<string, List<string>>> ImageWorkflow()
    {
        using var op = CreateTimedOperation(nameof(ImageWorkflow));
        var imageLink = await GetImageLinkFromUrl() ?? throw new ArgumentNullException("Failed to extract image link from HTML.");
        var imageBytes = await GetImageBytesFromLink(imageLink);
        AddExtraImageExtractInstructions();
        var offersJson = await ExtractOffersFromImage(imageBytes, imageLink) ?? throw new Exception("Failed to extract offers from image.");

        var offers = ParseInferenceResponseAsOffers(offersJson);

        LogInferenceCost();
        return offers;
    }

    protected async Task<Dictionary<string, List<string>>> HtmlWorkflow()
    {
        using var op = CreateTimedOperation(nameof(HtmlWorkflow));
        var html = await GetHtmlFromUrl();
        var offersJson = await ExtractOffersFromHtml(html) ?? throw new Exception("Failed to extract offers from page");

        var offers = ParseInferenceResponseAsOffers(offersJson);

        LogInferenceCost();
        return offers;
    }

    protected virtual void AddExtraImageExtractInstructions()
    {
        this.ExtractInstructions.AddRange(this.ExtraInstructions);
    }

    protected virtual async Task<string> GetImageLinkFromUrl()
    {
        using var _ = CreateTimedOperation(nameof(GetImageLinkFromUrl));
        var html = await GetHtmlFromUrl();
        var imageLink = await GetImageLinkFromHtml(html) ?? throw new ArgumentNullException("Failed to extract image link from HTML.");
        return imageLink;
    }

    protected async Task<string> GetHtmlFromUrl()
    {
        using var _ = CreateTimedOperation(nameof(GetHtmlFromUrl), Url);
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return await httpClient.GetStringAsync(Url);
    }

    protected async Task<byte[]> GetImageBytesFromLink(string link)
    {
        using var _ = CreateTimedOperation(nameof(GetImageBytesFromLink), link);
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return await httpClient.GetByteArrayAsync(link);
    }

    protected async Task<string?> GetImageLinkFromHtml(string html)
    {
        using var _ = CreateTimedOperation(nameof(GetImageLinkFromHtml), [$"{html.Length} bytes"]);
        InferenceProvider.AddContent($"{PromptConstants.ExtractImageLinkTask} {html}");
        var result = await InferenceProvider.Inference();
        if (result is not null) TrackInference(result);
        return result?.Text;
    }

    protected async Task<string?> ExtractOffersFromImage(byte[] imageBytes, string imageLink)
    {
        using var _ = CreateTimedOperation(nameof(ExtractOffersFromImage), $"{imageBytes.Length} bytes", imageLink);
        var mimeType = Utils.StringUtils.GetMimeTypeFromFilePath(imageLink);
        InferenceProvider.AddImage(imageBytes, mimeType);
        InferenceProvider.AddContent(string.Join(' ', ExtractInstructions));
        var result = await InferenceProvider.Inference();
        if (result is not null) TrackInference(result);
        return result?.Text;
    }

    protected async Task<string?> ExtractOffersFromHtml(string html)
    {
        using var _ = CreateTimedOperation(nameof(ExtractOffersFromHtml), $"{html.Length} bytes");
        InferenceProvider.AddContent($"{PromptConstants.ExtractInstruction} {html}");
        var result = await InferenceProvider.Inference();
        if (result is not null) TrackInference(result);
        return result?.Text;
    }

    protected Dictionary<string, List<string>> ParseInferenceResponseAsOffers(string json)
    {
        using var _ = CreateTimedOperation(nameof(ParseInferenceResponseAsOffers), json.Length);
        return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? throw new Exception("Unable to parse json");
    }

    protected TimedOperation CreateTimedOperation(string methodName, params object[] args)
    {
        return new TimedOperation("[{Class}] {Method} {args}", [GetType().Name, methodName, string.Join(' ', args)], Logger);
    }
}

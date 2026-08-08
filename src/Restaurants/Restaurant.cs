using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using debmenu.Logging;
using debmenu.Caching;
using debmenu.Providers.Inference;
using debmenu.Utils;
using Serilog;

namespace debmenu.Restaurants;

[method: SetsRequiredMembers]
public abstract class Restaurant(string url,
    IHttpClientFactory httpClientFactory,
    IInferenceProvider inferenceProvider,
    ILogger logger,
    List<string> extraInstructions,
    IHttpResourceStateStore stateStore,
    IRestaurantResultCache resultCache) : IRestaurant
{
    public required string Url { get; init; } = url;
    public required IHttpClientFactory HttpClientFactory { get; init; } = httpClientFactory;
    public required IInferenceProvider InferenceProvider { get; init; } = inferenceProvider;
    public required ILogger Logger { get; init; } = logger;
    protected List<string> ExtractInstructions { get; set; } = [PromptConstants.ResponseExtractTask, PromptConstants.ResponseStructure, PromptConstants.DateGrounding, PromptConstants.YearGrounding];
    protected List<string> ExtraInstructions { get; set; } = extraInstructions;

    public InferenceResult? TotalInferenceCost { get; private set; }

    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

    private int _totalPromptTokens = 0;
    private int _totalCandidatesTokens = 0;
    private int _totalTokens = 0;

    public virtual async Task<Dictionary<string, List<string>>> GetOffersAsync()
    {
        return await GetOffersWithCachingAsync(() => ImageWorkflow());
    }

    private async Task<bool> HasUrlChangedAsync()
    {
        HttpResourceState? stored;
        try
        {
            stored = await stateStore.GetAsync(Url);
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "[{Class}] Failed to read HTTP state for {Url}, assuming changed", GetType().Name, Url);
            return true;
        }

        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);

        string? etag, lastModified;
        try
        {
            using var headResponse = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, Url));
            etag = headResponse.Headers.ETag?.ToString();
            lastModified = headResponse.Content.Headers.LastModified?.ToString();
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "[{Class}] HEAD request failed for {Url}, assuming changed", GetType().Name, Url);
            return true;
        }

        if (stored is not null && stored.ETag == etag && stored.LastModified == lastModified)
            return false;

        await stateStore.SetAsync(Url, new HttpResourceState(etag, lastModified));
        return true;
    }

    protected async Task<Dictionary<string, List<string>>> GetOffersWithCachingAsync(Func<Task<Dictionary<string, List<string>>>> fetchWorkflow)
    {
        if (!await HasUrlChangedAsync())
        {
            var cached = await resultCache.GetAsync(GetType().Name);
            if (cached is not null)
            {
                Logger.Information("[{Class}] Page unchanged, using cached offers", GetType().Name);
                return cached;
            }
            Logger.Warning("[{Class}] Page unchanged but no cached offers available, fetching anyway", GetType().Name);
        }

        var offers = await fetchWorkflow();
        await resultCache.SetAsync(GetType().Name, offers);
        return offers;
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

    protected async Task<Dictionary<string, List<string>>> TextWorkflow()
    {
        using var op = CreateTimedOperation(nameof(TextWorkflow));
        var textContent = await GetContentFromUrl();
        var offersJson = await ExtractOffersFromText(textContent) ?? throw new Exception("Failed to extract offers from page");

        var offers = ParseInferenceResponseAsOffers(offersJson);

        LogInferenceCost();
        return offers;
    }

    protected virtual void AddExtraImageExtractInstructions()
    {
        ExtractInstructions.AddRange(ExtraInstructions);
    }

    protected virtual async Task<string> GetImageLinkFromUrl()
    {
        using var _ = CreateTimedOperation(nameof(GetImageLinkFromUrl));
        var html = await GetContentFromUrl();
        var imageLink = await GetImageLinkFromHtml(html) ?? throw new ArgumentNullException("Failed to extract image link from HTML.");
        return imageLink;
    }

    protected async Task<string> GetContentFromUrl()
    {
        using var _ = CreateTimedOperation(nameof(GetContentFromUrl), Url);
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        return await httpClient.GetStringAsync(Url);
    }

    protected async Task<byte[]> GetImageBytesFromLink(string link)
    {
        using var _ = CreateTimedOperation(nameof(GetImageBytesFromLink), link);
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
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
        var mimeType = StringUtils.GetMimeTypeFromFilePath(imageLink);
        InferenceProvider.AddImage(imageBytes, mimeType);
        InferenceProvider.AddContent(string.Join(' ', ExtractInstructions));
        var result = await InferenceProvider.Inference();
        if (result is not null) TrackInference(result);
        return result?.Text;
    }

    protected virtual async Task<string?> ExtractOffersFromText(string html)
    {
        using var _ = CreateTimedOperation(nameof(ExtractOffersFromText), $"{html.Length} bytes");
        InferenceProvider.AddContent($"{PromptConstants.ExtractInstruction} {html}");
        var result = await InferenceProvider.Inference();
        if (result is not null) TrackInference(result);
        return result?.Text;
    }

    protected virtual Dictionary<string, List<string>> ParseInferenceResponseAsOffers(string json)
    {
        using var _ = CreateTimedOperation(nameof(ParseInferenceResponseAsOffers), json.Length);
        var offers = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? throw new Exception("Unable to parse json");
        return FilterOutdatedOffers(offers);
    }

    protected virtual Dictionary<string, List<string>> FilterOutdatedOffers(Dictionary<string, List<string>> offers)
    {
        if (offers == null) throw new Exception("Unable to parse json");

        // Calculate the start of the current calendar week (Monday 00:00:00 UTC).                                                                                                                           
        // This assumes Monday is the first day of the week.                                                                                                                                                 
        var now = DateTime.UtcNow;
        var startOfWeek = now.Date.AddDays(-(now.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)now.DayOfWeek - 1));

        // Filter out dates before the current calendar week.                                                                                                                                                
        var filteredOffers = offers
            .Where(kvp =>
            {
                if (DateTime.TryParse(kvp.Key, out var date))
                {
                    return date >= startOfWeek;
                }
                // If parsing fails, we might treat it as invalid and discard it, or keep it if robustness is key.                                                                                           
                // Since the prompt implies structured data, let's assume valid YYYY-MM-DD format for keys.                                                                                                  
                // For safety, I will only include keys that successfully parse to a date >= startOfWeek.                                                                                                    
                return false;
            })
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return filteredOffers;
    }

    protected TimedOperation CreateTimedOperation(string methodName, params object[] args)
    {
        return new TimedOperation("[{Class}] {Method} {args}", [GetType().Name, methodName, string.Join(' ', args)], Logger);
    }
}

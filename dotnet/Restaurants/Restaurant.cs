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

    public abstract Task<Dictionary<string, List<string>>> GetOffers();

    protected Task<string> GetHtmlFromUrl()
    {
        using var _ = CreateTimedOperation()([Url]);
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return httpClient.GetStringAsync(Url);
    }

    protected Task<byte[]> GetImageBytesFromLink(string link)
    {
        using var _ = CreateTimedOperation()([link]);
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return httpClient.GetByteArrayAsync(link);
    }

    protected Task<string?> GetImageLinkFromHtml(string html)
    {
        using var _ = CreateTimedOperation()([html.Length]);
        InferenceProvider.AddContent(html);
        return InferenceProvider.Inference();
    }

    protected Task<string?> ExtractOffersFromImage(byte[] imageBytes, string imageLink)
    {
        using var _ = CreateTimedOperation()([imageBytes.Length, imageLink]);
        InferenceProvider.AddImage(imageBytes, imageLink);
        InferenceProvider.AddContent(string.Empty);
        return InferenceProvider.Inference();
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

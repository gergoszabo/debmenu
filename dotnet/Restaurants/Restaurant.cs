using System.Text.Json;
using debmenu.Providers.Inference;

namespace debmenu.Restaurants;

internal abstract class Restaurant : IRestaurant
{
    internal required string Url { get; init; }
    internal required IHttpClientFactory HttpClientFactory { get; init; }
    internal required IInferenceProvider InferenceProvider { get; init; }

    internal Restaurant(string url,
        IHttpClientFactory httpClientFactory,
        IInferenceProvider inferenceProvider)
    {
        Url = url;
        HttpClientFactory = httpClientFactory;
        InferenceProvider = inferenceProvider;
    }

    public abstract Task<Dictionary<string, List<string>>> GetOffers(Gemini gemini);

    protected Task<string> GetHtmlFromUrl()
    {
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return httpClient.GetStringAsync(Url);
    }

    protected Task<byte[]> GetImageBytesFromLink(string link)
    {
        var httpClient = HttpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return httpClient.GetByteArrayAsync(link);
    }

    protected Task<string?> GetImageLinkFromHtml(string html)
    {
        InferenceProvider.AddContent(html);
        return InferenceProvider.Inference();
    }

    protected Task<string?> ExtractOffersFromImage(byte[] imageBytes, string imageLink)
    {
        InferenceProvider.AddImage(imageBytes, imageLink);
        InferenceProvider.AddContent(string.Empty);
        return InferenceProvider.Inference();
    }

    protected Dictionary<string, List<string>> ParseInferenceResponseAsOffers(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? throw new Exception("Unable to parse json");
    }
}

internal interface IRestaurant
{
    Task<Dictionary<string, List<string>>> GetOffers(Gemini gemini);
}

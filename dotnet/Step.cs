namespace debmenu;

internal abstract class Step<TResult>()
{
    public abstract Task<TResult> Execute();
}

internal abstract class GeminiStep<TResult>(Gemini gemini) : Step<TResult>()
{
    protected Gemini Client { get; init; } = gemini;
}

internal static class StepExtension
{
    private static readonly HttpClient _httpClient = new();
    
    static StepExtension()
    {
        // Add a standard User-Agent header so the server doesn't reject or ignore the request
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
    }

    public static Task<string> GetStringAsync(this Step<string> step, string url)
    {
        return _httpClient.GetStringAsync(url);
    }

    public static Task<byte[]> GetByteArrayAsync(this Step<byte[]> step, string url)
    {
        return _httpClient.GetByteArrayAsync(url);
    }
}

internal class GetHtmlStep(string url) : Step<string>()
{
    public override Task<string> Execute()
    {
        Console.WriteLine($"[GetHtmlStep] {url}");
        return this.GetStringAsync(url);
    }
}

internal class GetImageLinkFromHtmlStep(string html, Gemini gemini) : GeminiStep<string?>(gemini)
{
    public override Task<string?> Execute()
    {
        Console.WriteLine("[GetImageLinkFromHtmlStep]");
        return Client.NewRequest()
            .AddImageLinkExtractTask(html)
            .SendAsync();
    }
}

internal class SaveImageFromUrlStep(string imageUrl) : Step<byte[]>()
{
    public override async Task<byte[]> Execute()
    {
        Console.WriteLine($"[SaveImageFromUrlStep] {imageUrl}");
        var bytes = await this.GetByteArrayAsync(imageUrl);

        return bytes;
    }
}

internal class ExtractOffersFromImageStep(byte[] imageBytes, string imageLink, Gemini gemini) : GeminiStep<string?>(gemini)
{
    public override Task<string?> Execute()
    {
        Console.WriteLine($"[ExtractOffersFromImageStep] {imageLink[Math.Min(25, imageLink.Length)..]} {imageBytes.Length} bytes");
        return Client.NewRequest()
            .AddImage(imageBytes, imageLink)
            .AddExtractTask()
            .SendAsync();
    }
}

internal class GetOffersFromHtmlStep(string html, Gemini gemini) : GeminiStep<string?>(gemini)
{
    public override Task<string?> Execute()
    {
        Console.WriteLine($"[GetOffersFromHtmlStep]");
        return Client.NewRequest()
            .AddExtractTask(html)
            .SendAsync();
    }
}

internal class ParseOffersStep(string offersJson) : Step<Dictionary<string, List<string>>>()
{
    public override Task<Dictionary<string, List<string>>> Execute()
    {
        Console.WriteLine($"[ParseOffersStep]");
        var offers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<string>>>(offersJson) ?? throw new Exception("Failed to parse offers JSON.");
        return Task.FromResult(offers);
    }
}

 
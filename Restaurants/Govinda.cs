using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

public class Govinda (
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger) : Restaurant(
        "https://www.govindadebrecen.hu/",
        httpClientFactory,
        inferenceProvider,
        logger)
{
    protected override async Task<string> GetImageLinkFromUrl()
    {
        using var op = CreateTimedOperation(nameof(GetImageLinkFromUrl));
        var html = await GetHtmlFromUrl();
        var imageLink = await GetImageLinkFromHtml(html) ?? throw new ArgumentNullException("Failed to extract image link from HTML.");

        return $"{Url}{imageLink}";
    }
}
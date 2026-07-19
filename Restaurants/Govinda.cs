using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

#pragma warning disable CA1812
internal sealed class Govinda(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger) : Restaurant(
        new Uri("https://www.govindadebrecen.hu/"),
        httpClientFactory,
        inferenceProvider,
        logger)
{
    protected override async Task<string> GetImageLinkFromUrl()
    {
        using var op = CreateTimedOperation(nameof(GetImageLinkFromUrl));
        string html = await GetHtmlFromUrl();
        string imageLink = await GetImageLinkFromHtml(html) ?? throw new ArgumentNullException("Failed to extract image link from HTML.");

        return $"{Uri}{imageLink}";
    }
}
#pragma warning restore CA1812

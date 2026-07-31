using debmenu.Caching;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

public class Govinda(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger,
    IHttpResourceStateStore stateStore,
    IRestaurantResultCache resultCache) : Restaurant(
        "https://www.govindadebrecen.hu/",
        httpClientFactory,
        inferenceProvider,
        logger,
        [],
        stateStore,
        resultCache)
{
    protected override async Task<string> GetImageLinkFromUrl()
    {
        using var op = CreateTimedOperation(nameof(GetImageLinkFromUrl));
        var html = await GetContentFromUrl();
        var imageLink = await GetImageLinkFromHtml(html) ?? throw new ArgumentNullException("Failed to extract image link from HTML.");

        return $"{Url}{imageLink}";
    }
}
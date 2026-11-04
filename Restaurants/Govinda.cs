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
        "https://govindadebrecen.hu/heti-menu/",
        httpClientFactory,
        inferenceProvider,
        logger,
        [],
        stateStore,
        resultCache)
{
    public override async Task<Dictionary<string, List<string>>> GetOffersAsync()
    {
        return await GetOffersWithCachingAsync(() => TextWorkflow());
    }
}

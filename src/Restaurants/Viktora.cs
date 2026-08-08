using System.Diagnostics.CodeAnalysis;
using debmenu.Caching;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

[method: SetsRequiredMembers]
public class Viktoria(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger,
    IHttpResourceStateStore stateStore,
    IRestaurantResultCache resultCache) : Restaurant(
        "https://www.viktoriaetterem.hu/menu",
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
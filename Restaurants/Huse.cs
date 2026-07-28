using debmenu.Caching;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

public class Huse(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger,
    IHttpResourceStateStore stateStore,
    IRestaurantResultCache resultCache) : Restaurant(
        "https://husevendeglo.hu/napi-ajanlat/",
        httpClientFactory,
        inferenceProvider,
        logger,
        ["Make sure the extra offering 'Állandó ajánlatunk' gets added to every day"],
        stateStore,
        resultCache)
{
}
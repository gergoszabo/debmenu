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
        [
            "Make sure the extra offering 'Állandó ajánlatunk' gets added to every day",
            $"The menu image shows days like 'November 16.' without a year. This is the current week's menu: assign the current year ({DateTime.UtcNow.Year}) to every day so all dates fall within the current calendar week (today is {DateTime.UtcNow:yyyy-MM-dd}). Never output a year other than {DateTime.UtcNow.Year}."
        ],
        stateStore,
        resultCache)
{
}
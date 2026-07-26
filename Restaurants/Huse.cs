using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

public class Huse(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger) : Restaurant(
        "https://husevendeglo.hu/napi-ajanlat/",
        httpClientFactory,
        inferenceProvider,
        logger,
        ["Make sure the extra offering 'Állandó ajánlatunk' gets added to every day"])
{
}
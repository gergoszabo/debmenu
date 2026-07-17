using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

public class Forest(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger) : Restaurant(
        "https://forestetterem.hu/",
        httpClientFactory,
        inferenceProvider,
        logger)
{
}
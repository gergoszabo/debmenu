using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

#pragma warning disable CA1812
internal sealed class Forest(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger) : Restaurant(
        new Uri("https://forestetterem.hu/"),
        httpClientFactory,
        inferenceProvider,
        logger)
{
}
#pragma warning restore CA1812

using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

#pragma warning disable CA1812
internal sealed class Huse(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger) : Restaurant(
        new Uri("https://husevendeglo.hu/napi-ajanlat/"),
        httpClientFactory,
        inferenceProvider,
        logger)
{
}
#pragma warning restore CA1812

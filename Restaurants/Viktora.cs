using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

#pragma warning disable CA1812
internal sealed class Viktoria(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger) : Restaurant(
        new Uri("https://www.viktoriaetterem.hu/menu"),
        httpClientFactory,
        inferenceProvider,
        logger)
{
    public override async Task<Dictionary<string, List<string>>> GetOffersAsync()
    {
        return await HtmlWorkflow();
    }
}
#pragma warning restore CA1812

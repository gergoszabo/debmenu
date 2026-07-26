using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

public class Viktoria(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger) : Restaurant(
        "https://www.viktoriaetterem.hu/menu",
        httpClientFactory,
        inferenceProvider,
        logger,
        [])
{
    public override async Task<Dictionary<string, List<string>>> GetOffersAsync()
    {
        return await HtmlWorkflow();
    }
}
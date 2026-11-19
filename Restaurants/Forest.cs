using debmenu.Caching;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

public class Forest(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger,
    IHttpResourceStateStore stateStore,
    IRestaurantResultCache resultCache) : Restaurant(
        "https://forestetterem.hu/",
        httpClientFactory,
        inferenceProvider,
        logger,
        [],
        stateStore,
        resultCache)
{
    protected override string ExtractTask => ForestPrompts.ExtractTask;
    protected override async Task<string> GetImageLinkFromUrl()
    {
        var html = await GetContentFromUrl();
        html = html[html.IndexOf("Heti étlap")..];
        var srcIdx = html.IndexOf("src=\"/files/heti-etlap", StringComparison.OrdinalIgnoreCase);
        if (srcIdx < 0) throw new Exception("Menu image not found");
        html = html[(srcIdx + 5)..]; // skip 'src='
        var endIdx = html.IndexOf('"');
        var path = html[..endIdx];

        return new Uri(new Uri("https://forestetterem.hu"), path).ToString();
    }
}
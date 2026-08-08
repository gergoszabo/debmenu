using System.Diagnostics.CodeAnalysis;
using debmenu.Caching;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

[method: SetsRequiredMembers]
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
        ["There is a section called 'Heti leves ajánlat' and 'Heti grill ajánlat'. ",
        "Those needs to be added to every day. The 'Heti grill ajánlat' contains one item each line, ",
        "it might happen that it was broken into multiple line due space constraints, apply some hungarian linguistics to detect this"],
        stateStore,
        resultCache)
{
    protected override async Task<string> GetImageLinkFromUrl()
    {
        var html = await GetContentFromUrl();
        html = html[html.IndexOf("Heti étlap")..];
        html = html[..(html.IndexOf(".jpg\"") + 4)];
        html = html[html.IndexOf("http://")..];

        return html;
    }
}
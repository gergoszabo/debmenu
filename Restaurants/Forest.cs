using debmenu.Caching;
using debmenu.Providers.Inference;
using debmenu.Utils;
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
    protected override string ExtractTask => PromptConstants.ResponseExtractTaskFromImage;
    protected override async Task<string> GetImageLinkFromUrl()
    {
        using var _ = CreateTimedOperation(nameof(GetImageLinkFromUrl));
        var html = await GetContentFromUrl();

        // Slice the page down to the weekly-menu section so the raw substring search below
        // cannot match an unrelated image elsewhere on the page.
        var menuIdx = html.IndexOf("Heti étlap", StringComparison.Ordinal);
        if (menuIdx < 0)
            throw new OfferExtractionException(debmenu_Resources_ErrorMessages.MenuImageNotFound);
        html = html[menuIdx..];

        var srcIdx = html.IndexOf("src=\"/files/heti-etlap", StringComparison.OrdinalIgnoreCase);
        if (srcIdx < 0) throw new OfferExtractionException(debmenu_Resources_ErrorMessages.MenuImageNotFound);
        html = html[(srcIdx + 5)..]; // skip 'src='

        var endIdx = html.IndexOf('"');
        if (endIdx < 0) throw new OfferExtractionException(debmenu_Resources_ErrorMessages.MenuImageNotFound);
        var path = html[..endIdx];

        return new Uri(new Uri("https://forestetterem.hu"), path).ToString();
    }
}
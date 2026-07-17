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
    public override async Task<Dictionary<string, List<string>>> GetOffers()
    {
        using var op = CreateTimedOperation()([]);
        var html = await GetHtmlFromUrl();
        var imageLink = await GetImageLinkFromHtml(html) ?? throw new ArgumentNullException("imageLink");
        var imageBytes = await GetImageBytesFromLink(imageLink);
        var offersJson = await ExtractOffersFromImage(imageBytes, imageLink) ?? throw new Exception("Failed to extract offers from image.");

        var offers = ParseInferenceResponseAsOffers(offersJson);

        return offers;
    }
}
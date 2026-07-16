using debmenu.Providers.Inference;

namespace debmenu.Restaurants;

internal class Forest : Restaurant
{
    internal Forest(
        IInferenceProvider inferenceProvider,
        IHttpClientFactory httpClientFactory) : base(
            "https://forestetterem.hu/",
            httpClientFactory,
            inferenceProvider)
    { }

    public override async Task<Dictionary<string, List<string>>> GetOffers(Gemini gemini)
    {
        var html = await GetHtmlFromUrl();
        var imageLink = await GetImageLinkFromHtml(html) ?? throw new ArgumentNullException("imageLink");
        var imageBytes = await GetImageBytesFromLink(imageLink);
        var offersJson = await ExtractOffersFromImage(imageBytes, imageLink) ?? throw new Exception("Failed to extract offers from image.");

        var offers = ParseInferenceResponseAsOffers(offersJson);

        return offers;
    }
}
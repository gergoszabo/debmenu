namespace debmenu;

internal static class Forest
{
    public static async Task<Dictionary<string, List<string>>> GetOffers(Gemini gemini)
    {
        var url = "https://forestetterem.hu/";
        var html = await new GetHtmlStep(url).Execute();
        var imageLink = await new GetImageLinkFromHtmlStep(html, gemini).Execute() ?? throw new Exception("Failed to extract image link from HTML.");
        var imageBytes = await new SaveImageFromUrlStep(imageLink).Execute();
        var offers = await new ExtractOffersFromImageStep(imageBytes, imageLink, gemini).Execute() ?? throw new Exception("Failed to extract offers from image.");
        return await new ParseOffersStep(offers).Execute();
    }
}
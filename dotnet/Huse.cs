namespace debmenu;

internal static class Huse
{
    public static async Task<Dictionary<string, List<string>>> GetOffers(Gemini gemini)
    {
        var url = "https://husevendeglo.hu/napi-ajanlat/";
        var html = await new GetHtmlStep(url).Execute();
        var imageLink = await new GetImageLinkFromHtmlStep(html, gemini).Execute() ?? throw new Exception("Failed to extract image link from HTML.");
        var imagePath = "huse.jpg";
        await new SaveImageFromUrlStep(imageLink, imagePath).Execute();
        var offers = await new ExtractOffersFromImageStep(imagePath, gemini).Execute() ?? throw new Exception("Failed to extract offers from image.");
        return await new ParseOffersStep(offers).Execute();
    }
}
namespace debmenu;

internal static class Viktoria
{
    public static async Task<Dictionary<string, List<string>>> GetOffers(Gemini gemini)
    {
        var url = "https://www.viktoriaetterem.hu/menu";
        var html = await new GetHtmlStep(url).Execute();
        var offers = await new GetOffersFromHtmlStep(html, gemini).Execute() ?? throw new Exception("Failed to extract offers from HTML.");
        return await new ParseOffersStep(offers).Execute();
    }
}
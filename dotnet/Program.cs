namespace debmenu;

using System.Text.Encodings.Web;
using System.Text.Json;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Env.Load();

        var gemini = new Gemini(Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? throw new Exception("GEMINI_API_KEY not set"));
        var viktoria = await Viktoria.GetOffers(gemini);
        var govinda = await Govinda.GetOffers(gemini);
        var forest = await Forest.GetOffers(gemini);
        var huse = await Huse.GetOffers(gemini);

        var allOffers = new Dictionary<string, Dictionary<string, List<string>>>
        {
            { "Viktoria", viktoria },
            { "Govinda", govinda },
            { "Forest", forest },
            { "Huse", huse }
        };

        await File.WriteAllTextAsync("offers.json", JsonSerializer.Serialize(allOffers, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

        var offers = await File.ReadAllTextAsync("offers.json");
        var template = await File.ReadAllTextAsync("template.html");
        var html = template.Replace("JSON_HERE", offers);
        await File.WriteAllTextAsync("index.html", html);
    }
}

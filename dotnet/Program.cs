namespace debmenu;

using System.Text;
using Google.GenAI;
using Google.GenAI.Types;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Env.Load();
        await Test();
    }

    private static async Task Test()
    {
        var apiKey = System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        using var client = new Client(apiKey: apiKey);

        // var bytes = await System.IO.File.ReadAllBytesAsync("FOREST_07.06.jpg");
        var bytes = await System.IO.File.ReadAllBytesAsync("huse.jpg");
        var imageData = Convert.ToBase64String(bytes);

        var imagePart = new Part
        {
            InlineData = new()
            {
                Data = bytes,
                MimeType = "image/jpeg"
            }
        };
        var textPart = new Part
        {
            Text = $@"{PromptConstants.ResponseExtractTask} {PromptConstants.ResponseStructure} {PromptConstants.DateGrounding} {PromptConstants.YearGrounding}"
        };
        var parts = new List<Part> { imagePart, textPart };
        var content = new Content
        {
            Parts = parts
        };

        var response = await client.Models.GenerateContentAsync(
            model: "gemini-2.5-flash", 
            contents: content,
            config: new GenerateContentConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    ThinkingBudget = 0
                }
            }
        );
        
        Console.WriteLine("{0}: {1}", response.UsageMetadata?.TotalTokenCount, response.Candidates?[0]?.Content?.Parts?[0].Text);
    }
}

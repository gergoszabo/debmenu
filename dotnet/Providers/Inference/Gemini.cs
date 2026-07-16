using Google.GenAI;
using Google.GenAI.Types;

namespace debmenu.Providers.Inference;

public class Gemini : IInferenceProvider
{
    private GeminiOptions Options { get; }
    private readonly Client client;

    private readonly List<Part> ContentParts = [];

    public Gemini(GeminiOptions options)
    {
        Options = options;
        client = new(apiKey: Options.ApiKey);
    }

    public void AddContent(string content)
    {
        var textPart = new Part
        {
            Text = $@"{PromptConstants.ResponseExtractTask} {PromptConstants.ResponseStructure} {PromptConstants.DateGrounding} {PromptConstants.YearGrounding} {content}"
        };
        ContentParts.Add(textPart);
    }

    public void AddImage(byte[] imageBytes, string mimeType)
    {
        var imagePart = new Part
        {
            InlineData = new()
            {
                Data = imageBytes,
                MimeType = mimeType
            }
        };
        ContentParts.Add(imagePart);
    }

    public Task<string?> Inference()
    {
        var content = new Content
        {
            Parts = ContentParts
        };

        return client.Models.GenerateContentAsync(
            model: Options.Model, 
            contents: content,
            config: new GenerateContentConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    ThinkingBudget = 0
                }
            }
        ).ContinueWith(HandleResponse);
    }

    private string? HandleResponse(Task<GenerateContentResponse> response)
    {
        ContentParts.Clear();

        var result = response.GetAwaiter().GetResult();

        var textContent = result?.Candidates?[0]?.Content?.Parts?[0].Text;

        if (string.IsNullOrEmpty(textContent))
        {
            throw new Exception("No text content found in the response.");
        }

        return textContent.Replace("```json", "").Replace("```", "").Trim();
    }
}

public class GeminiOptions
{
    public required string ApiKey { get; set; } = string.Empty;
    public required string Model { get; set; } = "gemini-3.1-flash-lite";
}

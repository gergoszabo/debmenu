using System.ComponentModel.DataAnnotations;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;

namespace debmenu.Providers.Inference;

public class Gemini : IInferenceProvider
{
    private GeminiOptions Options { get; }
    private readonly Client client;

    private readonly List<Part> ContentParts = [];

    public Gemini(IOptions<GeminiOptions> options)
    {
        Options = options.Value;
        client = new(apiKey: Options.ApiKey);
    }

    public void AddContent(string content)
    {
        var textPart = new Part
        {
            Text = content
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

    public async Task<string?> Inference()
    {
        var content = new Content
        {
            Parts = ContentParts
        };

        var response = await client.Models.GenerateContentAsync(
            model: Options.Model, 
            contents: content,
            config: new GenerateContentConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    ThinkingBudget = 0
                }
            }
        );

        ContentParts.Clear();

        var textContent = response?.Candidates?[0]?.Content?.Parts?[0].Text;

        if (string.IsNullOrEmpty(textContent))
        {
            throw new Exception("No text content found in the response.");
        }

        return textContent.Replace("```json", "").Replace("```", "").Trim();
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
    [Required(AllowEmptyStrings = false, ErrorMessage = "Gemini API Key is missing or empty in configuration.")]
    public required string ApiKey { get; set; } = string.Empty;
    public required string Model { get; set; } = "gemini-3.1-flash-lite";
}

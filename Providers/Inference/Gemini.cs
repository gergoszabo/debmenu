using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;

namespace debmenu.Providers.Inference;

#pragma warning disable CA1812
internal sealed class Gemini : IInferenceProvider, IDisposable
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

        string? textContent = response?.Candidates?[0]?.Content?.Parts?[0].Text;

        if (string.IsNullOrEmpty(textContent))
        {
            throw new NoTextContentFoundInResponseException();
        }

        return textContent.Replace("```json", "", StringComparison.InvariantCulture).Replace("```", "", StringComparison.InvariantCulture).Trim();
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
#pragma warning restore CA1812

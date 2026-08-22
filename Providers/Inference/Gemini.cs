using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;

namespace debmenu.Providers.Inference;

public class Gemini : IInferenceProvider
{
    private GeminiOptions Options { get; }
    private readonly Client client;

    public Gemini(IOptions<GeminiOptions> options)
    {
        Options = options.Value;
        client = new(apiKey: Options.ApiKey);
    }

    public async Task<InferenceResult?> Inference(string content)
    {
        return await Inference(content, [], string.Empty);
    }

    public async Task<InferenceResult?> Inference(string content, byte[] imageBytes, string mimeType)
    {
        List<Part> parts = [new Part { Text = content }];
        if (imageBytes.Length > 0 && !string.IsNullOrWhiteSpace(mimeType))
        {
            parts.Add(new Part
            {
                InlineData = new()
                {
                    Data = imageBytes,
                    MimeType = mimeType
                }
            });
        }

        var response = await client.Models.GenerateContentAsync(
            model: Options.Model,
            contents: new Content
            {
                Parts = parts
            },
            config: new GenerateContentConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    ThinkingBudget = 0
                }
            }
        );

        var textContent = response?.Candidates?[0]?.Content?.Parts?[0].Text;

        if (string.IsNullOrEmpty(textContent))
        {
            throw new Exception("No text content found in the response.");
        }

        var usage = response?.UsageMetadata;
        var result = new InferenceResult(
            textContent.Replace("```json", "").Replace("```", "").Trim(),
            usage?.PromptTokenCount ?? 0,
            usage?.CandidatesTokenCount ?? 0,
            usage?.TotalTokenCount ?? 0
        );

        return result;
    }
}

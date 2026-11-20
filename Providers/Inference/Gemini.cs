using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Options;

namespace debmenu.Providers.Inference;

/// <summary>
/// Inference provider backed by the Google Gemini API.
/// </summary>
public class Gemini : IInferenceProvider, IDisposable
{
    private GeminiOptions Options { get; }
    private readonly Client client;

    private readonly List<Part> ContentParts = [];

    /// <inheritdoc/>
    public string ProviderIdentity => $"gemini/{Options.Model}";

    /// <summary>
    /// Initializes a new instance with the Gemini API client.
    /// </summary>
    /// <param name="options">Gemini settings (API key, model).</param>
    public Gemini(IOptions<GeminiOptions> options)
    {
        Options = options.Value;
        client = new(apiKey: Options.ApiKey);
    }

    /// <summary>
    /// Buffers a text part to be included in the next inference call.
    /// </summary>
    /// <param name="content">The text content.</param>
    public void AddContent(string content)
    {
        var textPart = new Part
        {
            Text = content
        };
        ContentParts.Add(textPart);
    }

    /// <summary>
    /// Buffers an inline image part to be included in the next inference call.
    /// </summary>
    /// <param name="imageBytes">The raw image bytes.</param>
    /// <param name="mimeType">The image MIME type, e.g. <c>image/png</c>.</param>
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

    /// <inheritdoc/>
    public void Reset() => ContentParts.Clear();

    /// <summary>
    /// Disposes the underlying Gemini API client.
    /// </summary>
    public void Dispose()
    {
        client.Dispose();
    }

    /// <inheritdoc/>
    public async Task<InferenceResult?> Inference()
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
            throw new InferenceException(debmenu_Resources_ErrorMessages.NoTextContentFoundInResponse);
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

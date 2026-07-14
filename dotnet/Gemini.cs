using Google.GenAI;
using Google.GenAI.Types;

namespace debmenu;

internal class Gemini(string apiKey)
{
    private readonly Client client = new(apiKey: apiKey);
    private List<Part> parts = [];

    public Gemini NewRequest()
    {
        parts = [];
        return this;
    }

    public Gemini AddImage(byte[] imageBytes, string imageLink)
    {
        var imagePart = new Part
        {
            InlineData = new()
            {
                Data = imageBytes,
                MimeType = GetMimeTypeFromFilePath(imageLink)
            }
        };
        parts.Add(imagePart);
        return this;
    }

    public Gemini AddImageLinkExtractTask(string html)
    {
        var textPart = new Part
        {
            Text = $@"{PromptConstants.ExtractImageLinkTask} {html}"
        };
        parts.Add(textPart);
        return this;
    }

    public Gemini AddExtractTask()
    {
        return AddExtractTask(string.Empty);
    }

    public Gemini AddExtractTask(string html)
    {
        var textPart = new Part
        {
            Text = $@"{PromptConstants.ResponseExtractTask} {PromptConstants.ResponseStructure} {PromptConstants.DateGrounding} {PromptConstants.YearGrounding} {html}"
        };
        parts.Add(textPart);
        return this;
    }

    public Task<string?> SendAsync()
    {
        var content = new Content
        {
            Parts = parts
        };

        return client.Models.GenerateContentAsync(
            model: "gemini-3.1-flash-lite", 
            contents: content,
            config: new GenerateContentConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    ThinkingBudget = 0
                }
            }
        ).ContinueWith(response => response.GetAwaiter().GetResult()?.Candidates?[0]?.Content?.Parts?[0].Text?.Replace("```json", "").Replace("```", "").Trim());
    }

    public string? Send()
    {
        var content = new Content
        {
            Parts = parts
        };

        var response = client.Models.GenerateContentAsync(
            model: "gemini-3.1-flash-lite", 
            contents: content,
            config: new GenerateContentConfig
            {
                ThinkingConfig = new ThinkingConfig
                {
                    ThinkingBudget = 0
                }
            }
        ).Result;
        
        var resultText = response.Candidates?[0]?.Content?.Parts?[0].Text?.Replace("```json", "").Replace("```", "").Trim();
        return resultText;
    }

    private static string GetMimeTypeFromFilePath(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",
            _ => throw new NotSupportedException($"File extension '{extension}' is not supported.")
        };
    }
}
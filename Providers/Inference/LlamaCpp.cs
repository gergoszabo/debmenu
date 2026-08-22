using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using debmenu.Utils;

namespace debmenu.Providers.Inference;

class LlamaCpp(IHttpClientFactory httpClientFactory) : IInferenceProvider
{
    public IHttpClientFactory HttpClientFactory { get; set; } = httpClientFactory;
    public string Url { get; set; } = "http://mac.gergoszabo.name:8080/v1/chat/completions";

    public async Task<InferenceResult?> Inference(string prompt)
    {
        var httpClient = HttpClientFactory.CreateClient();
        var requestPayload = new
        {
            model = "qwen3.5-9b",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var json = JsonSerializer.Serialize(requestPayload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpResponse = await httpClient.PostAsync("http://mac.gergoszabo.name:8080/v1/chat/completions", content);
        httpResponse.EnsureSuccessStatusCode();

        var responseBody = await httpResponse.Content.ReadAsStringAsync();

        Console.WriteLine(responseBody);
        var response = JsonSerializer.Deserialize<LlamaCppResponse>(responseBody);
        return new InferenceResult(response?.Choices.FirstOrDefault()?.Message.Content, 0, 0, response?.Usage.TotalTokens ?? 0);
    }

    public async Task<InferenceResult?> Inference(string prompt, byte[] imageBytes, string mimeType)
    {
        var httpClient = HttpClientFactory.CreateClient();
        string base64Image = Convert.ToBase64String(imageBytes);
        string dataUri = $"data:{mimeType};base64,{base64Image}";

        // Open AI vision format structure
        var requestPayload = new
        {
            model = "glm-ocr",
            messages = new object[]
            {
                new { role = "user", content = prompt },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new { type = "image_url", image_url = new { url = dataUri } }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestPayload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpResponse = await httpClient.PostAsync("http://mac.gergoszabo.name:8080/v1/chat/completions", content);
        httpResponse.EnsureSuccessStatusCode();

        var responseBody = await httpResponse.Content.ReadAsStringAsync();

        Console.WriteLine(responseBody);

        var response = JsonSerializer.Deserialize<LlamaCppResponse>(responseBody);

        var text = response?.Choices.FirstOrDefault()?.Message.Content;
        var totalTokens = response?.Usage.TotalTokens ?? 0;
        var draftTokens = response?.Timinigs.DraftN ?? 0;
        var promptTokens = response?.Usage.PromptTokens ?? 0;

        var textResult = await Inference(string.Join(',', [PromptConstants.ResponseExtractTextTask, PromptConstants.ResponseStructure, PromptConstants.DateGrounding, PromptConstants.YearGrounding]));

        return new InferenceResult(textResult?.Text,
        textResult?.PromptTokenCount ?? 0 + promptTokens,
        textResult?.CandidatesTokenCount ?? 0 + draftTokens,
        textResult?.TotalTokenCount ?? 0 + totalTokens);
    }
}

class LlamaCppResponse
{
    [JsonPropertyName("choices")]
    public required LlamaCppResponseChoice[] Choices { get; set; }
    [JsonPropertyName("usage")]
    public required LlamaCppResponseUsage Usage { get; set; }
    [JsonPropertyName("timings")]
    public required LlamaCppResponseTimings Timinigs { get; set; }
}

class LlamaCppResponseChoice
{
    [JsonPropertyName("finish_reason")]
    public required string FinishReason { get; set; }
    [JsonPropertyName("index")]
    public int Index { get; set; }
    [JsonPropertyName("message")]
    public required LlamaCppResponseChoiceMessage Message { get; set; }
}

class LlamaCppResponseChoiceMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; set; }
    [JsonPropertyName("content")]
    public required string Content { get; set; }
    [JsonPropertyName("reasoning_content")]
    public string? ReasoningContent { get; set; }
}

class LlamaCppResponseUsage
{
    [JsonPropertyName("total_tokens")]
    public required int TotalTokens { get; set; }
    [JsonPropertyName("prompt_tokens")]
    public required int PromptTokens { get; set; }
}

class LlamaCppResponseTimings
{
    [JsonPropertyName("draft_n")]
    public int? DraftN { get; set; }
}
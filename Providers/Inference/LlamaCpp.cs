using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace debmenu.Providers.Inference;

public class LlamaCpp : IInferenceProvider
{
    private readonly LlamaCppOptions _options;
    private readonly HttpClient _client;
    private readonly List<Part> _parts = [];

    public LlamaCpp(IOptions<LlamaCppOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _client = httpClientFactory.CreateClient();
        _client.BaseAddress = new Uri(_options.BaseUrl);
        _client.Timeout = TimeSpan.FromMinutes(5);   // 27B model can be slow; speed is not a concern
        if (!string.IsNullOrEmpty(_options.ApiKey))
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public void AddContent(string content) => _parts.Add(new Part { Type = "text", Text = content });

    public void AddImage(byte[] imageBytes, string mimeType) => _parts.Add(new Part
    {
        Type = "image_url",
        ImageUrl = new() { Url = $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}" }
    });

    public async Task<InferenceResult?> Inference()
    {
        var request = new ChatRequest
        {
            Model = _options.Model,
            Temperature = _options.Temperature,
            TopP = _options.TopP,
            TopK = _options.TopK,
            MinP = _options.MinP,
            PresencePenalty = _options.PresencePenalty,
            RepetitionPenalty = _options.RepetitionPenalty,
            MaxTokens = _options.MaxTokens,
            ChatTemplateKwargs = new() { EnableThinking = false },
            Messages = [new Message { Role = "user", Content = _parts }]
        };

        const int maxAttempts = 3;
        var delay = TimeSpan.FromSeconds(20);   // fixed, no backoff
        ChatResponse? body = null;
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                using var response = await _client.PostAsJsonAsync("/v1/chat/completions", request);
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadFromJsonAsync<ChatResponse>();
                break;
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(delay);
            }
        }

        _parts.Clear();

        if (body is null)
            throw new Exception("llama.cpp request failed after all attempts.");

        var text = body.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrEmpty(text))
            throw new Exception("No text content found in the llama.cpp response.");

        return new InferenceResult(
            text!.Replace("```json", "").Replace("```", "").Trim(),   // same cleanup Gemini does
            body.Usage?.PromptTokens ?? 0,
            body.Usage?.CompletionTokens ?? 0,
            body.Usage?.TotalTokens ?? 0);
    }

    private sealed class Part
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "text";
        [JsonPropertyName("text")] public string? Text { get; set; }
        [JsonPropertyName("image_url")] public ImageUrl? ImageUrl { get; set; }
    }

    private sealed class ImageUrl
    {
        [JsonPropertyName("url")] public string Url { get; set; } = "";
    }

    private sealed class Message
    {
        [JsonPropertyName("role")] public string Role { get; set; } = "user";
        [JsonPropertyName("content")] public object? Content { get; set; }   // array of Part
    }

    private sealed class ChatRequest
    {
        [JsonPropertyName("model")] public string Model { get; set; } = "";
        [JsonPropertyName("messages")] public Message[] Messages { get; set; } = [];
        [JsonPropertyName("temperature")] public double Temperature { get; set; }
        [JsonPropertyName("top_p")] public double TopP { get; set; }
        [JsonPropertyName("top_k")] public int TopK { get; set; }
        [JsonPropertyName("min_p")] public double MinP { get; set; }
        [JsonPropertyName("presence_penalty")] public double PresencePenalty { get; set; }
        [JsonPropertyName("repetition_penalty")] public double RepetitionPenalty { get; set; }
        [JsonPropertyName("max_tokens")] public int? MaxTokens { get; set; }
        [JsonPropertyName("chat_template_kwargs")] public ChatTemplateKwargs? ChatTemplateKwargs { get; set; }
    }

    private sealed class ChatTemplateKwargs
    {
        [JsonPropertyName("enable_thinking")] public bool EnableThinking { get; set; }
    }

    private sealed class ChatResponse
    {
        [JsonPropertyName("choices")] public Choice[]? Choices { get; set; }
        [JsonPropertyName("usage")] public Usage? Usage { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")] public MessageOut Message { get; set; } = new();
    }

    private sealed class MessageOut
    {
        [JsonPropertyName("content")] public string? Content { get; set; }
    }

    private sealed class Usage
    {
        [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; }
        [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; set; }
        [JsonPropertyName("total_tokens")] public int TotalTokens { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace debmenu.Providers.Inference;

/// <summary>
/// Configuration for the llama.cpp inference provider (OpenAI-compatible server).
/// </summary>
public class LlamaCppOptions
{
    /// <summary>The base URL of the llama.cpp server.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "LlamaCpp BaseUrl is missing or empty in configuration.")]
    public required string BaseUrl { get; set; }

    /// <summary>The model identifier exposed by the server.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "LlamaCpp Model is missing or empty in configuration.")]
    public required string Model { get; set; }

    /// <summary>Sampling temperature.</summary>
    public double Temperature { get; set; }

    /// <summary>Nucleus sampling probability threshold.</summary>
    public double TopP { get; set; }

    /// <summary>Top-K sampling parameter.</summary>
    public int TopK { get; set; }

    /// <summary>Minimum token probability (relative to the most likely token).</summary>
    public double MinP { get; set; }

    /// <summary>Penalty applied to tokens already present in the prompt.</summary>
    public double PresencePenalty { get; set; }

    /// <summary>Penalty applied based on a token's frequency in the context.</summary>
    public double RepetitionPenalty { get; set; }

    /// <summary>Maximum number of tokens to generate, if bounded.</summary>
    public int? MaxTokens { get; set; }

    /// <summary>The API key expected by the server (may be a placeholder for local use).</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "LlamaCpp ApiKey is missing or empty in configuration.")]
    public required string ApiKey { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace debmenu.Providers.Inference;

/// <summary>
/// Configuration for the Gemini inference provider.
/// </summary>
public class GeminiOptions
{
    /// <summary>The Gemini API key.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Gemini API Key is missing or empty in configuration.")]
    public required string ApiKey { get; set; }

    /// <summary>The model identifier to use (e.g. <c>gemini-2.0-flash</c>).</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Gemini Model is missing or empty in configuration.")]
    public required string Model { get; set; }
}

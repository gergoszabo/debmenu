using System.ComponentModel.DataAnnotations;

namespace debmenu.Providers.Inference;

public class GeminiOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Gemini API Key is missing or empty in configuration.")]
    public required string ApiKey { get; set; } = string.Empty;
    public required string Model { get; set; } = "gemini-3.1-flash-lite";
}

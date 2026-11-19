using System.ComponentModel.DataAnnotations;

namespace debmenu.Providers.Inference;

public class LlamaCppOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "LlamaCpp BaseUrl is missing or empty in configuration.")]
    public required string BaseUrl { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "LlamaCpp Model is missing or empty in configuration.")]
    public required string Model { get; set; }
    public double Temperature { get; set; }
    public double TopP { get; set; }
    public int TopK { get; set; }
    public double MinP { get; set; }
    public double PresencePenalty { get; set; }
    public double RepetitionPenalty { get; set; }
    public int? MaxTokens { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "LlamaCpp ApiKey is missing or empty in configuration.")]
    public required string ApiKey { get; set; }
}

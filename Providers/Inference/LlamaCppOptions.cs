using System.ComponentModel.DataAnnotations;

namespace debmenu.Providers.Inference;

public class LlamaCppOptions
{
    public required string BaseUrl { get; set; }
    public required string Model { get; set; }
    public double Temperature { get; set; }
    public double TopP { get; set; }
    public int TopK { get; set; }
    public double MinP { get; set; }
    public double PresencePenalty { get; set; }
    public double RepetitionPenalty { get; set; }
    public int? MaxTokens { get; set; }
    public required string ApiKey { get; set; }
}

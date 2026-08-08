using System.ComponentModel.DataAnnotations;

namespace debmenu.Providers.Infrastructure;

public class AWSOptions
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "SecretAccessKeyId is missing or empty in configuration.")]
    public required string SecretAccessKeyId { get; init; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "SecretAccessKey is missing or empty in configuration.")]
    public required string SecretAccessKey { get; init; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Region is missing or empty in configuration.")]
    public required string Region { get; init; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Bucket is missing or empty in configuration.")]
    public required string Bucket { get; init; }
}
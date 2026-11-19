using System.ComponentModel.DataAnnotations;

namespace debmenu.Providers.Infrastructure;

/// <summary>
/// Configuration for the AWS S3 infrastructure provider.
/// </summary>
public class AWSOptions
{
    /// <summary>The AWS access key ID.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SecretAccessKeyId is missing or empty in configuration.")]
    public required string SecretAccessKeyId { get; init; }

    /// <summary>The AWS secret access key.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "SecretAccessKey is missing or empty in configuration.")]
    public required string SecretAccessKey { get; init; }

    /// <summary>The AWS region system name (e.g. <c>eu-central-1</c>).</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Region is missing or empty in configuration.")]
    public required string Region { get; init; }

    /// <summary>The S3 bucket to upload the menu page into.</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Bucket is missing or empty in configuration.")]
    public required string Bucket { get; init; }
}
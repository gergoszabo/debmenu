using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using debmenu.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace debmenu.Providers.Infrastructure;

public class AWS(IOptions<AWSOptions> options, ILogger logger) : IInfrastructureProvider
{
    private AWSOptions AwsOptions { get; } = options.Value;
    private ILogger Logger { get; } = logger;

    public async Task Upload(string content, string fileName)
    {
        using var op = new TimedOperation("AWS Upload {fileName}", [fileName], Logger);
        var regionEndpoint = RegionEndpoint.EnumerableAllRegions.Single(region => region.SystemName == AwsOptions.Region);
        var s3Client = new AmazonS3Client(new BasicAWSCredentials(AwsOptions.SecretAccessKeyId, AwsOptions.SecretAccessKey), regionEndpoint);

        var putRequest = new PutObjectRequest
        {
            BucketName = AwsOptions.Bucket,
            Key = fileName,
            ContentType = "text/html",
            ContentBody = content
        };

        var response = await s3Client.PutObjectAsync(putRequest);
        Logger.Information("AWS Uplaod {HttpStatusCode} {RequestId}", response.HttpStatusCode, response.ResponseMetadata.RequestId);
    }
}

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
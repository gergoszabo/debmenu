using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using debmenu.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace debmenu.Providers.Infrastructure;

#pragma warning disable CA1812
internal sealed class AWS(IOptions<AWSOptions> options, ILogger logger) : IInfrastructureProvider
{
    private AWSOptions AwsOptions { get; } = options.Value;
    private ILogger Logger { get; } = logger;

    public async Task Upload(string content, string fileName)
    {
        using var op = new TimedOperation("AWS Upload {fileName}", [fileName], Logger);
        var regionEndpoint = RegionEndpoint.EnumerableAllRegions.Single(region => region.SystemName == AwsOptions.Region);
        using var s3Client = new AmazonS3Client(new BasicAWSCredentials(AwsOptions.SecretAccessKeyId, AwsOptions.SecretAccessKey), regionEndpoint);

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
#pragma warning restore CA1812

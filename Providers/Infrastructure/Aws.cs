using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using debmenu.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace debmenu.Providers.Infrastructure;

/// <summary>
/// Uploads the generated menu page to an S3 bucket.
/// </summary>
public class AWS : IInfrastructureProvider, IDisposable
{
    private readonly AWSOptions _options;
    private readonly ILogger _logger;
    private readonly AmazonS3Client _s3Client;

    /// <summary>
    /// Initializes a new instance with an S3 client held for the app lifetime.
    /// </summary>
    /// <param name="options">AWS connection settings (region, bucket, credentials).</param>
    /// <param name="logger">Logger used for operation logging.</param>
    public AWS(IOptions<AWSOptions> options, ILogger logger)
    {
        _options = options.Value;
        _logger = logger;
        _s3Client = CreateS3Client(_options);
    }

    private static AmazonS3Client CreateS3Client(AWSOptions options)
    {
        var regionEndpoint = RegionEndpoint.EnumerableAllRegions.Single(region => region.SystemName == options.Region);
        // Held for the app lifetime (the upload runs exactly once) and disposed on Dispose().
        return new AmazonS3Client(
            new BasicAWSCredentials(options.SecretAccessKeyId, options.SecretAccessKey),
            regionEndpoint);
    }

    /// <summary>
    /// Uploads the given content to the configured S3 bucket under the specified key.
    /// </summary>
    /// <param name="content">The HTML content to upload.</param>
    /// <param name="fileName">The object key (file name) in the bucket.</param>
    /// <returns>A task representing the asynchronous upload operation.</returns>
    public async Task Upload(string content, string fileName)
    {
        using var op = _logger.CreateTimedOperation(GetType(), nameof(Upload), fileName);

        var putRequest = new PutObjectRequest
        {
            BucketName = _options.Bucket,
            Key = fileName,
            ContentType = "text/html",
            ContentBody = content
        };

        var response = await _s3Client.PutObjectAsync(putRequest);
        _logger.Information("AWS Upload {HttpStatusCode} {RequestId}", response.HttpStatusCode, response.ResponseMetadata.RequestId);
    }

    /// <summary>
    /// Disposes the underlying S3 client.
    /// </summary>
    public void Dispose()
    {
        _s3Client.Dispose();
    }
}

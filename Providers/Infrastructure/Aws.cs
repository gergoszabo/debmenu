using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using debmenu.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace debmenu.Providers.Infrastructure;

public class AWS : IInfrastructureProvider, IDisposable
{
    private readonly AWSOptions _options;
    private readonly ILogger _logger;
    private readonly AmazonS3Client _s3Client;

    public AWS(IOptions<AWSOptions> options, ILogger logger)
    {
        _options = options.Value;
        _logger = logger;
        var regionEndpoint = RegionEndpoint.EnumerableAllRegions.Single(region => region.SystemName == _options.Region);
        // Held for the app lifetime (the upload runs exactly once) and disposed on Dispose().
        _s3Client = new AmazonS3Client(
            new BasicAWSCredentials(_options.SecretAccessKeyId, _options.SecretAccessKey),
            regionEndpoint);
    }

    public async Task Upload(string content, string fileName)
    {
        using var op = new TimedOperation("[{Class}] Upload {fileName}", [GetType().Name, fileName], _logger);

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

    public void Dispose()
    {
        _s3Client.Dispose();
    }
}

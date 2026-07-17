using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace debmenu;

internal class AWsS(string secretAccessKeyId, string secretAccessKey)
{
    public async Task UploadToS3Bucket(string content)
    {
        var s3Client = new AmazonS3Client(new BasicAWSCredentials(secretAccessKeyId, secretAccessKey), RegionEndpoint.EUCentral2);
        var bucketName = "debmenuaws";
        var keyName = "index.html";

        var putRequest = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = keyName,
            ContentType = "text/html",
            ContentBody = content
        };

        await s3Client.PutObjectAsync(putRequest);
    }
}
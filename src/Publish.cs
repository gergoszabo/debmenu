
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

public class Publish
{
    public static void UploadToS3()
    {
        try
        {
            string? accessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            string? secretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

            ArgumentNullException.ThrowIfNullOrWhiteSpace(accessKeyId);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(secretAccessKey);

            var client = new AmazonS3Client(accessKeyId, secretAccessKey, RegionEndpoint.EUCentral2);

            var directory = "results";
            var files = Directory.GetFiles(directory);

            foreach (var file in files)
            {
                // results/index.html
                var name = file.Split('/')[1];
                var start = DateTime.Now;
                Console.WriteLine($"Uploading {name}");

                var putObjectRequest = new PutObjectRequest()
                {
                    BucketName = "debmenuaws",
                    Key = name,
                    ContentType = "text/html",
                    ContentBody = File.ReadAllText(file)
                };

                Console.WriteLine(putObjectRequest.ContentBody.Length);

                client.PutObjectAsync(putObjectRequest).GetAwaiter().GetResult();

                Console.WriteLine($"Uploaded {name} in {DateTime.Now.Subtract(start).TotalMilliseconds:n0} ms");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}

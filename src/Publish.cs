
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

public class Publish
{
    public static void CheckEnvVariables()
    {
        try
        {
            var skipPublish = Environment.GetCommandLineArgs().Any(a => a == "--skip-publish");
            if (skipPublish)
            {
                return;
            }

            ArgumentNullException.ThrowIfNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Environment.Exit(-1);
        }
    }

    public static void UploadToS3()
    {
        try
        {
            var skipPublish = Environment.GetCommandLineArgs().Any(a => a == "--skip-publish");
            if (skipPublish)
            {
                Console.WriteLine("--skip-publish flag provided, skipping publish.");
                return;
            }

            string? AWS_ACCESS_KEY_ID = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            string? AWS_SECRET_ACCESS_KEY = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");

            ArgumentNullException.ThrowIfNullOrWhiteSpace(AWS_ACCESS_KEY_ID);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(AWS_SECRET_ACCESS_KEY);

            var client = new AmazonS3Client(AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, RegionEndpoint.EUCentral2);

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

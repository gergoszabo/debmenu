using System.Text;
using MailKit.Net.Smtp;
using MimeKit;

public class EmailResults
{
    public static void CheckEnvVariables()
    {
        try
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(Environment.GetEnvironmentVariable("EMAIL_FROM"));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(Environment.GetEnvironmentVariable("EMAIL_TO"));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(Environment.GetEnvironmentVariable("EMAIL_SERVER"));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(Environment.GetEnvironmentVariable("EMAIL_USERNAME"));
            ArgumentNullException.ThrowIfNullOrWhiteSpace(Environment.GetEnvironmentVariable("EMAIL_PASSWORD"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Environment.Exit(-1);
        }
    }

    public static void SendScrapeResults(IScrapeResult[] results)
    {
        var skipEmail = Environment.GetCommandLineArgs().Any(a => a == "--skip-email");
        if (skipEmail)
        {
            Console.WriteLine("--skip-email flag provided, skipping email.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("<ul>");

        var ERROR_TYPE = new FailedScraperResult()
        {
            Name = "y",
            ErrorMessage = "y"
        }.GetType().ToString();

        foreach (var result in results)
        {
            var mark = result.GetType().ToString() != ERROR_TYPE ? "✅" : "❌";
            sb.AppendLine($"<li>{result.Name}: {mark} ({result.Elapsed} ms)</li>");
        }
        sb.AppendLine("</ul>");

        Console.WriteLine(sb);

        var EMAIL_FROM = Environment.GetEnvironmentVariable("EMAIL_FROM");
        var EMAIL_TO = Environment.GetEnvironmentVariable("EMAIL_TO");
        var EMAIL_SERVER = Environment.GetEnvironmentVariable("EMAIL_SERVER");
        var EMAIL_USERNAME = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
        var EMAIL_PASSWORD = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
        ArgumentNullException.ThrowIfNullOrWhiteSpace(EMAIL_FROM);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(EMAIL_TO);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(EMAIL_SERVER);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(EMAIL_USERNAME);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(EMAIL_PASSWORD);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(EMAIL_FROM, EMAIL_FROM));
        message.To.Add(new MailboxAddress(EMAIL_TO, EMAIL_TO));
        message.Subject = "deb-menu results";
        message.Body = new TextPart("html")
        {
            Text = sb.ToString()
        };

        using var client = new SmtpClient();
        client.Connect(EMAIL_SERVER, 587, false);
        client.Authenticate(EMAIL_USERNAME, EMAIL_PASSWORD);
        Console.WriteLine($"Sending email");

        var response = client.Send(message);

        Console.WriteLine($"Mail sent {response}");
        client.Disconnect(true);
    }
}

using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using debmenu.Logging;
using debmenu.Providers.Infrastructure;
using debmenu.Utils;
using Serilog;

namespace debmenu;

public class Orchestrator(IDataCollector dataCollector,
    IInfrastructureProvider infrastructureProvider,
    ILogger logger) : IOrchestrator
{
    public async Task RunAsync()
    {
        using var op = new TimedOperation("[{Class}] RunAsync", [GetType().Name], logger);

        string? version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var offers = await dataCollector.CollectOffers();

        var offersJson = JsonSerializer.Serialize(offers, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        var generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

        var htmlContent = Html.Template
            .Replace("JSON_HERE", offersJson)
            .Replace("VVV", version)
            .Replace("GENERATED_AT", generatedAt);

        File.WriteAllText("index.html", htmlContent);

        await infrastructureProvider.Upload(htmlContent, "index.html");
    }
}

using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using debmenu.Logging;
using debmenu.Providers.Infrastructure;
using debmenu.Utils;
using Serilog;

namespace debmenu;

/// <summary>
/// Collects offers from all restaurants, renders them into a static HTML page and publishes it.
/// </summary>
public class Orchestrator(IDataCollector dataCollector,
    IInfrastructureProvider infrastructureProvider,
    ILogger logger) : IOrchestrator
{
    /// <summary>
    /// Collects offers from all restaurants, renders the menu into an HTML page and uploads it to the infrastructure provider.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
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

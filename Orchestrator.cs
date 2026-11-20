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
    IPathProvider pathProvider,
    ILogger logger) : IOrchestrator
{
    /// <summary>
    /// Collects offers from all restaurants, renders the menu into an HTML page and uploads it to the infrastructure provider.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RunAsync()
    {
        using var op = logger.CreateTimedOperation(GetType(), nameof(RunAsync));

        string? version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var offers = await dataCollector.CollectOffers();

        var offersJson = JsonSerializer.Serialize(offers, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        var generatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

        var htmlContent = Html.Template
            .Replace("__OFFERS_JSON__", offersJson)
            .Replace("__VERSION__", version)
            .Replace("__GENERATED_AT__", generatedAt);

        var outputPath = pathProvider.Combine("index.html");
        File.WriteAllText(outputPath, htmlContent);

        await infrastructureProvider.Upload(htmlContent, Path.GetFileName(outputPath));
    }
}

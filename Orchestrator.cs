using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using debmenu.Logging;
using debmenu.Providers.Infrastructure;
using debmenu.Utils;
using Serilog;

namespace debmenu;

public class Orchestrator(DataCollector dataCollector,
    IInfrastructureProvider infrastructureProvider,
    ILogger logger)
{
    private DataCollector DataCollector { get; } = dataCollector;
    private IInfrastructureProvider InfrastructureProvider { get; } = infrastructureProvider;
    private ILogger Logger { get; } = logger;

    public async Task RunAsync()
    {
        using var op = new TimedOperation("Orchestrator RunAsync", [], Logger);

        string? version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        
        var offers = await DataCollector.CollectOffers();

        var offersJson = JsonSerializer.Serialize(offers, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        var htmlContent = Html.Template.Replace("JSON_HERE", offersJson).Replace("VVV", version);

        File.WriteAllText("index.html", htmlContent);

        // await InfrastructureProvider.Upload(htmlContent, "index.html");
    }
}

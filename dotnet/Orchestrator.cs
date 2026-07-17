using System.Text.Encodings.Web;
using System.Text.Json;
using debmenu.Logging;
using debmenu.Providers.Infrastructure;
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
        using var op = new TimedOperation("DataCollector UploadResult", [], Logger);

        var offers = await DataCollector.CollectOffers();

        var offersJson = JsonSerializer.Serialize(offers, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

        var htmlContent = Html.Template.Replace("JSON_HERE", offersJson);

        await InfrastructureProvider.Upload(htmlContent, "index.html");
    }
}

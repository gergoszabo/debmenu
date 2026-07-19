using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using debmenu.Logging;
using debmenu.Providers.Infrastructure;
using debmenu.Utils;
using Serilog;

namespace debmenu;

#pragma warning disable CA1812
internal sealed class Orchestrator(DataCollector dataCollector,
    IInfrastructureProvider infrastructureProvider,
    ILogger logger)
{
    private DataCollector DataCollector { get; } = dataCollector;
    private IInfrastructureProvider InfrastructureProvider { get; } = infrastructureProvider;
    private ILogger Logger { get; } = logger;

    private static JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public async Task RunAsync()
    {
        using var op = new TimedOperation("Orchestrator RunAsync", [], Logger);

        string? version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var offers = await DataCollector.CollectOffers();

        string offersJson = JsonSerializer.Serialize(offers, jsonSerializerOptions);

        string htmlContent = Html.Template.Replace("JSON_HERE", offersJson, StringComparison.InvariantCulture).Replace("VVV", version, StringComparison.InvariantCulture);

        await File.WriteAllTextAsync("index.html", htmlContent);

        // await InfrastructureProvider.Upload(htmlContent, "index.html");
    }
}
#pragma warning restore CA1812

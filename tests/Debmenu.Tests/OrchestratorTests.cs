using System.Reflection;
using debmenu;
using debmenu.Providers.Infrastructure;
using NSubstitute;
using Serilog;

namespace Debmenu.Tests;

public class OrchestratorTests
{
    private readonly IDataCollector _dataCollector = Substitute.For<IDataCollector>();
    private readonly IInfrastructureProvider _infra = Substitute.For<IInfrastructureProvider>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public async Task RunAsync_WritesIndexHtml_ReplacingJsonAndVersion()
    {
        var offers = new Dictionary<string, Dictionary<string, List<string>>>
        {
            ["Forest"] = new() { ["2026-08-10"] = ["Rántott hús"] }
        };
        _dataCollector.CollectOffers().Returns(new OffersCollection(offers, 0, 0, 0));

        var orchestrator = new Orchestrator(_dataCollector, _infra, _logger);
        await orchestrator.RunAsync();

        Assert.True(File.Exists("index.html"));
        var content = await File.ReadAllTextAsync("index.html");

        Assert.DoesNotContain("JSON_HERE", content);
        Assert.DoesNotContain("VVV", content);
        Assert.Contains("Rántott hús", content);
        Assert.Contains("2026-08-10", content);
    }

    [Fact]
    public async Task RunAsync_UsesInformationalVersion()
    {
        _dataCollector.CollectOffers().Returns(new OffersCollection([], 0, 0, 0));

        var version = typeof(Orchestrator).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        var orchestrator = new Orchestrator(_dataCollector, _infra, _logger);
        await orchestrator.RunAsync();

        var content = await File.ReadAllTextAsync("index.html");
        Assert.Contains(version!, content);
    }

    [Fact]
    public async Task RunAsync_CallsUploadWithHtmlAndIndexFileName()
    {
        _dataCollector.CollectOffers().Returns(new OffersCollection([], 0, 0, 0));

        var orchestrator = new Orchestrator(_dataCollector, _infra, _logger);
        await orchestrator.RunAsync();

        await _infra.Received(1).Upload(Arg.Is<string>(c => c.Contains("DebMenu")), Arg.Is("index.html"));
    }
}
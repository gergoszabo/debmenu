using System.Reflection;
using debmenu.Providers.Infrastructure;
using Serilog;

namespace debmenu.Tests;

/// <summary>
/// Tests for <see cref="Orchestrator.RunAsync"/>: HTML rendering, file output and upload.
/// </summary>
[TestClass]
public class OrchestratorTests : TestContextBase
{
    private sealed class FakePathProvider(string root) : IPathProvider
    {
        public string Combine(params string[] segments)
        {
            var path = Path.Combine([root, .. segments]);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            return path;
        }
    }

    private sealed class FakeDataCollector(Dictionary<string, Dictionary<string, List<string>>> offers) : IDataCollector
    {
        public Task<Dictionary<string, Dictionary<string, List<string>>>> CollectOffers()
            => Task.FromResult(offers);
    }

    private sealed class FakeInfrastructureProvider : IInfrastructureProvider
    {
        public string? UploadedContent { get; private set; }
        public string? UploadedFileName { get; private set; }

        public Task Upload(string content, string fileName)
        {
            UploadedContent = content;
            UploadedFileName = fileName;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task RunAsync_WritesIndexHtml_WithOffersJson_AndUploadsIt()
    {
        // Arrange
        var offers = new Dictionary<string, Dictionary<string, List<string>>>
        {
            ["Govinda"] = new() { ["2026-11-23"] = ["Krémleves", "Lecsó"] },
        };
        var infrastructure = new FakeInfrastructureProvider();
        var orchestrator = new Orchestrator(
            new FakeDataCollector(offers), infrastructure, new FakePathProvider(_root), Log.Logger);

        // Act
        await orchestrator.RunAsync();

        // Assert
        var outputPath = Path.Combine(_root, "index.html");
        File.Exists(outputPath).Should().BeTrue();
        var html = File.ReadAllText(outputPath);
        html.Should().Contain("Krémleves").And.Contain("Govinda");
        html.Should().NotContain("__OFFERS_JSON__").And.NotContain("__VERSION__").And.NotContain("__GENERATED_AT__");
        infrastructure.UploadedContent.Should().Be(html);
        infrastructure.UploadedFileName.Should().Be("index.html");
    }

    [TestMethod]
    public async Task RunAsync_IncludesAssemblyVersion_AndGeneratedAtTimestamp()
    {
        // Arrange: the version under test comes from the main assembly (debmenu.dll),
        // not the test assembly, because Orchestrator uses GetExecutingAssembly().
        var mainAssembly = typeof(Orchestrator).Assembly;
        var version = mainAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        var infrastructure = new FakeInfrastructureProvider();
        var orchestrator = new Orchestrator(
            new FakeDataCollector(new Dictionary<string, Dictionary<string, List<string>>>()),
            infrastructure, new FakePathProvider(_root), Log.Logger);

        // Act
        await orchestrator.RunAsync();

        // Assert
        var html = File.ReadAllText(Path.Combine(_root, "index.html"));
        html.Should().Contain(version).And.MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z");
    }

    [TestMethod]
    public async Task RunAsync_WithEmptyOffers_StillWritesPage_AndUploads()
    {
        // Arrange
        var infrastructure = new FakeInfrastructureProvider();
        var orchestrator = new Orchestrator(
            new FakeDataCollector(new Dictionary<string, Dictionary<string, List<string>>>()),
            infrastructure, new FakePathProvider(_root), Log.Logger);

        // Act
        await orchestrator.RunAsync();

        // Assert
        File.Exists(Path.Combine(_root, "index.html")).Should().BeTrue();
        infrastructure.UploadedContent.Should().NotBeNull();
    }
}

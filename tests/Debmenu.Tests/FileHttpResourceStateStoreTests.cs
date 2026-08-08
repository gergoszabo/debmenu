using debmenu.Caching;
using NSubstitute;
using Serilog;

namespace Debmenu.Tests;

public class FileHttpResourceStateStoreTests : IDisposable
{
    private readonly string _cacheDir = Path.Combine(Path.GetTempPath(), "debmenu-tests-" + Guid.NewGuid().ToString("N"));
    private readonly ILogger _logger = Substitute.For<ILogger>();

    private FileHttpResourceStateStore CreateStore() => new(_logger, _cacheDir);

    [Fact]
    public async Task GetAsync_WhenFileMissing_ReturnsNull()
    {
        var store = CreateStore();
        var result = await store.GetAsync("https://example.test/");
        Assert.Null(result);
    }

    [Fact]
    public async Task SetThenGet_RoundTripsState()
    {
        var store = CreateStore();
        var state = new HttpResourceState("etag-abc", "2026-01-01T00:00:00Z");

        await store.SetAsync("https://example.test/", state);
        var result = await store.GetAsync("https://example.test/");

        Assert.Equal(state, result);
    }

    [Fact]
    public async Task GetAsync_UnknownUrl_ReturnsNull()
    {
        var store = CreateStore();
        await store.SetAsync("https://example.test/a", new HttpResourceState("e1", null));

        var result = await store.GetAsync("https://example.test/other");

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_NullEtagAndLastModified_DoesNotCreateFile()
    {
        var store = CreateStore();
        await store.SetAsync("https://example.test/", new HttpResourceState(null, null));

        Assert.False(File.Exists(Path.Combine(_cacheDir, "http-states.json")));
    }

    [Fact]
    public async Task SetAsync_MultipleUrls_AreStoredSeparately()
    {
        var store = CreateStore();
        await store.SetAsync("https://a.test/", new HttpResourceState("ea", null));
        await store.SetAsync("https://b.test/", new HttpResourceState("eb", null));

        var a = await store.GetAsync("https://a.test/");
        var b = await store.GetAsync("https://b.test/");

        Assert.Equal("ea", a!.ETag);
        Assert.Equal("eb", b!.ETag);
    }

    public void Dispose()
    {
        TryDeleteDirectory(_cacheDir);
    }

    internal static void TryDeleteDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
    }
}
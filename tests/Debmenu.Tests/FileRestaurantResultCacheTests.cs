using debmenu.Caching;
using NSubstitute;
using Serilog;

namespace Debmenu.Tests;

public class FileRestaurantResultCacheTests : IDisposable
{
    private readonly string _cacheDir = Path.Combine(Path.GetTempPath(), "debmenu-tests-" + Guid.NewGuid().ToString("N"));
    private readonly ILogger _logger = Substitute.For<ILogger>();

    private FileRestaurantResultCache CreateCache() => new(_logger, _cacheDir);

    [Fact]
    public async Task GetAsync_WhenFileMissing_ReturnsNull()
    {
        var cache = CreateCache();
        var result = await cache.GetAsync("Forest");
        Assert.Null(result);
    }

    [Fact]
    public async Task SetThenGet_RoundTripsOffers()
    {
        var cache = CreateCache();
        var offers = new Dictionary<string, List<string>>
        {
            ["2026-08-10"] = ["Menu A", "Menu B"]
        };

        await cache.SetAsync("Forest", offers);
        var result = await cache.GetAsync("Forest");

        Assert.NotNull(result);
        Assert.Equal(offers, result);
    }

    [Fact]
    public async Task GetAsync_UnknownRestaurant_ReturnsNull()
    {
        var cache = CreateCache();
        await cache.SetAsync("Forest", new Dictionary<string, List<string>> { ["2026-08-10"] = ["x"] });

        var result = await cache.GetAsync("Viktoria");

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ReplacesExistingEntry()
    {
        var cache = CreateCache();
        var first = new Dictionary<string, List<string>> { ["2026-08-10"] = ["old"] };
        var second = new Dictionary<string, List<string>> { ["2026-08-11"] = ["new"] };

        await cache.SetAsync("Forest", first);
        await cache.SetAsync("Forest", second);

        var result = await cache.GetAsync("Forest");
        Assert.Equal(second, result);
    }

    public void Dispose()
    {
        if (Directory.Exists(_cacheDir))
            Directory.Delete(_cacheDir, recursive: true);
    }
}
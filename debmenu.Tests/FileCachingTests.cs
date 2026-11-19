using debmenu;
using debmenu.Caching;
using Serilog;

namespace debmenu.Tests;

/// <summary>
/// Tests for the file-backed cache classes (<see cref="FileHttpResourceStateStore"/>,
/// <see cref="FileRestaurantResultCache"/>) using a per-class temp directory.
/// </summary>
[TestClass]
public class FileCachingTests : TestContextBase
{
    private sealed class FakePathProvider(string root) : IPathProvider
    {
        public string Combine(params string[] segments) => Path.Combine([root, .. segments]);
    }

    // ---------- FileHttpResourceStateStore ----------

    [TestMethod]
    public async Task StateStore_GetAsync_WhenNoFile_ReturnsNull()
    {
        // Arrange
        var store = CreateStateStore();

        // Act
        var result = await store.GetAsync("https://example.test");

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task StateStore_SetAsync_ThenGetAsync_ReturnsStoredState()
    {
        // Arrange
        var store = CreateStateStore();
        const string url = "https://example.test/menu";
        var state = new HttpResourceState("etag-1", "Mon, 01 Jan 2024 00:00:00 GMT");

        // Act
        await store.SetAsync(url, state);
        var result = await store.GetAsync(url);

        // Assert
        result.Should().Be(state);
    }

    [TestMethod]
    public async Task StateStore_GetAsync_WithUnknownUrl_ReturnsNull()
    {
        // Arrange
        var store = CreateStateStore();
        await store.SetAsync("https://known.test", new HttpResourceState("e1", null));

        // Act
        var result = await store.GetAsync("https://unknown.test");

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task StateStore_SetAsync_WithBothFieldsNull_DoesNotWriteFile()
    {
        // Arrange
        var store = CreateStateStore();
        const string url = "https://example.test";

        // Act
        await store.SetAsync(url, new HttpResourceState(null, null));

        // Assert
        File.Exists(Path.Combine(_root, "Cache", "http-states.json")).Should().BeFalse();
    }

    [TestMethod]
    public async Task StateStore_SetAsync_WithOnlyLastModified_WritesFile()
    {
        // Arrange
        var store = CreateStateStore();

        // Act
        await store.SetAsync("https://example.test", new HttpResourceState(null, "lm-1"));

        // Assert
        File.Exists(Path.Combine(_root, "Cache", "http-states.json")).Should().BeTrue();
    }

    [TestMethod]
    public async Task StateStore_SetAsync_UpdatesExistingEntry()
    {
        // Arrange
        var store = CreateStateStore();
        const string url = "https://example.test";
        await store.SetAsync(url, new HttpResourceState("etag-old", null));

        // Act
        await store.SetAsync(url, new HttpResourceState("etag-new", null));
        var result = await store.GetAsync(url);

        // Assert
        result!.ETag.Should().Be("etag-new");
    }

    // ---------- FileRestaurantResultCache ----------

    [TestMethod]
    public async Task ResultCache_GetAsync_WhenNoFile_ReturnsNull()
    {
        // Arrange
        var cache = CreateResultCache();

        // Act
        var result = await cache.GetAsync("Govinda");

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task ResultCache_SetAsync_ThenGetAsync_ReturnsStoredOffers()
    {
        // Arrange
        var cache = CreateResultCache();
        var offers = new Dictionary<string, List<string>>
        {
            ["2026-11-23"] = ["Krémleves", "Lecsó"],
        };

        // Act
        await cache.SetAsync("Govinda", offers);
        var result = await cache.GetAsync("Govinda");

        // Assert
        result.Should().BeEquivalentTo(offers);
    }

    [TestMethod]
    public async Task ResultCache_GetAsync_WithUnknownRestaurant_ReturnsNull()
    {
        // Arrange
        var cache = CreateResultCache();
        await cache.SetAsync("Govinda", new Dictionary<string, List<string>> { ["d"] = ["x"] });

        // Act
        var result = await cache.GetAsync("Viktoria");

        // Assert
        result.Should().BeNull();
    }

    [TestMethod]
    public async Task ResultCache_SetAsync_KeepsOtherRestaurants()
    {
        // Arrange
        var cache = CreateResultCache();
        var offersA = new Dictionary<string, List<string>> { ["a"] = ["1"] };
        await cache.SetAsync("RestaurantA", offersA);

        // Act
        var offersB = new Dictionary<string, List<string>> { ["b"] = ["2"] };
        await cache.SetAsync("RestaurantB", offersB);
        var resultA = await cache.GetAsync("RestaurantA");
        var resultB = await cache.GetAsync("RestaurantB");

        // Assert
        resultA.Should().BeEquivalentTo(offersA);
        resultB.Should().BeEquivalentTo(offersB);
    }

    [TestMethod]
    public async Task ResultCache_SetAsync_OverwritesExistingEntry()
    {
        // Arrange
        var cache = CreateResultCache();
        await cache.SetAsync("Govinda", new Dictionary<string, List<string>> { ["old"] = ["x"] });

        // Act
        var offers = new Dictionary<string, List<string>> { ["new"] = ["y"] };
        await cache.SetAsync("Govinda", offers);
        var result = await cache.GetAsync("Govinda");

        // Assert
        result.Should().BeEquivalentTo(offers);
    }

    // ---------- helpers ----------

    private FileHttpResourceStateStore CreateStateStore()
        => new(new FakePathProvider(_root), Log.Logger);

    private FileRestaurantResultCache CreateResultCache()
        => new(new FakePathProvider(_root), Log.Logger);
}

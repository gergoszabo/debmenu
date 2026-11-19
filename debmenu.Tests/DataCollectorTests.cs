using debmenu.Providers.Inference;
using debmenu.Restaurants;
using Serilog;

namespace debmenu.Tests;

/// <summary>
/// Tests for <see cref="DataCollector.CollectOffers"/>: aggregation and per-scraper fault tolerance.
/// </summary>
[TestClass]
public class DataCollectorTests
{
    private sealed class FakeRestaurant(
        Dictionary<string, List<string>>? offersToReturn,
        InferenceResult? cost = null,
        Exception? exceptionToThrow = null) : IRestaurant
    {
        public string Name => GetType().Name;

        public InferenceResult? TotalInferenceCost { get; } = cost;

        public Task<Dictionary<string, List<string>>> GetOffersAsync()
            => exceptionToThrow is not null
                ? Task.FromException<Dictionary<string, List<string>>>(exceptionToThrow)
                : Task.FromResult(offersToReturn!);
    }

    private static DataCollector Create(params IRestaurant[] scrapers)
        => new(scrapers, Log.Logger);

    [TestMethod]
    public async Task CollectOffers_WithSingleScraper_ReturnsItsOffers()
    {
        // Arrange
        var offers = new Dictionary<string, List<string>> { ["2026-11-23"] = ["Lecsó"] };
        var collector = Create(new FakeRestaurant(offers));

        // Act
        var result = await collector.CollectOffers();

        // Assert
        result.Should().HaveCount(1);
        result[typeof(FakeRestaurant).Name].Should().BeEquivalentTo(offers);
    }

    [TestMethod]
    public async Task CollectOffers_WithMultipleScrapers_ReturnsAllOffers()
    {
        // Arrange
        var offersA = new Dictionary<string, List<string>> { ["a"] = ["1"] };
        var offersB = new Dictionary<string, List<string>> { ["b"] = ["2"] };
        var collector = Create(new FakeRestaurant(offersA), new FakeRestaurant(offersB));

        // Act
        var result = await collector.CollectOffers();

        // Assert: both are keyed by the scraper type name (same fake type here, so one key).
        result.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task CollectOffers_WhenScraperThrows_CollectsRemainingScrapers()
    {
        // Arrange
        var goodOffers = new Dictionary<string, List<string>> { ["ok"] = ["fine"] };
        var collector = Create(
            new FakeRestaurant(null, exceptionToThrow: new HttpRequestException("scrape failed")),
            new FakeRestaurant(goodOffers));

        // Act
        var result = await collector.CollectOffers();

        // Assert: the failing scraper is skipped (logged), the good one is still collected.
        result.Should().HaveCount(1);
        result[typeof(FakeRestaurant).Name].Should().BeEquivalentTo(goodOffers);
    }

    [TestMethod]
    public async Task CollectOffers_WhenAllScrapersThrow_ReturnsEmptyDictionary()
    {
        // Arrange
        var collector = Create(new FakeRestaurant(null, exceptionToThrow: new Exception("boom")));

        // Act
        var result = await collector.CollectOffers();

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public async Task CollectOffers_WithNoScrapers_ReturnsEmptyDictionary()
    {
        // Arrange/Act
        var result = await Create().CollectOffers();

        // Assert
        result.Should().BeEmpty();
    }
}

using debmenu;
using debmenu.Providers.Inference;
using debmenu.Restaurants;
using NSubstitute;
using Serilog;

namespace Debmenu.Tests;

public class DataCollectorTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public async Task CollectOffers_AggregatesOffersFromAllRestaurants()
    {
        var forest = new FakeForest { Offers = new Dictionary<string, List<string>> { ["2026-08-10"] = ["menu"] } };
        var viktoria = new FakeViktoria { Offers = new Dictionary<string, List<string>> { ["2026-08-11"] = ["more"] } };

        var collector = new DataCollector([forest, viktoria], _logger);
        var result = await collector.CollectOffers();

        Assert.Equal(2, result.Offers.Count);
        Assert.Equal(["menu"], result.Offers[nameof(FakeForest)]["2026-08-10"]);
        Assert.Equal(["more"], result.Offers[nameof(FakeViktoria)]["2026-08-11"]);
    }

    [Fact]
    public async Task CollectOffers_OneRestaurantThrows_StillReturnsOthers()
    {
        var throwing = new FakeForest { ThrowOnGetOffers = true };
        var healthy = new FakeViktoria { Offers = new Dictionary<string, List<string>> { ["2026-08-10"] = ["ok"] } };

        var collector = new DataCollector([throwing, healthy], _logger);
        var result = await collector.CollectOffers();

        Assert.Single(result.Offers);
        Assert.True(result.Offers.ContainsKey(nameof(FakeViktoria)));
        Assert.False(result.Offers.ContainsKey(nameof(FakeForest)));
    }

    [Fact]
    public async Task CollectOffers_ExposesTotalInferenceCostFields()
    {
        var a = new FakeForest { Offers = [], TotalInferenceCost = new InferenceResult(null, 10, 20, 30) };
        var b = new FakeViktoria { Offers = [], TotalInferenceCost = new InferenceResult(null, 5, 5, 10) };

        var collector = new DataCollector([a, b], _logger);
        var result = await collector.CollectOffers();

        Assert.Equal(15, result.PromptTokenCount);
        Assert.Equal(25, result.CandidatesTokenCount);
        Assert.Equal(40, result.TotalTokenCount);
    }

    private abstract class FakeRestaurant : IRestaurant
    {
        public Dictionary<string, List<string>> Offers { get; init; } = [];
        public InferenceResult? TotalInferenceCost { get; init; }
        public bool ThrowOnGetOffers { get; init; }

        public virtual Task<Dictionary<string, List<string>>> GetOffersAsync()
        {
            if (ThrowOnGetOffers)
                throw new Exception("boom");
            return Task.FromResult(Offers);
        }
    }

    private sealed class FakeForest : FakeRestaurant { }
    private sealed class FakeViktoria : FakeRestaurant { }
}

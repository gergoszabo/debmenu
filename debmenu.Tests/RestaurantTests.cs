using debmenu.Caching;
using debmenu.Providers.Inference;
using debmenu.Restaurants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Serilog;

namespace debmenu.Tests;

/// <summary>
/// Tests for <see cref="Restaurant.FilterOutdatedOffers"/>, which drops offers dated before the
/// current calendar week (Monday 00:00 UTC).
/// </summary>
[TestClass]
public class RestaurantTests
{
    private static readonly DateTime _startOfWeek =
        DateTime.UtcNow.Date.AddDays(-(DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)DateTime.UtcNow.DayOfWeek - 1));

    private sealed class TestRestaurant(
        IInferenceProvider inferenceProvider,
        IHttpClientFactory httpClientFactory,
        ILogger logger,
        IHttpResourceStateStore stateStore,
        IRestaurantResultCache resultCache)
        : Restaurant("https://example.test/menu", httpClientFactory, inferenceProvider, logger, [], stateStore, resultCache)
    {
        public new Dictionary<string, List<string>> FilterOutdatedOffers(Dictionary<string, List<string>> offers)
            => base.FilterOutdatedOffers(offers);
    }

    private static TestRestaurant CreateRestaurant()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        using var provider = services.BuildServiceProvider();
        return new TestRestaurant(
            null!,
            provider.GetRequiredService<IHttpClientFactory>(),
            Log.Logger,
            null!,
            null!);
    }

    [TestMethod]
    public void FilterOutdatedOffers_WithNullOffers_ThrowsArgumentNullException()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        Dictionary<string, List<string>>? offers = null;

        // Act
        Action act = () => restaurant.FilterOutdatedOffers(offers!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void FilterOutdatedOffers_RemovesDatesBeforeCurrentWeek()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var lastMonday = _startOfWeek.AddDays(-7);
        var offers = new Dictionary<string, List<string>>
        {
            [lastMonday.ToString("yyyy-MM-dd")] = ["Old offer"],
        };

        // Act
        var result = restaurant.FilterOutdatedOffers(offers);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void FilterOutdatedOffers_KeepsDatesInCurrentWeek()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var today = DateTime.UtcNow.Date;
        var offers = new Dictionary<string, List<string>>
        {
            [today.ToString("yyyy-MM-dd")] = ["Today's offer"],
        };

        // Act
        var result = restaurant.FilterOutdatedOffers(offers);

        // Assert
        var key = today.ToString("yyyy-MM-dd");
        result.Should().ContainKey(key);
        result[key].Should().BeEquivalentTo(["Today's offer"]);
    }

    [TestMethod]
    public void FilterOutdatedOffers_KeepsDatesInCurrentWeek_EvenBeforeNow()
    {
        // Arrange: an earlier day of the current week (still >= startOfWeek).
        var restaurant = CreateRestaurant();
        var someDayThisWeek = _startOfWeek.AddDays(Math.Min(3, (int)(DateTime.UtcNow.Date - _startOfWeek).TotalDays));
        var offers = new Dictionary<string, List<string>>
        {
            [someDayThisWeek.ToString("yyyy-MM-dd")] = ["Earlier this week"],
        };

        // Act
        var result = restaurant.FilterOutdatedOffers(offers);

        // Assert
        result.Should().HaveCount(1).And.ContainKey(someDayThisWeek.ToString("yyyy-MM-dd"));
    }

    [TestMethod]
    public void FilterOutdatedOffers_DropsUnparseableKeys()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var offers = new Dictionary<string, List<string>>
        {
            ["not-a-date"] = ["Mystery offer"],
            [""] = ["Empty key"],
        };

        // Act
        var result = restaurant.FilterOutdatedOffers(offers);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void FilterOutdatedOffers_WithMixedKeys_KeepsOnlyCurrentWeekDates()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var lastMonday = _startOfWeek.AddDays(-7);
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var offers = new Dictionary<string, List<string>>
        {
            [lastMonday.ToString("yyyy-MM-dd")] = ["Old"],
            [tomorrow.ToString("yyyy-MM-dd")] = ["Upcoming"],
            ["garbage"] = ["Invalid"],
        };

        // Act
        var result = restaurant.FilterOutdatedOffers(offers);

        // Assert
        result.Should().HaveCount(1).And.ContainKey(tomorrow.ToString("yyyy-MM-dd"));
    }
}

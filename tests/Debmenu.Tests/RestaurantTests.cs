using System.Net;
using System.Net.Http.Headers;
using debmenu.Caching;
using debmenu.Providers.Inference;
using Debmenu.Tests.Helpers;
using NSubstitute;
using Serilog;

namespace Debmenu.Tests;

public class RestaurantTests
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private static readonly DateTime StartOfWeek = Now.Date.AddDays(-(Now.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)Now.DayOfWeek - 1));

    private readonly IInferenceProvider _inference = Substitute.For<IInferenceProvider>();
    private readonly IHttpResourceStateStore _stateStore = Substitute.For<IHttpResourceStateStore>();
    private readonly IRestaurantResultCache _resultCache = Substitute.For<IRestaurantResultCache>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    private TestRestaurant CreateRestaurant(string etag = "etag-current", string? lastModified = null, string? storedEtag = null)
    {
        storedEtag ??= etag;
        var storedEtagHeader = EntityTagHeaderValue.Parse("\"" + storedEtag + "\"").ToString();
        _stateStore.GetAsync(Arg.Any<string>())
            .Returns(new HttpResourceState(storedEtagHeader, lastModified));

        var handler = new ScriptedHttpMessageHandler(request =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("body")
            };
            if (request.Method == HttpMethod.Head)
            {
                response.Headers.ETag = EntityTagHeaderValue.Parse("\"" + etag + "\"");
                if (lastModified is not null)
                    response.Content.Headers.LastModified = DateTimeOffset.Parse(lastModified);
            }
            return response;
        });

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient().Returns(new HttpClient(handler));

        return new TestRestaurant(httpClientFactory, _inference, _logger, _stateStore, _resultCache);
    }

    [Fact]
    public void FilterOutdatedOffers_KeepsCurrentAndFutureWeek()
    {
        var restaurant = CreateRestaurant();
        var offers = new Dictionary<string, List<string>>
        {
            [StartOfWeek.ToString("yyyy-MM-dd")] = ["today"],
            [Now.AddDays(2).ToString("yyyy-MM-dd")] = ["future"]
        };

        var result = restaurant.FilterOutdatedOffers(offers);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FilterOutdatedOffers_DropsPreviousWeek()
    {
        var restaurant = CreateRestaurant();
        var lastWeek = StartOfWeek.AddDays(-1).ToString("yyyy-MM-dd");

        var result = restaurant.FilterOutdatedOffers(new Dictionary<string, List<string>>
        {
            [lastWeek] = ["old"]
        });

        Assert.Empty(result);
    }

    [Fact]
    public void FilterOutdatedOffers_DropsInvalidDateKeys()
    {
        var restaurant = CreateRestaurant();
        var result = restaurant.FilterOutdatedOffers(new Dictionary<string, List<string>>
        {
            ["not-a-date"] = ["x"]
        });

        Assert.Empty(result);
    }

    [Fact]
    public void ParseInferenceResponseAsOffers_ParsesJsonAndFilters()
    {
        var restaurant = CreateRestaurant();
        var json = $@"{{""{StartOfWeek:yyyy-MM-dd}"": [""A"", ""B""], ""invalid"": [""x""]}}";

        var result = restaurant.ParseInferenceResponseAsOffers(json);

        Assert.Single(result);
        Assert.Equal(["A", "B"], result[StartOfWeek.ToString("yyyy-MM-dd")]);
    }

    [Fact]
    public void ParseInferenceResponseAsOffers_InvalidJson_Throws()
    {
        var restaurant = CreateRestaurant();
        Assert.ThrowsAny<Exception>(() => restaurant.ParseInferenceResponseAsOffers("not json"));
    }

    [Fact]
    public async Task GetOffersWithCaching_UnchangedPage_ReturnsCachedWithoutFetching()
    {
        var restaurant = CreateRestaurant(etag: "etag-current");
        var cached = new Dictionary<string, List<string>> { ["2026-08-10"] = ["cached"] };
        _resultCache.GetAsync("TestRestaurant").Returns(cached);

        var fetched = 0;
        var result = await restaurant.TestGetOffersWithCachingAsync(async () => { fetched++; return []; });

        Assert.Equal(cached, result);
        Assert.Equal(0, fetched);
    }

    [Fact]
    public async Task GetOffersWithCaching_UnchangedPage_NoCachedOffers_Refetches()
    {
        var restaurant = CreateRestaurant(etag: "etag-current");
        _resultCache.GetAsync("TestRestaurant").Returns((Dictionary<string, List<string>>?)null);

        var fetched = 0;
        var result = await restaurant.TestGetOffersWithCachingAsync(async () => { fetched++; return new Dictionary<string, List<string>> { ["2026-08-10"] = ["new"] }; });

        Assert.Equal(1, fetched);
        Assert.Equal(["new"], result["2026-08-10"]);
    }

    [Fact]
    public async Task GetOffersWithCaching_ChangedPage_Refetches()
    {
        var restaurant = CreateRestaurant(etag: "etag-new", storedEtag: "etag-different");
        _resultCache.GetAsync("TestRestaurant").Returns(new Dictionary<string, List<string>> { ["2026-08-10"] = ["stale"] });

        var fetched = 0;
        var result = await restaurant.TestGetOffersWithCachingAsync(async () => { fetched++; return new Dictionary<string, List<string>> { ["2026-08-10"] = ["fresh"] }; });

        Assert.Equal(1, fetched);
        Assert.Equal(["fresh"], result["2026-08-10"]);
    }
}
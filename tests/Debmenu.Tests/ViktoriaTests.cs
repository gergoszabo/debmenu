using System.Net;
using debmenu.Caching;
using debmenu.Providers.Inference;
using debmenu.Restaurants;
using Debmenu.Tests.Helpers;
using NSubstitute;
using Serilog;

namespace Debmenu.Tests;

public class ViktoriaTests
{
    private static DateTime Now => DateTime.UtcNow;
    private static DateTime StartOfWeek => Now.Date.AddDays(-(Now.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)Now.DayOfWeek - 1));

    private readonly IInferenceProvider _inference = Substitute.For<IInferenceProvider>();
    private readonly IHttpResourceStateStore _stateStore = Substitute.For<IHttpResourceStateStore>();
    private readonly IRestaurantResultCache _resultCache = Substitute.For<IRestaurantResultCache>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public async Task GetOffersAsync_ReturnsParsedOffers()
    {
        var viktoria = CreateViktoria("<html><body>heti menu</body></html>");
        _inference.Inference().Returns(Task.FromResult<InferenceResult?>(new InferenceResult(
            $@"{{""{StartOfWeek:yyyy-MM-dd}"": [""Csal""]}}", 1, 1, 2)));

        var offers = await viktoria.GetOffersAsync();

        Assert.Single(offers);
        Assert.Equal(["Csal"], offers[StartOfWeek.ToString("yyyy-MM-dd")]);
    }

    [Fact]
    public async Task GetOffersAsync_NoInferenceResult_Throws()
    {
        var viktoria = CreateViktoria("<html></html>");
        _inference.Inference().Returns(Task.FromResult<InferenceResult?>(null));

        await Assert.ThrowsAnyAsync<Exception>(() => viktoria.GetOffersAsync());
    }

    private Viktoria CreateViktoria(string content)
    {
        _stateStore.GetAsync(Arg.Any<string>()).Returns((HttpResourceState?)null);
        var handler = new ScriptedHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) });
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient().Returns(new HttpClient(handler));
        return new Viktoria(_inference, factory, _logger, _stateStore, _resultCache);
    }
}
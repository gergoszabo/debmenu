using System.Net;
using System.Reflection;
using debmenu.Caching;
using debmenu.Providers.Inference;
using debmenu.Restaurants;
using Debmenu.Tests.Helpers;
using NSubstitute;
using Serilog;

namespace Debmenu.Tests;

public class ForestTests
{
    private readonly IInferenceProvider _inference = Substitute.For<IInferenceProvider>();
    private readonly IHttpResourceStateStore _stateStore = Substitute.For<IHttpResourceStateStore>();
    private readonly IRestaurantResultCache _resultCache = Substitute.For<IRestaurantResultCache>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public async Task GetImageLinkFromUrl_ExtractsAbsoluteImageUrl()
    {
        var html = "<div>Heti étlap <img src=\"http://example.com/img.jpg\"></div>";
        var forest = CreateForest(html);

        var link = await InvokeGetImageLink(forest);

        Assert.StartsWith("http://", link);
        Assert.Contains("example.com/img.jpg", link);
    }

    [Fact]
    public async Task GetImageLinkFromUrl_HtmlWithoutMarker_Throws()
    {
        var forest = CreateForest("<html><body>no monthly marker here</body></html>");

        await Assert.ThrowsAnyAsync<Exception>(() => InvokeGetImageLink(forest));
    }

    private Forest CreateForest(string content)
    {
        var handler = new ScriptedHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) });
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient().Returns(new HttpClient(handler));
        return new Forest(_inference, factory, _logger, _stateStore, _resultCache);
    }

    private static async Task<string> InvokeGetImageLink(Forest forest)
    {
        var method = typeof(Forest).GetMethod("GetImageLinkFromUrl", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task<string>)method.Invoke(forest, [])!;
        return await task;
    }
}
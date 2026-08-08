using System.Net;
using System.Reflection;
using debmenu.Caching;
using debmenu.Providers.Inference;
using debmenu.Restaurants;
using Debmenu.Tests.Helpers;
using NSubstitute;
using Serilog;

namespace Debmenu.Tests;

public class GovindaTests
{
    private readonly IInferenceProvider _inference = Substitute.For<IInferenceProvider>();
    private readonly IHttpResourceStateStore _stateStore = Substitute.For<IHttpResourceStateStore>();
    private readonly IRestaurantResultCache _resultCache = Substitute.For<IRestaurantResultCache>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public async Task GetImageLinkFromUrl_PrefixesRelativeLinkWithBaseUrl()
    {
        var govinda = CreateGovinda("<html><img src='logo.jpg'></html>");
        _inference.Inference().Returns(Task.FromResult<InferenceResult?>(new InferenceResult("/uploads/menu.jpg", 0, 0, 0)));

        var link = await InvokeGetImageLink(govinda);

        Assert.Equal("https://www.govindadebrecen.hu//uploads/menu.jpg", link);
    }

    [Fact]
    public async Task GetImageLinkFromUrl_NoInferenceResult_Throws()
    {
        var govinda = CreateGovinda("<html></html>");
        _inference.Inference().Returns(Task.FromResult<InferenceResult?>(null));

        await Assert.ThrowsAsync<ArgumentNullException>(() => InvokeGetImageLink(govinda));
    }

    private Govinda CreateGovinda(string content)
    {
        var handler = new ScriptedHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) });
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient().Returns(new HttpClient(handler));
        return new Govinda(_inference, factory, _logger, _stateStore, _resultCache);
    }

    private static async Task<string> InvokeGetImageLink(Govinda govinda)
    {
        var method = typeof(Govinda).GetMethod("GetImageLinkFromUrl", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task<string>)method.Invoke(govinda, [])!;
        return await task;
    }
}
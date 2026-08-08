using System.Net;
using System.Reflection;
using System.Text.Json;
using debmenu.Caching;
using debmenu.Providers.Inference;
using debmenu.Restaurants;
using Debmenu.Tests.Helpers;
using NSubstitute;
using Serilog;

namespace Debmenu.Tests;

public class MannaTests
{
    private readonly IInferenceProvider _inference = Substitute.For<IInferenceProvider>();
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly IHttpResourceStateStore _stateStore = Substitute.For<IHttpResourceStateStore>();
    private readonly IRestaurantResultCache _resultCache = Substitute.For<IRestaurantResultCache>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public void ParseInferenceResponse_NoHetiMenu_Throws()
    {
        var manna = CreateManna();
        var json = JsonSerializer.Serialize(new MannaMenuItem[]
        {
            new() { name = "Napi menü", products = [] }
        });

        Assert.Throws<MannaHetiMenuNotFoundException>(() => InvokeParse(manna, json));
    }

    [Fact]
    public void ParseInferenceResponse_InvalidJson_Throws()
    {
        var manna = CreateManna();
        Assert.Throws<JsonException>(() => InvokeParse(manna, "not-json"));
    }

    [Fact]
    public void ParseInferenceResponse_GroupsProductsByExpectedAt_AndSkipsNull()
    {
        var manna = CreateManna();
        var date1 = new DateTime(2026, 8, 10);
        var json = JsonSerializer.Serialize(new MannaMenuItem[]
        {
            new()
            {
                name = "Heti menü",
                products =
                [
                    new() { name = "a", description = "Soup", expected_at = date1 },
                    new() { name = "b", description = "Main", expected_at = date1 },
                    new() { name = "c", description = "No date", expected_at = null }
                ]
            }
        });

        var result = InvokeParse(manna, json);

        Assert.Single(result);
        Assert.Equal(2, result["2026-08-10"].Count);
        Assert.Contains("Soup", result["2026-08-10"]);
        Assert.Contains("Main", result["2026-08-10"]);
    }

    [Fact]
    public void ParseInferenceResponse_EmptyProducts_ReturnsEmpty()
    {
        var manna = CreateManna();
        var json = JsonSerializer.Serialize(new MannaMenuItem[]
        {
            new() { name = "Heti menü", products = [] }
        });

        var result = InvokeParse(manna, json);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExtractOffersFromText_ReturnsContentAsIs()
    {
        var manna = CreateManna();

        var result = await InvokeExtractOffersFromText(manna, "raw-json");

        Assert.Equal("raw-json", result);
    }

    [Fact]
    public async Task GetOffersAsync_ValidHetiMenu_ReturnsOffers()
    {
        var json = """[{"name":"Heti menü","products":[{"name":"n","description":"Csirkés","expected_at":"2026-08-10T00:00:00"}]}]""";
        var manna = CreateMannaWithContent(json);

        var offers = await manna.GetOffersAsync();

        Assert.Single(offers);
        Assert.Equal(["Csirkés"], offers["2026-08-10"]);
    }

    [Fact]
    public async Task GetOffersAsync_NoHetiMenu_Throws()
    {
        var manna = CreateMannaWithContent("""[{"name":"Napi menü","products":[]}]""");

        await Assert.ThrowsAsync<MannaHetiMenuNotFoundException>(() => manna.GetOffersAsync());
    }

    private Manna CreateManna()
        => new(_inference, _httpClientFactory, _logger, _stateStore, _resultCache);

    private Manna CreateMannaWithContent(string content)
    {
        _stateStore.GetAsync(Arg.Any<string>()).Returns((HttpResourceState?)null);
        var handler = new ScriptedHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) });
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient().Returns(new HttpClient(handler));
        return new Manna(_inference, factory, _logger, _stateStore, _resultCache);
    }

    private static async Task<string> InvokeExtractOffersFromText(Manna manna, string html)
    {
        var method = typeof(Manna).GetMethod("ExtractOffersFromText", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task<string>)method.Invoke(manna, [html])!;
        return await task;
    }

    private static Dictionary<string, List<string>> InvokeParse(Manna manna, string json)
    {
        var method = typeof(Manna).GetMethod(
            "ParseInferenceResponseAsOffers",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        try
        {
            return (Dictionary<string, List<string>>)method.Invoke(manna, [json])!;
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            throw tie.InnerException;
        }
    }

    private sealed class MannaMenuItem
    {
        public string name { get; set; } = "";
        public MannaProduct[] products { get; set; } = [];
    }

    private sealed class MannaProduct
    {
        public string name { get; set; } = "";
        public string description { get; set; } = "";
        public DateTime? expected_at { get; set; }
    }
}
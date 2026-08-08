using System.Diagnostics.CodeAnalysis;
using debmenu.Caching;
using debmenu.Providers.Inference;
using debmenu.Restaurants;
using Serilog;

namespace Debmenu.Tests.Helpers;

[method: SetsRequiredMembers]
public sealed class TestRestaurant(
    IHttpClientFactory httpClientFactory,
    IInferenceProvider inferenceProvider,
    ILogger logger,
    IHttpResourceStateStore stateStore,
    IRestaurantResultCache resultCache) : Restaurant(
        "https://example.test/",
        httpClientFactory,
        inferenceProvider,
        logger,
        [],
        stateStore,
        resultCache)
{
    public new Dictionary<string, List<string>> FilterOutdatedOffers(Dictionary<string, List<string>> offers)
        => base.FilterOutdatedOffers(offers);

    public new Dictionary<string, List<string>> ParseInferenceResponseAsOffers(string json)
        => base.ParseInferenceResponseAsOffers(json);

    public Task<Dictionary<string, List<string>>> TestGetOffersWithCachingAsync(Func<Task<Dictionary<string, List<string>>>> fetchWorkflow)
        => GetOffersWithCachingAsync(fetchWorkflow);

    public async Task<string> TestGetContentFromUrl()
        => await GetContentFromUrl();

    public void AddInstruction(string instruction)
        => ExtractInstructions.Add(instruction);

    protected override async Task<string?> ExtractOffersFromText(string html)
        => await Task.FromResult("{\"2026-08-10\": [\"Empty\"]}");
}
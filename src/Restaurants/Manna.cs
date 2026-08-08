using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using debmenu.Caching;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Restaurants;

[method: SetsRequiredMembers]
public sealed class Manna(
    IInferenceProvider inferenceProvider,
    IHttpClientFactory httpClientFactory,
    ILogger logger,
    IHttpResourceStateStore stateStore,
    IRestaurantResultCache resultCache) : Restaurant(
        "https://onemin-prod.herokuapp.com/api/v3/partners/304/restaurants/287/product-categories/with-products?type=web",
        httpClientFactory,
        inferenceProvider,
        logger,
        [],
        stateStore,
        resultCache)
{
    private const string HETI_MENU = "Heti menü";

    public override async Task<Dictionary<string, List<string>>> GetOffersAsync()
    {
        return await GetOffersWithCachingAsync(() => TextWorkflow());
    }

    protected override async Task<string?> ExtractOffersFromText(string html)
    {
        return await Task.FromResult(html);
    }

    protected override Dictionary<string, List<string>> ParseInferenceResponseAsOffers(string json)
    {
        using var op = CreateTimedOperation(nameof(ParseInferenceResponseAsOffers));
        var menu = JsonSerializer.Deserialize<MannaMenu[]>(json) ?? throw new MannaMenuCannotBeParsedException();

        var hetiMenu = menu.FirstOrDefault(menu => menu.Name == HETI_MENU) ?? throw new MannaHetiMenuNotFoundException();

        Dictionary<string, List<string>> offers = [];

        foreach (var product in hetiMenu.Products)
        {
            if (product.ExpectedAt is null)
            {
                Logger.Warning("Skipping {Name} because there is no ExpectedAt defined", product.Name);
                continue;
            }

            var day = product.ExpectedAt.Value.ToString("yyyy-MM-dd");
            if (!offers.ContainsKey(day))
            {
                offers.Add(day, [product.Description]);
            }
            else
            {
                offers[day].Add(product.Description);
            }
        }

        return offers;
    }

    private sealed class MannaMenu
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("products")]
        public required MannaProduct[] Products { get; set; }
    }

    private sealed class MannaProduct
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }
        [JsonPropertyName("description")]
        public required string Description { get; set; }
        [JsonPropertyName("expected_at")]
        public DateTime? ExpectedAt { get; set; }
    }

}

using System.Text.Json;
using OpenQA.Selenium;

public class MannaScraper : Scraper
{
    public override string Url => "https://onemin-prod.herokuapp.com/api/v3/partners/304/restaurants/287/product-categories/with-products?type=web";

    public override string Name => "Manna";

    public override By? Selector => null;

    protected async override Task<IScrapeResult> ScrapeInternal(IWebDriver webDriver, IWebElement? element)
    {
        try
        {
            var json = await new HttpClient().GetStringAsync(Url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var resp = JsonSerializer.Deserialize<MannaResponseCategory[]>(json, options);

            var hetiMenu = resp?.FirstOrDefault(p => p.name == "Heti menü") ?? new MannaResponseCategory { name = "", products = [] };

            Dictionary<string, List<string>> menu = new();
            foreach (var product in hetiMenu.products)
            {
                menu.Add(product.name, [product.description]);
            }

            return new SeleniumMenuScraperResult
            {
                Name = Name,
                Menu = menu,
                Successful = true
            };
        }
        catch (Exception ex)
        {
            return new FailedScraperResult
            {
                Name = Name,
                ErrorMessage = ex.Message
            };
        }
    }

    class MannaResponseCategory
    {
        public int id { get; set; }
        public required string name { get; set; }
        public required MannaProduct[] products { get; set; }

        public class MannaProduct
        {
            public int id { get; set; }
            public required string name { get; set; }
            public required string description { get; set; }
        }
    }
}
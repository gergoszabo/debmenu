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

            Dictionary<Tuple<DateTime, string>, List<string>> menu = new();
            foreach (var product in hetiMenu.products)
            {
                // Hétfő (01.15.) A menü
                var split = product.name.Split(" ");
                var monthAndDay = split[1][1..].Replace(")", "");
                var date = DateTime.Parse($"{DateTime.Now.Year}.{monthAndDay}");
                var key = new Tuple<DateTime, string>(date, split[0]);
                if (menu.ContainsKey(key))
                {
                    menu[key].AddRange([string.Join(" ", split[2..]), product.description]);
                }
                else
                {
                    menu.Add(key, [string.Join(" ", split[2..]), product.description]);
                }
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
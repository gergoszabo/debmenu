using debmenu.Logging;
using debmenu.Restaurants;
using Serilog;

namespace debmenu;

public class DataCollector(
    IEnumerable<IRestaurant> scrapers,
    ILogger logger) : IDataCollector
{
    private IEnumerable<IRestaurant> Restaurants { get; } = scrapers;
    private ILogger Logger { get; } = logger;

    public async Task<Dictionary<string, Dictionary<string, List<string>>>> CollectOffers()
    {
        using var op = new TimedOperation("DataCollector CollectOffers", [], Logger);
        var allOffers = new Dictionary<string, Dictionary<string, List<string>>>();

        foreach (var restaurant in Restaurants)
        {
            try
            {
                var offers = await restaurant.GetOffersAsync(); 
                allOffers[restaurant.GetType().Name] = offers;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to collect offers from scraper: {ScraperType}", restaurant.GetType().Name);
            }
        }

        return allOffers;
    }
}

interface IDataCollector
{
    Task<Dictionary<string, Dictionary<string, List<string>>>> CollectOffers();
}
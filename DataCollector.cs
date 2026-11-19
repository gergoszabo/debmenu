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
        using var op = new TimedOperation("[{Class}] CollectOffers", [GetType().Name], Logger);
        var allOffers = new Dictionary<string, Dictionary<string, List<string>>>();
        int totalPromptTokens = 0, totalCandidatesTokens = 0, totalTokens = 0;

        foreach (var restaurant in Restaurants)
        {
            try
            {
                var offers = await restaurant.GetOffersAsync();
                allOffers[restaurant.GetType().Name] = offers;

                if (restaurant.TotalInferenceCost is not null)
                {
                    totalPromptTokens += restaurant.TotalInferenceCost.PromptTokenCount;
                    totalCandidatesTokens += restaurant.TotalInferenceCost.CandidatesTokenCount;
                    totalTokens += restaurant.TotalInferenceCost.TotalTokenCount;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[{Class}] Failed to collect offers from scraper: {ScraperType}", GetType().Name, restaurant.GetType().Name);
            }
        }

        Logger.Information("[{Class}] Total inference cost: {PromptTokenCount} prompt + {CandidatesTokenCount} response = {TotalTokenCount} total tokens",
            GetType().Name, totalPromptTokens, totalCandidatesTokens, totalTokens);

        return allOffers;
    }
}

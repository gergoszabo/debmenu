using debmenu.Logging;
using debmenu.Restaurants;
using Serilog;

namespace debmenu;

/// <summary>
/// Aggregates menu offers from all registered restaurant scrapers.
/// </summary>
public class DataCollector(
    IEnumerable<IRestaurant> scrapers,
    ILogger logger) : IDataCollector
{
    /// <summary>
    /// Collects menu offers from every registered scraper, tolerating individual failures.
    /// </summary>
    /// <returns>A dictionary mapping restaurant name to day-of-week to list of offer items.</returns>
    public async Task<Dictionary<string, Dictionary<string, List<string>>>> CollectOffers()
    {
        using var op = new TimedOperation("[{Class}] CollectOffers", [GetType().Name], logger);
        var allOffers = new Dictionary<string, Dictionary<string, List<string>>>();
        int totalPromptTokens = 0, totalCandidatesTokens = 0, totalTokens = 0;

        foreach (var restaurant in scrapers)
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
                logger.Error(ex, "[{Class}] Failed to collect offers from scraper: {ScraperType}", GetType().Name, restaurant.GetType().Name);
            }
        }

        logger.Information("[{Class}] Total inference cost: {PromptTokenCount} prompt + {CandidatesTokenCount} response = {TotalTokenCount} total tokens",
            GetType().Name, totalPromptTokens, totalCandidatesTokens, totalTokens);

        return allOffers;
    }
}

using debmenu.Providers.Inference;

namespace debmenu.Restaurants;

/// <summary>
/// Represents a restaurant scraper that fetches and parses the weekly menu.
/// </summary>
public interface IRestaurant
{
    /// <summary>
    /// Fetches and parses the restaurant's menu offers.
    /// </summary>
    /// <returns>A dictionary mapping day-of-week to list of offer items.</returns>
    Task<Dictionary<string, List<string>>> GetOffersAsync();

    /// <summary>
    /// Gets the total inference cost (token usage) accumulated during the last scrape, if any.
    /// </summary>
    InferenceResult? TotalInferenceCost { get; }
}

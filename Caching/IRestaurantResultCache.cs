namespace debmenu.Caching;

/// <summary>
/// Caches parsed menu offers per restaurant so unchanged resources skip re-scraping.
/// </summary>
public interface IRestaurantResultCache
{
    /// <summary>
    /// Retrieves cached offers for a restaurant.
    /// </summary>
    /// <param name="restaurantName">The restaurant's display name.</param>
    /// <returns>The cached day-of-week to offers mapping, or null on a cache miss.</returns>
    Task<Dictionary<string, List<string>>?> GetAsync(string restaurantName);

    /// <summary>
    /// Stores freshly parsed offers for a restaurant.
    /// </summary>
    /// <param name="restaurantName">The restaurant's display name.</param>
    /// <param name="offers">Day-of-week to list of offer items.</param>
    Task SetAsync(string restaurantName, Dictionary<string, List<string>> offers);
}

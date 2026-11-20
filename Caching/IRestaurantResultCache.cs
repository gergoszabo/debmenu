namespace debmenu.Caching;

/// <summary>
/// Caches parsed menu offers per restaurant together with the content identity of the source
/// they were produced from, so unchanged resources skip re-scraping and re-inference.
/// </summary>
public interface IRestaurantResultCache
{
    /// <summary>
    /// Retrieves cached offers for a restaurant along with the source content identity they
    /// were computed from.
    /// </summary>
    /// <param name="restaurantName">The restaurant's display name.</param>
    /// <returns>The cached offers and their source content hash, or null on a cache miss.</returns>
    Task<(Dictionary<string, List<string>> Offers, string ContentHash)?> GetAsync(string restaurantName);

    /// <summary>
    /// Stores freshly parsed offers for a restaurant together with the content identity of the
    /// source they were produced from.
    /// </summary>
    /// <param name="restaurantName">The restaurant's display name.</param>
    /// <param name="offers">Day-of-week to list of offer items.</param>
    /// <param name="contentHash">SHA-256 hex identity of the fetched source content (HTML, plus image bytes).</param>
    Task SetAsync(string restaurantName, Dictionary<string, List<string>> offers, string contentHash);
}

namespace debmenu;

/// <summary>
/// Aggregates menu offers from multiple restaurant scrapers.
/// </summary>
public interface IDataCollector
{
    /// <summary>
    /// Collects menu offers from all registered scrapers.
    /// </summary>
    /// <returns>A dictionary mapping restaurant name to day-of-week to list of offer items.</returns>
    Task<Dictionary<string, Dictionary<string, List<string>>>> CollectOffers();
}
using System.Text.Json;
using debmenu.Logging;
using Serilog;

namespace debmenu.Caching;

/// <summary>
/// File-backed implementation of <see cref="IRestaurantResultCache"/> that persists offers as JSON.
/// </summary>
public class FileRestaurantResultCache(ILogger logger) : IRestaurantResultCache
{
    private const string FilePath = "Cache/offers.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <inheritdoc/>
    public async Task<Dictionary<string, List<string>>?> GetAsync(string restaurantName)
    {
        using var op = new TimedOperation("[{Class}] GetAsync", [GetType().Name, restaurantName], logger);

        if (!File.Exists(FilePath))
            return null;

        var json = await File.ReadAllTextAsync(FilePath);
        var all = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json, JsonOptions);

        var offers = all?.GetValueOrDefault(restaurantName);
        if (offers is not null)
            logger.Information("[{Class}] Cache hit for {Restaurant}", GetType().Name, restaurantName);
        else
            logger.Information("[{Class}] Cache miss for {Restaurant}", GetType().Name, restaurantName);

        return offers;
    }

    /// <inheritdoc/>
    public async Task SetAsync(string restaurantName, Dictionary<string, List<string>> offers)
    {
        using var op = new TimedOperation("[{Class}] SetAsync", [GetType().Name, restaurantName], logger);

        Dictionary<string, Dictionary<string, List<string>>> all;
        if (File.Exists(FilePath))
        {
            var json = await File.ReadAllTextAsync(FilePath);
            all = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json, JsonOptions) ?? [];
        }
        else
        {
            all = [];
        }

        all[restaurantName] = offers;

        if (!Directory.Exists("Cache"))
            Directory.CreateDirectory("Cache");

        var newJson = JsonSerializer.Serialize(all, JsonOptions);
        await File.WriteAllTextAsync(FilePath, newJson);

        logger.Information("[{Class}] Cached offers for {Restaurant} ({Count} days)", GetType().Name, restaurantName, offers.Count);
    }
}

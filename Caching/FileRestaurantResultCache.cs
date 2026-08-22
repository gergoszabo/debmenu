using System.Text.Json;
using debmenu.Logging;
using Serilog;

namespace debmenu.Caching;

public class FileRestaurantResultCache(ILogger logger) : IRestaurantResultCache
{
    private const string FilePath = "Cache/offers.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<Dictionary<string, List<string>>?> GetAsync(string restaurantName)
    {
        using var op = new TimedOperation("[{Class}] {Method} {args}", [GetType().Name, nameof(GetAsync), restaurantName], logger);

        if (!File.Exists(FilePath))
            return null;

        var json = await File.ReadAllTextAsync(FilePath);
        var all = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<string>>>>(json, JsonOptions);

        var offers = all?.GetValueOrDefault(restaurantName);
        if (offers is not null)
            logger.Information("[{Class}] {Method} Cache hit for {Restaurant}", GetType().Name, nameof(GetAsync), restaurantName);
        else
            logger.Information("[{Class}] {Method} Cache miss for {Restaurant}", GetType().Name, nameof(GetAsync), restaurantName);

        return offers;
    }

    public async Task SetAsync(string restaurantName, Dictionary<string, List<string>> offers)
    {
        using var op = new TimedOperation("[{Class}] {Method} {args}", [GetType().Name, nameof(SetAsync), restaurantName], logger);

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

        logger.Information("[{Class}] {Method} Cached offers for {Restaurant} ({Count} days)", GetType().Name, nameof(GetAsync), restaurantName, offers.Count);
    }
}

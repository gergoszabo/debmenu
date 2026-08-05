using System.Text.Json;
using debmenu.Logging;
using Serilog;

namespace debmenu.Caching;

public class FileHttpResourceStateStore(ILogger logger) : IHttpResourceStateStore
{
    private const string FilePath = "Cache/http-states.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<HttpResourceState?> GetAsync(string url)
    {
        using var op = new TimedOperation("FileHttpResourceStateStore GetAsync", [url], logger);

        if (!File.Exists(FilePath))
            return null;

        var json = await File.ReadAllTextAsync(FilePath);
        var states = JsonSerializer.Deserialize<Dictionary<string, HttpResourceState>>(json, JsonOptions);

        var state = states?.GetValueOrDefault(url);
        if (state is not null)
            logger.Information("HTTP state found for {Url}: {State}", url, state);
        else
            logger.Information("No HTTP state for {Url}", url);

        return state;
    }

    public async Task SetAsync(string url, HttpResourceState state)
    {
        using var op = new TimedOperation("FileHttpResourceStateStore SetAsync", [url], logger);
        if (state.ETag == null && state.LastModified == null) {
            logger.Warning("HTTP state not saved for {Url}: Both ETag and LastModified were null, skipping cache update.", url);
            return;
        }

        Dictionary<string, HttpResourceState> states;
        if (File.Exists(FilePath))
        {
            var json = await File.ReadAllTextAsync(FilePath);
            states = JsonSerializer.Deserialize<Dictionary<string, HttpResourceState>>(json, JsonOptions) ?? [];
        }
        else
        {
            states = [];
        }

        states[url] = state;

        if (!Directory.Exists("Cache"))
            Directory.CreateDirectory("Cache");

        var newJson = JsonSerializer.Serialize(states, JsonOptions);
        await File.WriteAllTextAsync(FilePath, newJson);

        logger.Information("HTTP state saved for {Url} (ETag: {Etag}, LastModified: {LastModified})",
            url, state.ETag ?? "(none)", state.LastModified ?? "(none)");
    }
}

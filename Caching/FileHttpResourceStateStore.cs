using System.Text.Json;
using debmenu.Logging;
using Serilog;

namespace debmenu.Caching;

/// <summary>
/// File-backed implementation of <see cref="IHttpResourceStateStore"/> that persists state as JSON.
/// </summary>
public class FileHttpResourceStateStore(ILogger logger) : IHttpResourceStateStore
{
    private const string FilePath = "Cache/http-states.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <inheritdoc/>
    public async Task<HttpResourceState?> GetAsync(string url)
    {
        using var op = new TimedOperation("[{Class}] GetAsync", [GetType().Name, url], logger);

        if (!File.Exists(FilePath))
            return null;

        var json = await File.ReadAllTextAsync(FilePath);
        var states = JsonSerializer.Deserialize<Dictionary<string, HttpResourceState>>(json, JsonOptions);

        var state = states?.GetValueOrDefault(url);
        if (state is not null)
            logger.Information("[{Class}] HTTP state found for {Url}: {State}", GetType().Name, url, state);
        else
            logger.Information("[{Class}] No HTTP state for {Url}", GetType().Name, url);

        return state;
    }

    /// <inheritdoc/>
    public async Task SetAsync(string url, HttpResourceState state)
    {
        using var op = new TimedOperation("[{Class}] SetAsync", [GetType().Name, url], logger);
        if (state.ETag == null && state.LastModified == null)
        {
            logger.Warning("[{Class}] HTTP state not saved for {Url}: Both ETag and LastModified were null, skipping cache update.", GetType().Name, url);
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

        logger.Information("[{Class}] HTTP state saved for {Url} (ETag: {Etag}, LastModified: {LastModified})",
            GetType().Name, url, state.ETag ?? "(none)", state.LastModified ?? "(none)");
    }
}

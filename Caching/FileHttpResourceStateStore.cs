using System.Text.Json;
using debmenu.Logging;
using debmenu.Utils;
using Serilog;

namespace debmenu.Caching;

/// <summary>
/// File-backed implementation of <see cref="IHttpResourceStateStore"/> that persists state as JSON.
/// </summary>
public class FileHttpResourceStateStore(IPathProvider pathProvider, ILogger logger) : IHttpResourceStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <inheritdoc/>
    public async Task<HttpResourceState?> GetAsync(string url)
    {
        using var op = new TimedOperation("[{Class}] GetAsync", [GetType().Name, url], logger);

        var filePath = pathProvider.Combine("Cache", "http-states.json");
        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath);
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

        var filePath = pathProvider.Combine("Cache", "http-states.json");
        Dictionary<string, HttpResourceState> states;
        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath);
            states = JsonSerializer.Deserialize<Dictionary<string, HttpResourceState>>(json, JsonOptions) ?? [];
        }
        else
        {
            states = [];
        }

        states[url] = state;

        var newJson = JsonSerializer.Serialize(states, JsonOptions);
        await AtomicFileWriter.WriteTextAtomicAsync(filePath, newJson);

        logger.Information("[{Class}] HTTP state saved for {Url} (ETag: {Etag}, LastModified: {LastModified})",
            GetType().Name, url, state.ETag ?? "(none)", state.LastModified ?? "(none)");
    }
}

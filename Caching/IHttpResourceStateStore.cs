namespace debmenu.Caching;

/// <summary>
/// Persists HTTP conditional-request state (ETag / Last-Modified) per resource URL.
/// </summary>
public interface IHttpResourceStateStore
{
    /// <summary>
    /// Retrieves the stored HTTP state for a URL, if any.
    /// </summary>
    /// <param name="url">The resource URL.</param>
    /// <returns>The stored state, or null if none exists.</returns>
    Task<HttpResourceState?> GetAsync(string url);

    /// <summary>
    /// Stores the HTTP state for a URL.
    /// </summary>
    /// <param name="url">The resource URL.</param>
    /// <param name="state">The ETag / Last-Modified values to persist.</param>
    Task SetAsync(string url, HttpResourceState state);
}

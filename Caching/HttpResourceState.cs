namespace debmenu.Caching;

/// <summary>
/// HTTP conditional-request state for a resource, used to detect unchanged content.
/// </summary>
/// <param name="ETag">The response's ETag header value, if present.</param>
/// <param name="LastModified">The response's Last-Modified header value, if present.</param>
public record HttpResourceState(string? ETag, string? LastModified);

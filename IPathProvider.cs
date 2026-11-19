namespace debmenu;

/// <summary>
/// Resolves file paths relative to the application's working directory, making
/// CWD-dependent I/O configurable and unit-testable.
/// </summary>
public interface IPathProvider
{
    /// <summary>
    /// Combines a path relative to the working directory with optional segments.
    /// </summary>
    /// <param name="segments">Path segments, in order.</param>
    /// <returns>The absolute path under the working directory.</returns>
    string Combine(params string[] segments);
}

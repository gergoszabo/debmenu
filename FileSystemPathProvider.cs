namespace debmenu;

/// <summary>
/// Default <see cref="IPathProvider"/> that resolves paths against the process working directory.
/// </summary>
public class FileSystemPathProvider : IPathProvider
{
    /// <inheritdoc/>
    public string Combine(params string[] segments) => Path.Combine(segments);
}

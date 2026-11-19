namespace debmenu.Tests;

/// <summary>
/// Gives each test class its own unique temp directory (safe under method-level parallelization)
/// and removes it when the class's tests finish.
/// </summary>
public abstract class TestContextBase : IDisposable
{
    protected readonly string _root;

    protected TestContextBase()
    {
        _root = Path.Combine(Path.GetTempPath(), "debmenu-tests", $"{GetType().Name}-{Guid.NewGuid():N}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}

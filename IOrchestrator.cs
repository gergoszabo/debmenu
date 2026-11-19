namespace debmenu;

/// <summary>
/// Coordinates the end-to-end menu publishing pipeline.
/// </summary>
public interface IOrchestrator
{
    /// <summary>
    /// Collects offers, renders the HTML page and publishes it.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync();
}

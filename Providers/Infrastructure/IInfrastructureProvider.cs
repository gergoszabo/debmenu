namespace debmenu.Providers.Infrastructure;

public interface IInfrastructureProvider
{
    /// <summary>
    /// Uploads the generated menu page to the infrastructure.
    /// </summary>
    /// <param name="content">The HTML content to upload.</param>
    /// <param name="fileName">The object key (file name) in the target storage.</param>
    /// <returns>A task representing the asynchronous upload operation.</returns>
    Task Upload(string content, string fileName);
}

namespace debmenu.Providers.Infrastructure;

public interface IInfrastructureProvider
{
    Task Upload(string content, string fileName);
}

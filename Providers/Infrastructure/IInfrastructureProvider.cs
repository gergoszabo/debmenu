namespace debmenu.Providers.Infrastructure;

internal interface IInfrastructureProvider
{
    public Task Upload(string content, string fileName);
}

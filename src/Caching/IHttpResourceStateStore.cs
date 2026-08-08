namespace debmenu.Caching;

public interface IHttpResourceStateStore
{
    Task<HttpResourceState?> GetAsync(string url);
    Task SetAsync(string url, HttpResourceState state);
}

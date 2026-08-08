namespace debmenu.Caching;

public interface IRestaurantResultCache
{
    Task<Dictionary<string, List<string>>?> GetAsync(string restaurantName);
    Task SetAsync(string restaurantName, Dictionary<string, List<string>> offers);
}

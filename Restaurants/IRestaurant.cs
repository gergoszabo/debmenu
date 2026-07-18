namespace debmenu.Restaurants;

public interface IRestaurant
{
    Task<Dictionary<string, List<string>>> GetOffersAsync();
}

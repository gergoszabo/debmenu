namespace debmenu.Restaurants;

internal interface IRestaurant
{
    public Task<Dictionary<string, List<string>>> GetOffersAsync();
}

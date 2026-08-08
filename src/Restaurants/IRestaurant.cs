using debmenu.Providers.Inference;

namespace debmenu.Restaurants;

public interface IRestaurant
{
    Task<Dictionary<string, List<string>>> GetOffersAsync();
    InferenceResult? TotalInferenceCost { get; }
}

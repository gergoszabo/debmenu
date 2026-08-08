namespace debmenu;

public interface IDataCollector
{
    Task<OffersCollection> CollectOffers();
}
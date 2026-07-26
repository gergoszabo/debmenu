namespace debmenu;

public interface IDataCollector
{
    Task<Dictionary<string, Dictionary<string, List<string>>>> CollectOffers();
}
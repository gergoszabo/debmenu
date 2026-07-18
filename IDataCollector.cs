namespace debmenu;

interface IDataCollector
{
    Task<Dictionary<string, Dictionary<string, List<string>>>> CollectOffers();
}
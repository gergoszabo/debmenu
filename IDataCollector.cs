namespace debmenu;

internal interface IDataCollector
{
    public Task<Dictionary<string, Dictionary<string, List<string>>>> CollectOffers();
}
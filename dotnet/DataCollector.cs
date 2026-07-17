using debmenu.Logging;
using debmenu.Restaurants;
using Serilog;

namespace debmenu;

public class DataCollector(
    Forest forest,
    Govinda govinda,
    Huse huse,
    Viktoria viktoria,
    ILogger logger
    )
{
    private Forest Forest { get; } = forest;
    private Govinda Govinda { get; } = govinda;
    private Huse Huse { get; } = huse;
    private Viktoria Viktoria { get; } = viktoria;
    private ILogger Logger { get; } = logger;

    public async Task<Dictionary<string, Dictionary<string, List<string>>>> CollectOffers()
    {
        using var op = new TimedOperation("DataCollector CollectOffers", [], Logger);
        var allOffers = new Dictionary<string, Dictionary<string, List<string>>>
        {
            { "Viktoria", await Viktoria.GetOffers() },
            { "Govinda", await Govinda.GetOffers() },
            { "Forest", await Forest.GetOffers() },
            { "Huse", await Huse.GetOffers() }
        };

        return allOffers;
    }
}
namespace debmenu.Restaurants;

public static class ForestPrompts
{
    /// <summary>
    /// Forest's weekly menu image has two special sections that must be merged into every day,
    /// and the grill list can be line-broken mid-item. These rules replace the generic extraction
    /// task; the response structure and date grounding stay shared.
    /// </summary>
    public const string ExtractTask = """
Extract the offers from the image grouped for each day, without headers and pricing information.
it can happen that the daterange contains a typo.
There is a section called 'Heti leves ajánlat' and 'Heti grill ajánlat'.
Those needs to be added to every day. The 'Heti grill ajánlat' contains one item each line,
it might happen that it was broken into multiple line due space constraints, apply some hungarian linguistics to detect this.
""";
}

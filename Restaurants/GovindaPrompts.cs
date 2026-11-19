namespace debmenu.Restaurants;

public static class GovindaPrompts
{
    /// <summary>
    /// Govinda's menu is fetched as raw HTML and dissected down to plain text before inference,
    /// so the generic "from the image" extraction task does not apply. This task describes the
    /// actual input: flat text with ## markers for days/sections, category labels that are not
    /// offers, and no pricing information (prices live in a client-side tab on the site).
    /// </summary>
    public const string ExtractTask = """
Extract the weekly menu offers from the plain-text menu content below, grouped by day.
Lines starting with "##" mark day or section boundaries in the source text; they are not offers themselves.
Category labels such as Leves, Dhal, Sabjik, Egytétel, Egytálétel, Köretek, Csatni and Desszert group the dishes but are not offers — skip them.
The menu contains no pricing information; do not invent prices.
The date range may contain a typo.
""";
}

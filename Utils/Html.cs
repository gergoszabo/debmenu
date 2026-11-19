namespace debmenu.Utils;

/// <summary>
/// Holds the HTML template used to render the final menu page.
/// </summary>
public static class Html
{
    /// <summary>
    /// The full HTML page template, loaded from resources. Placeholders: <c>__VERSION__</c> (version),
    /// <c>__OFFERS_JSON__</c> (offers payload), <c>__GENERATED_AT__</c> (timestamp).
    /// </summary>
    public static string Template => debmenu_Resources_Html.Template;
}

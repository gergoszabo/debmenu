namespace debmenu.Utils;

public static class PromptConstants
{
    /// <summary>
    /// Task prompt for locating the menu image link within a page's HTML.
    /// </summary>
    public const string ExtractImageLinkTask = """
  Extract the image links for \"heti étlap\", \"heti menü\" or the single one with high fetch priority from this piece of html.
  Only return the links, nothing else.
""";
    /// <summary>
    /// Default extraction task for text-based workflows (HTML, JSON or plain-text payloads).
    /// The image-specific variant is <see cref="ResponseExtractTaskFromImage"/>; keep the two split
    /// so a text workflow never tells the model "from the image".
    /// </summary>
    public const string ResponseExtractTask = @"
Extract the offers from the provided content grouped for each day 
day without headers and pricing information. 
it can happen that the daterange contains a typo.
";

    /// <summary>
    /// Extraction task for image-based workflows, where the menu is rendered as an image.
    /// </summary>
    public const string ResponseExtractTaskFromImage = @"
Extract the offers from the image grouped for each day 
day without headers and pricing information. 
it can happen that the daterange contains a typo.
";

    public const string ResponseStructure = @"
Respond only with json.
The structure should be like this:

{
  ""date"": [
    ""Offer 1"",
    ""Offer 2""
  ],
  ""date"": [
    ""Offer 1"",
    ""Offer 2""
  ]
}

The keys are ISO date strings, and the values are arrays of strings representing the offers for that day.
";

    /// <summary>
    /// Date grounding prompt, computed per call so a long-lived host never grounds to a stale date.
    /// </summary>
    public static string DateGrounding => $@"Dates are in the format YYYY-MM-DD.
The current date is {DateTime.UtcNow:yyyy-MM-dd}.";

    /// <summary>
    /// Year grounding prompt, computed per call so a long-lived host never grounds to a stale year.
    /// </summary>
    public static string YearGrounding => $"The current year is {DateTime.UtcNow.Year}.";
}

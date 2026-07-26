namespace debmenu.Utils;

public static class PromptConstants
{
    public const string ExtractImageLinkTask = """
  Extract the image links for \"heti étlap\", \"heti menü\" or the single one with high fetch priority from this piece of html.
  Only return the links, nothing else.
""";
    public const string ResponseExtractTask = @"
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

    public static readonly string DateGrounding = $@"Dates are in the format YYYY-MM-DD.
The current date is {DateTime.UtcNow:yyyy-MM-dd}.";

    public static readonly string YearGrounding = $"The current year is {DateTime.UtcNow.Year}.";

    public static readonly string ExtractInstruction = $"{ResponseExtractTask} {ResponseStructure} {DateGrounding} {YearGrounding}";
}

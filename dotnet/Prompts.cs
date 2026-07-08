using System;

namespace debmenu
{
    public static class PromptConstants
    {
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

        // Must be 'static readonly' instead of 'const' because DateTime.UtcNow is evaluated at runtime
        public static readonly string DateGrounding = $@"Dates are in the format YYYY-MM-DD.
The current date is {DateTime.UtcNow:yyyy-MM-dd}.";

        public static readonly string YearGrounding = $"The current year is {DateTime.UtcNow.Year}.";
    }
}
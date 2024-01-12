using OpenQA.Selenium;

public class MelangeKavehazScraper : Scraper
{
    public override string Name => "Melange Kávéház";

    public override string Url => "https://melangekavehaz.hu";

    public override By? Selector => By.Id("hetimenu");

    protected override Task<IScrapeResult> ScrapeInternal(IWebDriver webDriver, IWebElement? element)
    {
        var startTime = DateTime.Now;
        ArgumentNullException.ThrowIfNull(element);

        // 2024.01.08 - 01.12.
        var dateRangeElement = element.FindElement(By.ClassName("wpb_text_column"));

        var dateRange = dateRangeElement.Text.Split("-").Select(p => p.Trim()).ToArray();
        var startDate = DateTime.Parse(dateRange[0]);
        var endDate = DateTime.Parse($"{startDate.Year}.{dateRange[1]}");

        var dailyOffers = element.FindElements(By.TagName("div"))
            .Where(d => d.GetAttribute("class").Contains("vc_col-sm-1/5"))
            .ToArray();

        var dates = Utils.GenerateHungarianFormattedDateRange(startDate, endDate);

        Dictionary<string, List<string>> coursesForEachDay = [];
        foreach (var dailyOffer in dailyOffers)
        {
            var lines = dailyOffer.Text.Split("\n");
            var dateForLine = dates.FirstOrDefault();
            dates.RemoveAt(0);

            coursesForEachDay.Add($"{dateForLine:yyyy.MM.dd} {lines[0]}", lines.Skip(1).ToList());
        }

        return Task.FromResult<IScrapeResult>(new SeleniumMenuScraperResult()
        {
            Menu = coursesForEachDay,
            Name = Name,
            Elapsed = DateTime.Now.Subtract(startDate).TotalMilliseconds,
            Successful = true
        });
    }
}

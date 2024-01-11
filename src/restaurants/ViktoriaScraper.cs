using OpenQA.Selenium;

public class ViktoriaScraper : Scraper
{
    public override string Url => "https://www.viktoriaetterem.hu/menu";

    public override string Name => "Viktória";

    public override By? Selector => By.ClassName("content-tab");

    protected override Task<IScrapeResult> ScrapeInternal(IWebDriver webDriver, IWebElement? element)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(element);
            Dictionary<string, List<string>> coursesForEachDay = [];
            var dates = GetDateRange(webDriver);

            var currentDay = "";
            element.FindElements(By.TagName("div"))
                .ToList()
                .ForEach(div =>
                {
                    if (div.GetAttribute("class") == "")
                    {
                        var date = dates.FirstOrDefault();
                        if (dates.Count > 0)
                        {
                            dates.RemoveAt(0);
                        }
                        else
                        {
                            return;
                        }
                        // this is a day, key already exists in dict
                        currentDay = string.Format("{0} {1}", date, div.Text.ToLower()).Trim();
                        coursesForEachDay.Add(currentDay, []);
                    }
                    else if (div.GetAttribute("class") == "featured-title")
                    {
                        coursesForEachDay[currentDay]?.Add(div.Text);
                    }
                    // Handle special case, one per month
                    else if (div.GetAttribute("class") == "featured-desc")
                    {
                        if (div.Text.Trim().Length > 0)
                        {
                            var text = string.Join("<br>", div
                                .FindElements(By.TagName("p"))
                                .Select(p => p.Text.Trim())
                                .Where(p => p.Trim().Length > 0));
                            coursesForEachDay[currentDay]?.Add(text + "<br><br>");
                        }
                    }
                });

            return Task.FromResult<IScrapeResult>(new SeleniumMenuScraperResult
            {
                Name = Name,
                Menu = coursesForEachDay,
                Successful = true
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult<IScrapeResult>(new FailedScraperResult
            {
                Name = Name,
                ErrorMessage = ex.Message
            });
        }
    }

    private static List<string> GetDateRange(IWebDriver driver)
    {
        List<string> dates = [];
        var yearStr = DateTime.Now.Year.ToString();
        // grab date range
        var dateRange = driver
            .FindElements(By.TagName("p"))
            .Where(p => p.GetAttribute("style") == "text-align: center;")
            .Select(p => p.GetAttribute("innerText"))
            .Where(text => text.Contains(yearStr))
            .FirstOrDefault();

        // sample: 2023. December 18. - December 22.
        var startDate = DateTime.Parse(dateRange?.Split("-").FirstOrDefault()?.Trim().Replace("Január", "January") ?? "");
        var endDate = DateTime.Parse(yearStr + ". " + dateRange?.Split("-").LastOrDefault()?.Trim().Replace("Január", "January") ?? "");
        dates = [];
        for (int i = 0; i < 5; i++)
        {
            dates.Add(startDate.AddDays(i).ToString("yyyy.MM.dd."));
        }

        return dates;
    }
}
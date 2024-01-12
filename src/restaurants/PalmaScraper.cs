using System.Globalization;
using OpenQA.Selenium;

public class PalmaScraper : Scraper
{
    public override string Name => "Pálma";

    public override string Url => "https://www.palmaetterem.hu/#HETI_MENU";

    public override By? Selector => By.Id("HETI_MENU");

    protected override Task<IScrapeResult> ScrapeInternal(IWebDriver webDriver, IWebElement? element)
    {
        var startDate = DateTime.Now;
        ArgumentNullException.ThrowIfNull(element);

        var menuListItems = element.FindElements(By.ClassName("menuList"));

        Dictionary<Tuple<DateTime, string>, List<string>> coursesForEachDay = [];
        foreach (var menuListItem in menuListItems)
        {
            var dateDay = menuListItem.FindElement(By.ClassName("Date__day")).Text;
            // Január 15 HÉTFŐ
            var dateDate = menuListItem.FindElement(By.ClassName("Date__date")).Text;
            var parsedDate = DateTime.Parse($"{DateTime.Now.Year}. {dateDate}", CultureInfo.GetCultureInfo("hu"));
            var menuList = menuListItem.FindElement(By.ClassName("menuList__menu")).Text;
            coursesForEachDay.Add(new Tuple<DateTime, string>(parsedDate, dateDay), menuList.Split("\n").ToList());
        }

        return Task.FromResult<IScrapeResult>(new SeleniumMenuScraperResult()
        {
            Menu = coursesForEachDay,
            Name = Name,
            Elapsed = DateTime.Now.Subtract(startDate).TotalMilliseconds,
        });
    }
}

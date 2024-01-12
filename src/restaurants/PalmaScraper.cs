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

        Dictionary<string, List<string>> coursesForEachDay = [];
        foreach (var menuListItem in menuListItems)
        {
            var dateDay = menuListItem.FindElement(By.ClassName("Date__day")).Text;
            var dateDate = menuListItem.FindElement(By.ClassName("Date__date")).Text;
            var menuList = menuListItem.FindElement(By.ClassName("menuList__menu")).Text;
            coursesForEachDay.Add($"{dateDate} {dateDay}", menuList.Split("\n").ToList());
        }

        return Task.FromResult<IScrapeResult>(new SeleniumMenuScraperResult()
        {
            Menu = coursesForEachDay,
            Name = Name,
            Elapsed = DateTime.Now.Subtract(startDate).TotalMilliseconds,
        });
    }
}

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

public class CampusScraper : Scraper
{
    public override string Url => "https://napimenu.campusettermek.hu/hetimenu";

    public override string Name => "Campus";

    public override By? Selector => By.TagName("img");

    protected async override Task<IScrapeResult> ScrapeInternal(IWebDriver webDriver, IWebElement? element)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(element);
            var imgSrc = element.GetAttribute("src");

            var bytes = await new HttpClient().GetByteArrayAsync(imgSrc);

            return new ImageScraperResult
            {
                Name = Name,
                Successful = true,
                ImageAsByteArray = bytes
            };
        }
        catch (Exception ex)
        {
            return new FailedScraperResult
            {
                Name = Name,
                ErrorMessage = ex.Message
            };
        }
    }
}

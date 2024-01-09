using OpenQA.Selenium;

public class GovindaScraper : Scraper {
    public override string Url => "https://www.govindadebrecen.hu";

    public override string Name => "Govinda";

    public override By? Selector => By.ClassName("menu-img");

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
using ImageMagick;
using OpenQA.Selenium;

public class HuseScraper : Scraper
{
    public override string Url => "http://www.husevendeglo.hu";

    public override string Name => "Hüse";

    public override By? Selector => By.ClassName("napimenu");

    protected async override Task<IScrapeResult> ScrapeInternal(IWebDriver webDriver, IWebElement? element)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(element);
            var imgTag = element.FindElement(By.TagName("img"));
            var imgSrc = imgTag.GetAttribute("src");

            var bytes = await new HttpClient().GetByteArrayAsync(imgSrc);

            var cleanedUpImageBytes = cleanupImage(bytes);

            return new ImageScraperResult
            {
                Name = Name,
                Successful = true,
                ImageAsByteArray = cleanedUpImageBytes
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

    private static byte[] cleanupImage(byte[] bytes)
    {
        var img = new MagickImage(bytes);

        img.Evaluate(Channels.All, EvaluateOperator.Add, 50);

        return img.ToByteArray(MagickFormat.Png);
    }
}

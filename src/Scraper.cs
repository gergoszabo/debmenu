using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

public abstract class Scraper
{
    public abstract string Name { get; }
    public abstract string Url { get; }
    public abstract By? Selector { get; }
    protected Stopwatch timer = new();

    protected virtual IWebDriver GetWebDriver()
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("--headless=new");
        return new ChromeDriver(chromeOptions);
    }

    public virtual async Task<IScrapeResult> Scrape()
    {
        var driver = GetWebDriver();
        try
        {
            timer.Start();

            IScrapeResult result;
            if (Selector != null)
            {
                var element = await OpenBrowserAndGetElementBySelector(driver);

                result = await ScrapeInternal(driver, element);
            }
            else
            {
                result = await ScrapeInternal(driver, null);
            }

            result.Elapsed = timer.ElapsedMilliseconds;

            Console.WriteLine($"{Name} done {result.GetType().FullName} in {result.Elapsed} ms");
            return result;
        }
        catch (Exception ex)
        {
            return new FailedScraperResult
            {
                Name = Name,
                ErrorMessage = ex.Message,
                Elapsed = timer.ElapsedMilliseconds
            };
        }
        finally
        {
            driver.Close();
            driver.Quit();
        }
    }

    protected virtual Task<IWebElement> OpenBrowserAndGetElementBySelector(IWebDriver webDriver)
    {
        var wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(2));
        webDriver.Navigate().GoToUrl(Url);

        var element = webDriver.FindElement(Selector ?? By.Name("body"));
        wait.Until(d => element.Displayed);

        return Task.FromResult(element);
    }

    protected abstract Task<IScrapeResult> ScrapeInternal(IWebDriver webDriver, IWebElement? element);
}

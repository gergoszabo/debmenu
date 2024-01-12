// See https://aka.ms/new-console-template for more information

Publish.CheckEnvVariables();
EmailResults.CheckEnvVariables();

var directory = "results";
if (Directory.Exists(directory))
{
    Directory.Delete(directory, true);
}
Directory.CreateDirectory(directory);

Scraper[] scrapers = [
    new CampusScraper(),
    new GovindaScraper(),
    new HuseScraper(),
    new MannaScraper(),
    new MelangeKavehazScraper(),
    new PalmaScraper(),
    new ViktoriaScraper(),
];

var results = scrapers.Select(scraper => scraper.Scrape().GetAwaiter().GetResult()).ToArray();

ScraperResultHandler.GenerateHtmlFromResults(results);

Publish.UploadToS3();

EmailResults.SendScrapeResults(results);

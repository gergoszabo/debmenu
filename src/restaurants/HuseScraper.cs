using System.Globalization;
using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using ImageMagick;
using OpenQA.Selenium;

public class HuseScraper : Scraper
{
    public override string Url => "http://www.husevendeglo.hu";

    public override string Name => "Hüse";

    public override By? Selector => By.ClassName("napimenu");

    private bool skipDetectText => Environment.GetCommandLineArgs().Contains("--skip-detect-text");

    protected async override Task<IScrapeResult> ScrapeInternal(IWebDriver webDriver, IWebElement? element)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(element);
            var imgTag = element.FindElement(By.TagName("img"));
            var imgSrc = imgTag.GetAttribute("src");

            var bytes = await new HttpClient().GetByteArrayAsync(imgSrc);

            var cleanedUpImageBytes = cleanupImage(bytes);

            File.WriteAllBytes("results/huse.png", cleanedUpImageBytes);

            Console.WriteLine("DetectText: " + skipDetectText.ToString());
            if (!skipDetectText)
            {
                return DetectTextOnImage(cleanedUpImageBytes);
            }

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

    private SeleniumMenuScraperResult DetectTextOnImage(byte[] imageBytes)
    {
        DetectTextRequest detectTextRequest = new DetectTextRequest()
        {
            Image = new Image()
            {
                Bytes = new MemoryStream(imageBytes)
            }
        };
        Console.WriteLine("Making DetectText request");
        DetectTextResponse detectTextResponse = new AmazonRekognitionClient(RegionEndpoint.EUCentral1)
            .DetectTextAsync(detectTextRequest).GetAwaiter().GetResult();
        Console.WriteLine($"DetectText response {detectTextResponse.HttpStatusCode}");

        foreach (TextDetection text in detectTextResponse.TextDetections)
        {
            Console.WriteLine("Detected: " + text.DetectedText);
            Console.WriteLine("Confidence: " + text.Confidence);
            Console.WriteLine("Id : " + text.Id);
            Console.WriteLine("Parent Id: " + text.ParentId);
            Console.WriteLine("Type: " + text.Type);
        }

        return processDetectTextResponse(detectTextResponse);
    }

    private SeleniumMenuScraperResult processDetectTextResponse(DetectTextResponse detectTextResponse)
    {
        var dates = parseDatesFromDateRangeLine(detectTextResponse.TextDetections[1].DetectedText);

        Dictionary<Tuple<DateTime, string>, List<string>> coursesForEachDay = [];
        Tuple<DateTime, string> currentDay = new Tuple<DateTime, string>(DateTime.Now, "");
        // 0: Heti menü
        // 1: DateRange
        // 2: 2000 Ft
        // 3: days
        var dateIndex = 0;
        for (int i = 3; i < detectTextResponse.TextDetections.Count; i++)
        {
            var td = detectTextResponse.TextDetections[i];
            var lowerText = td.DetectedText.ToLower();

            if (Utils.DAYS_LOWER_HU.Any(d => d == lowerText))
            {
                // currentDay = $"{dates[dateIndex++]} {td.DetectedText}";
                currentDay = new Tuple<DateTime, string>(dates[dateIndex++], td.DetectedText);
                coursesForEachDay.Add(currentDay, new());
            }
            else if (td.DetectedText.ToLower() == "***")
            {
                break;
            }
            else
            {
                coursesForEachDay[currentDay].Add(td.DetectedText);
            }
        }

        return new SeleniumMenuScraperResult()
        {
            Menu = coursesForEachDay,
            Name = Name,
            Successful = true
        };
    }

    private static List<DateTime> parseDatesFromDateRangeLine(string dateRangeLine)
    {
        // JANUÁR 8. ÉS 12. KÖZÖTT
        var dateRange = dateRangeLine.ToLower()
            .Replace("között", "")
            .Trim()
            .Split("és")
            .Select(s => s.Trim())
            .ToArray();
        // ["január 8.", "12."]
        var monthAndStartDayHu = dateRange[0].Split(" ");
        var startDate = DateTime.Parse($"{DateTime.Now.Year}. {monthAndStartDayHu[0]} {monthAndStartDayHu[1]}", CultureInfo.GetCultureInfo("hu"));
        var endDate = DateTime.Parse($"{DateTime.Now.Year}. {monthAndStartDayHu[0]} {dateRange[1]}", CultureInfo.GetCultureInfo("hu"));

        return Utils.GenerateHungarianFormattedDateRange(startDate, endDate);
    }
}

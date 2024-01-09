public static class ScraperResultHandler
{
    static string ResultDirectory = "results";
    static string MetaAndCss = "<meta charset=\"UTF-8\"><style>div,h1{text-align:center;width:100%;}img{max-width:100%;max-height:calc(100vh - 80px)}</style>";

    public static void GenerateHtmlFromResults(IScrapeResult[] results)
    {
        foreach (var result in results)
        {
            string? generatedHtml = null;
            switch (result)
            {
                case ImageScraperResult restaurantImageResult:
                    generatedHtml = GeneratePageFromImage(restaurantImageResult);
                    break;
                case SeleniumMenuScraperResult restaurantTextResult:
                    generatedHtml = GeneratePageFromText(restaurantTextResult);
                    break;
                case FailedScraperResult failedScraperResult:
                    generatedHtml = GeneratePageFromFailedScraperResult(failedScraperResult);
                    break;
                default:
                    break;
            }

            if (generatedHtml != null)
            {
                if (!Directory.Exists(ResultDirectory))
                {
                    Directory.CreateDirectory(ResultDirectory);
                }
                File.WriteAllText(GetFullFilenameForResult(result), generatedHtml);
            }
        }

        GenerateMainPageForResults(results);
    }

    private static string GetFilenameForResult(IScrapeResult result)
    {
        var name = result.Name
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ö", "o")
                .Replace("ő", "o")
                .Replace("ú", "u")
                .Replace("ü", "u")
                .Replace("ű", "u");
        return $"{name}.html";
    }

    private static string GetFullFilenameForResult(IScrapeResult result)
    {
        return $"{ResultDirectory}{Path.DirectorySeparatorChar}{GetFilenameForResult(result)}";
    }

    private static void GenerateMainPageForResults(IScrapeResult[] results)
    {
        var html = MetaAndCss + "<ul>" + string.Join("", results.Select(r => $"""<li><a href="{GetFilenameForResult(r)}">{r.Name}</a></li>""")) + $"</ul>Generated at {DateTime.Now.ToString("yyyy.MM.dd. HH:mm:ss")}";
        File.WriteAllText($"{ResultDirectory}{Path.DirectorySeparatorChar}index.html", html);
    }

    private static string? GeneratePageFromFailedScraperResult(FailedScraperResult failedScraperResult)
    {
        return $"{MetaAndCss}<h1>{failedScraperResult.Name}</h1><div><pre>{failedScraperResult.ErrorMessage}</pre>";
    }

    private static string? GeneratePageFromText(SeleniumMenuScraperResult result)
    {
        if (result.Menu != null)
        {
            var str = string.Join("",
            result.Menu.Select(day =>
            {
                return $"<h3>{day.Key}</h3>{string.Join("", day.Value.Select(course => "<div>" + course + "</div>"))}";
            }));

            return $"{MetaAndCss}<h1>{result.Name}</h1><div>{str}";
        }

        return $"{MetaAndCss}<h1>{result.Name}</h1><div><pre>{result.Menu}</pre>";
    }

    private static string? GeneratePageFromImage(ImageScraperResult result)
    {
        return $$"""{{MetaAndCss}}<h1>{{result.Name}}</h1><div><img src="data:image/png;base64, {{Convert.ToBase64String(result.ImageAsByteArray ?? [])}}">""";
    }
}

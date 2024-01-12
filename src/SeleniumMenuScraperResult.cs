public class SeleniumMenuScraperResult: BaseScraperResult {
    public required Dictionary<Tuple<DateTime, string>, List<string>> Menu { get; set; }
}

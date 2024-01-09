public abstract class BaseScraperResult : IScrapeResult
{
    public virtual bool Successful { get; set; }
    public double Elapsed { get; set; }
    public required string Name { get; set; }
}

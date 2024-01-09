public class FailedScraperResult : BaseScraperResult
{
    public override bool Successful { get => false; }
    public required string ErrorMessage { get; set; }
}

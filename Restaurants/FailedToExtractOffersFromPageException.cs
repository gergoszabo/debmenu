namespace debmenu.Restaurants;

internal sealed class FailedToExtractOffersFromPageException : Exception
{
    public FailedToExtractOffersFromPageException() : this("Failed to extract offers from page")
    {

    }

    public FailedToExtractOffersFromPageException(string message) : base(message)
    {
    }

    public FailedToExtractOffersFromPageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
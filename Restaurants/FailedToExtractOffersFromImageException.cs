namespace debmenu.Restaurants;

internal sealed class FailedToExtractOffersFromImageException : Exception
{
    public FailedToExtractOffersFromImageException()
        : this("Failed to extract offers from image.")
    {

    }

    public FailedToExtractOffersFromImageException(string message) : base(message)
    {
    }

    public FailedToExtractOffersFromImageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

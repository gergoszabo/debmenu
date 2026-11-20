namespace debmenu.Restaurants;

/// <summary>
/// Thrown when offers cannot be extracted from a restaurant's source (HTML, image or API payload).
/// </summary>
public class OfferExtractionException : Exception
{
    public OfferExtractionException() : this(debmenu_Resources_ErrorMessages.OfferExtractionFailed)
    {
    }

    public OfferExtractionException(string? message) : base(message)
    {
    }

    public OfferExtractionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

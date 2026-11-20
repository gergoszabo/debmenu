namespace debmenu.Restaurants;

public class MannaHetiMenuNotFoundException : Exception
{
    public MannaHetiMenuNotFoundException() : this(debmenu_Resources_ErrorMessages.MannaHetiMenuNotFound)
    {
    }

    public MannaHetiMenuNotFoundException(string? message) : base(message)
    {
    }

    public MannaHetiMenuNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

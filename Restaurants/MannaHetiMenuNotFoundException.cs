namespace debmenu.Restaurants;

public class MannaHetiMenuNotFoundException : Exception
{
    public MannaHetiMenuNotFoundException() : this("Manna 'Heti menü' was not found.")
    {
    }

    public MannaHetiMenuNotFoundException(string? message) : base(message)
    {
    }

    public MannaHetiMenuNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

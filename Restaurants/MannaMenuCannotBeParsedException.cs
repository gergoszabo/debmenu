namespace debmenu.Restaurants;

public class MannaMenuCannotBeParsedException : Exception
{
    public MannaMenuCannotBeParsedException() : this(debmenu_Resources_ErrorMessages.MannaMenuCannotBeParsed)
    {
    }

    public MannaMenuCannotBeParsedException(string? message) : base(message)
    {
    }

    public MannaMenuCannotBeParsedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

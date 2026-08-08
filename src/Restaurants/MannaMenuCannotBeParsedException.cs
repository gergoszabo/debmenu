namespace debmenu.Restaurants;

public class MannaMenuCannotBeParsedException : Exception
{
    public MannaMenuCannotBeParsedException() : this("There was an error during Manna menu parsing")
    {
    }

    public MannaMenuCannotBeParsedException(string? message) : base(message)
    {
    }

    public MannaMenuCannotBeParsedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

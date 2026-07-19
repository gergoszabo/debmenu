namespace debmenu.Restaurants;

internal sealed class UnableToParseInfereceResponseException : Exception
{
    public UnableToParseInfereceResponseException()
        : this("Unable to parse json")
    {

    }

    public UnableToParseInfereceResponseException(string message) : base(message)
    {
    }

    public UnableToParseInfereceResponseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
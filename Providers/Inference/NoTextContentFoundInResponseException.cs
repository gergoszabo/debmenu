namespace debmenu.Providers.Inference;

internal sealed class NoTextContentFoundInResponseException : Exception
{
    public NoTextContentFoundInResponseException() : this("No text content found in the response.")
    {

    }

    public NoTextContentFoundInResponseException(string message) : base(message)
    {
    }

    public NoTextContentFoundInResponseException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
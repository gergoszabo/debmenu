namespace debmenu.Providers.Inference;

/// <summary>
/// Thrown when an inference provider fails to produce a usable result from the model
/// (e.g. the response contains no text content).
/// </summary>
public class InferenceException : Exception
{
    public InferenceException() : this(debmenu_Resources_ErrorMessages.InferenceFailed)
    {
    }

    public InferenceException(string? message) : base(message)
    {
    }

    public InferenceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

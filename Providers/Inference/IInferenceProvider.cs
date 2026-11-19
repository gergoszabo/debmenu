namespace debmenu.Providers.Inference;

public interface IInferenceProvider
{
    void AddContent(string content);
    void AddImage(byte[] imageBytes, string fileName);
    /// <summary>
    /// Discards any content/images added via <see cref="AddContent"/>/<see cref="AddImage"/>
    /// without performing an inference. Useful when a caller (e.g. a caching decorator) must
    /// drain the buffer without triggering a model call.
    /// </summary>
    void Reset();
    Task<InferenceResult?> Inference();
}

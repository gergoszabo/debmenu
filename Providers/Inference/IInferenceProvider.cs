namespace debmenu.Providers.Inference;

public interface IInferenceProvider
{
    /// <summary>
    /// The backend/model identity this provider instance is configured for
    /// (e.g. <c>llamacpp/unsloth/Qwen3.8-27B-GGUF</c>). Used to namespace the inference cache.
    /// </summary>
    string ProviderIdentity { get; }

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

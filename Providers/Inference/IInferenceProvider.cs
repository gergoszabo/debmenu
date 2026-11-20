namespace debmenu.Providers.Inference;

public interface IInferenceProvider
{
    /// <summary>
    /// The backend/model identity this provider instance is configured for
    /// (e.g. <c>llamacpp/unsloth/Qwen3.8-27B-GGUF</c>). Used to namespace the inference cache.
    /// </summary>
    string ProviderIdentity { get; }

    /// <summary>
    /// Buffers a text part to be included in the next inference call.
    /// </summary>
    /// <param name="content">The text content.</param>
    void AddContent(string content);

    /// <summary>
    /// Buffers an image part to be included in the next inference call.
    /// </summary>
    /// <param name="imageBytes">The raw image bytes.</param>
    /// <param name="fileName">The MIME type of the image, e.g. <c>image/png</c>.</param>
    void AddImage(byte[] imageBytes, string fileName);
    /// <summary>
    /// Discards any content/images added via <see cref="AddContent"/>/<see cref="AddImage"/>
    /// without performing an inference. Useful when a caller (e.g. a caching decorator) must
    /// drain the buffer without triggering a model call.
    /// </summary>
    void Reset();
    /// <summary>
    /// Runs the inference over all buffered parts and returns the result.
    /// </summary>
    /// <returns>The inference result, or null if the provider produced no usable output.</returns>
    Task<InferenceResult?> Inference();
}

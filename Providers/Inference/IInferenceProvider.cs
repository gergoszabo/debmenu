namespace debmenu.Providers.Inference;

public interface IInferenceProvider
{
    Task<InferenceResult?> Inference(string content);
    Task<InferenceResult?> Inference(string content, byte[] imageBytes, string mimeType);
}

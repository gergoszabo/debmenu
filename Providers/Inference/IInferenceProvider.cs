namespace debmenu.Providers.Inference;

public interface IInferenceProvider
{
    void AddContent(string content);
    void AddImage(byte[] imageBytes, string fileName);
    Task<string?> Inference();
}

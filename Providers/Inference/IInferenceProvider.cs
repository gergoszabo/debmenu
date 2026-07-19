namespace debmenu.Providers.Inference;

internal interface IInferenceProvider
{
    public void AddContent(string content);
    public void AddImage(byte[] imageBytes, string fileName);
    public Task<string?> Inference();
}

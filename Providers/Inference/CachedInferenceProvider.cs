using System.Security.Cryptography;
using System.Text;
using debmenu.Logging;
using Serilog;

namespace debmenu.Providers.Inference;

internal sealed class CachedInferenceProvider(IInferenceProvider inferenceProvider, ILogger logger) : IInferenceProvider
{
    private IInferenceProvider InferenceProvider { get; } = inferenceProvider;
    private ILogger Logger { get; } = logger;

    private List<string> StringContents { get; set; } = [];
    private List<Tuple<byte[], string>> ImageContents { get; set; } = [];

    public void AddContent(string content)
    {
        StringContents.Add(content);
        InferenceProvider.AddContent(content);
    }

    public void AddImage(byte[] imageBytes, string fileName)
    {
        ImageContents.Add(new(imageBytes, fileName));
        InferenceProvider.AddImage(imageBytes, fileName);
    }

    public async Task<string?> Inference()
    {
        using var op = new TimedOperation("CachedInferenceProvider Inference", [], Logger);

        List<byte> hashes = [];

        foreach (string content in StringContents)
        {
            hashes.AddRange(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
        }

        foreach (var content in ImageContents)
        {
            hashes.AddRange(SHA256.HashData(content.Item1));
            hashes.AddRange(SHA256.HashData(Encoding.UTF8.GetBytes(content.Item2)));
        }

        string finalHash = Convert.ToHexString(SHA256.HashData([.. hashes]));

        string cacheFileName = $"Cache/{finalHash}";

        if (!Directory.Exists(Path.GetDirectoryName(cacheFileName)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cacheFileName)!);
        }

        StringContents.Clear();
        ImageContents.Clear();

        if (File.Exists(cacheFileName))
        {
            return await File.ReadAllTextAsync(cacheFileName);
        }

        string? result = await InferenceProvider.Inference();

        await File.WriteAllTextAsync(cacheFileName, result);

        return result;
    }
}
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using debmenu.Logging;
using Serilog;

namespace debmenu.Providers.Inference;

public class CachedInferenceProvider(IInferenceProvider inferenceProvider, ILogger logger) : IInferenceProvider
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

    public async Task<InferenceResult?> Inference()
    {
        using var op = new TimedOperation("CachedInferenceProvider Inference", [], Logger);

        List<byte> hashes = [];

        foreach (var content in StringContents)
        {
            hashes.AddRange(SHA1.HashData(Encoding.UTF8.GetBytes(content)));
        }

        foreach (var content in ImageContents)
        {
            hashes.AddRange(SHA1.HashData(content.Item1));
            hashes.AddRange(SHA1.HashData(Encoding.UTF8.GetBytes(content.Item2)));
        }

        var finalHash = Convert.ToHexString(SHA1.HashData([.. hashes]));

        var cacheFileName = $"Cache/{finalHash}";

        if (!Directory.Exists(Path.GetDirectoryName(cacheFileName)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cacheFileName)!);
        }

        StringContents.Clear();
        ImageContents.Clear();

        if (File.Exists(cacheFileName))
        {
            var cached = JsonSerializer.Deserialize<InferenceResult>(await File.ReadAllTextAsync(cacheFileName));
            if (cached is not null)
                return cached with { PromptTokenCount = 0, CandidatesTokenCount = 0, TotalTokenCount = 0 };
            return cached;
        }

        var result = await InferenceProvider.Inference();

        var json = JsonSerializer.Serialize(result);
        await File.WriteAllTextAsync(cacheFileName, json);

        return result;
    }
}
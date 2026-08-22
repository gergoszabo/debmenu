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

    public async Task<InferenceResult?> Inference(string content)
    {
        using var op = new TimedOperation("[{Class}] {Method} {args}", [GetType().Name, nameof(Inference), string.Empty], Logger);
        return await Inference(content, [], string.Empty);
    }

    public async Task<InferenceResult?> Inference(string content, byte[] imageBytes, string mimeType)
    {
        using var op = new TimedOperation("[{Class}] {Method} {args}", [GetType().Name, nameof(Inference), mimeType], Logger);

        List<byte> hashes = [];

        hashes.AddRange(SHA1.HashData(Encoding.UTF8.GetBytes(content)));

        var hasImage = imageBytes.Length > 0 && !string.IsNullOrWhiteSpace(mimeType);
        if (hasImage)
        {
            hashes.AddRange(SHA1.HashData(imageBytes));
            hashes.AddRange(SHA1.HashData(Encoding.UTF8.GetBytes(mimeType)));
        }

        var finalHash = Convert.ToHexString(SHA1.HashData([.. hashes]));

        var cacheFileName = $"Cache/{finalHash}";

        if (!Directory.Exists(Path.GetDirectoryName(cacheFileName)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(cacheFileName)!);
        }

        if (File.Exists(cacheFileName))
        {
            var cached = JsonSerializer.Deserialize<InferenceResult>(await File.ReadAllTextAsync(cacheFileName));
            if (cached is not null)
                return cached with { PromptTokenCount = 0, CandidatesTokenCount = 0, TotalTokenCount = 0 };
            return cached;
        }

        var result = await (hasImage ? InferenceProvider.Inference(content, imageBytes, mimeType) : InferenceProvider.Inference(content));

        var json = JsonSerializer.Serialize(result);
        await File.WriteAllTextAsync(cacheFileName, json);

        return result;
    }
}

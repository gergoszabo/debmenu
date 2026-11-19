using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using debmenu.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace debmenu.Providers.Inference;

public class CachedInferenceProvider(IInferenceProvider inferenceProvider, ILogger logger,
    IOptions<GeminiOptions> geminiOptions, IOptions<LlamaCppOptions> llamaCppOptions) : IInferenceProvider
{
    private IInferenceProvider InferenceProvider { get; } = inferenceProvider;
    private ILogger Logger { get; } = logger;

    // Namespaces the cache directory per provider/model so results produced by one backend
    // are never served for a different one (e.g. after switching between Gemini and llama.cpp).
    private string CacheNamespace { get; } = GetCacheNamespace(geminiOptions.Value, llamaCppOptions.Value);

    private readonly List<string> _stringContents = [];
    private readonly List<Tuple<byte[], string>> _imageContents = [];

    public void AddContent(string content)
    {
        // Defer forwarding to the wrapped provider until Inference() actually needs it,
        // so a cache hit never leaves stale parts buffered in the real provider.
        _stringContents.Add(content);
    }

    public void AddImage(byte[] imageBytes, string fileName)
    {
        _imageContents.Add(new(imageBytes, fileName));
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _stringContents.Clear();
        _imageContents.Clear();
        InferenceProvider.Reset();
    }

    public async Task<InferenceResult?> Inference()
    {
        using var op = new TimedOperation("[{Class}] Inference", [GetType().Name], Logger);

        List<byte> hashes = [];

        foreach (var content in _stringContents)
        {
            hashes.AddRange(SHA1.HashData(Encoding.UTF8.GetBytes(content)));
        }

        foreach (var content in _imageContents)
        {
            hashes.AddRange(SHA1.HashData(content.Item1));
            hashes.AddRange(SHA1.HashData(Encoding.UTF8.GetBytes(content.Item2)));
        }

        var finalHash = Convert.ToHexString(SHA1.HashData([.. hashes]));

        var cacheFileName = Path.Combine("Cache", CacheNamespace, finalHash);

        var cacheDir = Path.GetDirectoryName(cacheFileName)!;
        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }

        if (File.Exists(cacheFileName))
        {
            // Cache hit: no model call, so discard the buffered parts and drain the wrapped
            // provider to make sure nothing leaks into a later inference.
            _stringContents.Clear();
            _imageContents.Clear();
            InferenceProvider.Reset();

            var cached = JsonSerializer.Deserialize<InferenceResult>(await File.ReadAllTextAsync(cacheFileName));
            if (cached is not null)
                return cached with { PromptTokenCount = 0, CandidatesTokenCount = 0, TotalTokenCount = 0 };
            return cached;
        }

        // Cache miss: forward the accumulated parts to the wrapped provider only now that we
        // know a real inference is happening, then run it.
        foreach (var content in _stringContents)
            InferenceProvider.AddContent(content);
        foreach (var content in _imageContents)
            InferenceProvider.AddImage(content.Item1, content.Item2);

        var result = await InferenceProvider.Inference();

        // The wrapped provider cleared its own buffer inside Inference(); drop our local copy too.
        _stringContents.Clear();
        _imageContents.Clear();

        var json = JsonSerializer.Serialize(result);
        await File.WriteAllTextAsync(cacheFileName, json);

        return result;
    }

    private static string GetCacheNamespace(GeminiOptions gemini, LlamaCppOptions llamaCpp)
    {
        // Both option sets are always bound in Program.cs; include whichever is configured.
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(gemini.ApiKey))
            parts.Add($"gemini/{gemini.Model}");
        if (!string.IsNullOrWhiteSpace(llamaCpp.BaseUrl))
            parts.Add($"llamacpp/{llamaCpp.Model}");
        return string.Join('-', parts);
    }
}
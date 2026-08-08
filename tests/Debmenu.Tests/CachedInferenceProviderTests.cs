using System.Security.Cryptography;
using System.Text;
using debmenu.Providers.Inference;
using NSubstitute;
using Serilog;

namespace Debmenu.Tests;

public class CachedInferenceProviderTests : IDisposable
{
    private readonly string _cacheDir = Path.Combine(Path.GetTempPath(), "debmenu-tests-" + Guid.NewGuid().ToString("N"));
    private readonly IInferenceProvider _inner = Substitute.For<IInferenceProvider>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    private CachedInferenceProvider CreateProvider() => new(_inner, _logger, _cacheDir);

    [Fact]
    public async Task Inference_FirstCall_HitsInnerAndReturnsResult()
    {
        var expected = new InferenceResult("offers json", 10, 20, 30);
        _inner.Inference().Returns(expected);

        var provider = CreateProvider();
        provider.AddContent("prompt");

        var result = await provider.Inference();

        Assert.Equal(expected, result);
        await _inner.Received(1).Inference();
    }

    [Fact]
    public async Task Inference_SecondCallSameContent_ReturnsCachedAndDoesNotHitInner()
    {
        var expected = new InferenceResult("offers json", 10, 20, 30);
        _inner.Inference().Returns(expected);

        var provider = CreateProvider();
        provider.AddContent("sametext");
        await provider.Inference();

        provider.AddContent("sametext");
        var result = await provider.Inference();

        Assert.Equal("offers json", result!.Text);
        Assert.Equal(0, result.PromptTokenCount);
        Assert.Equal(0, result.CandidatesTokenCount);
        Assert.Equal(0, result.TotalTokenCount);
        await _inner.Received(1).Inference();
    }

    [Fact]
    public void Inference_DifferentContent_ProducesDifferentCacheFile()
    {
        var p1 = CreateProvider();
        var p2 = CreateProvider();

        var file1 = ResolveCacheFileForContent("content-a");
        var file2 = ResolveCacheFileForContent("content-b");

        Assert.NotEqual(file1, file2);
    }

    [Fact]
    public void Inference_DifferentImageBytes_ProducesDifferentCacheFile()
    {
        var file1 = ResolveCacheFileForImage(new byte[] { 1, 2, 3 }, "a.png");
        var file2 = ResolveCacheFileForImage(new byte[] { 9, 9, 9 }, "a.png");

        Assert.NotEqual(file1, file2);
    }

    [Fact]
    public async Task Inference_CleansContentBetweenCalls()
    {
        _inner.Inference().Returns(new InferenceResult("{}", 0, 0, 0));

        var provider = CreateProvider();
        provider.AddContent("one");
        await provider.Inference();

        provider.AddContent("one");
        await provider.Inference();

        await _inner.Received(1).Inference();
    }

    private string ResolveCacheFileForContent(string content)
    {
        var hash = ComputeHash([content], []);
        return Path.Combine(_cacheDir, hash);
    }

    private string ResolveCacheFileForImage(byte[] bytes, string filename)
    {
        var hash = ComputeHash([], [(bytes, filename)]);
        return Path.Combine(_cacheDir, hash);
    }

    private static string ComputeHash(List<string> contents, List<(byte[] Bytes, string Name)> images)
    {
        List<byte> hashes = [];
        foreach (var c in contents)
            hashes.AddRange(SHA1.HashData(Encoding.UTF8.GetBytes(c)));

        foreach (var img in images)
        {
            hashes.AddRange(SHA1.HashData(img.Bytes));
            hashes.AddRange(SHA1.HashData(Encoding.UTF8.GetBytes(img.Name)));
        }

        return Convert.ToHexString(SHA1.HashData([.. hashes]));
    }

    public void Dispose()
    {
        if (Directory.Exists(_cacheDir))
            Directory.Delete(_cacheDir, recursive: true);
    }
}
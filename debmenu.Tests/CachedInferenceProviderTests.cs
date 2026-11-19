using debmenu;
using debmenu.Providers.Inference;
using Serilog;

namespace debmenu.Tests;

/// <summary>
/// Tests for <see cref="CachedInferenceProvider"/>: cache round-trip and corrupt-cache handling.
/// </summary>
[TestClass]
public class CachedInferenceProviderTests : TestContextBase
{
    private sealed class FakePathProvider(string root) : IPathProvider
    {
        public string Combine(params string[] segments) => Path.Combine([root, .. segments]);
    }

    private sealed class FakeInferenceProvider : IInferenceProvider
    {
        public string ProviderIdentity { get; set; } = "fake/model";

        public List<string> Contents { get; } = [];
        public List<Tuple<byte[], string>> Images { get; } = [];
        public int InferenceCallCount { get; private set; }
        public InferenceResult? ResultToReturn { get; set; }

        public void AddContent(string content) => Contents.Add(content);

        public void AddImage(byte[] imageBytes, string fileName) => Images.Add(new(imageBytes, fileName));

        public void Reset()
        {
            Contents.Clear();
            Images.Clear();
        }

        public Task<InferenceResult?> Inference()
        {
            InferenceCallCount++;
            Reset();
            return Task.FromResult(ResultToReturn);
        }
    }

    [TestMethod]
    public async Task Inference_WhenCacheMiss_CallsProvider_AndWritesCacheFile()
    {
        // Arrange
        var inner = new FakeInferenceProvider { ResultToReturn = new("answer", 10, 5, 15) };
        var provider = CreateProvider(inner);
        provider.AddContent("prompt");

        // Act
        var result = await provider.Inference();

        // Assert
        result.Should().Be(inner.ResultToReturn);
        inner.InferenceCallCount.Should().Be(1);
        Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories)
            .Should().ContainSingle()
            .Which.Should().StartWith(Path.Combine(_root, "Cache", "fake_model"));
    }

    [TestMethod]
    public async Task Inference_WhenSamePromptRepeats_ServesFromCache_WithoutCallingProvider()
    {
        // Arrange
        var inner = new FakeInferenceProvider { ResultToReturn = new("answer", 10, 5, 15) };
        var provider = CreateProvider(inner);
        provider.AddContent("prompt");

        // Act: first call populates the cache, second must hit it.
        await provider.Inference();
        provider.AddContent("prompt");
        var result = await provider.Inference();

        // Assert
        inner.InferenceCallCount.Should().Be(1);
        result!.Text.Should().Be("answer");
        result.PromptTokenCount.Should().Be(0);
        result.CandidatesTokenCount.Should().Be(0);
        result.TotalTokenCount.Should().Be(0);
    }

    [TestMethod]
    public async Task Inference_WhenDifferentPrompt_CallsProviderAgain()
    {
        // Arrange
        var inner = new FakeInferenceProvider { ResultToReturn = new("answer", 1, 1, 2) };
        var provider = CreateProvider(inner);
        provider.AddContent("prompt A");

        // Act
        await provider.Inference();
        provider.AddContent("prompt B");
        await provider.Inference();

        // Assert
        inner.InferenceCallCount.Should().Be(2);
    }

    [TestMethod]
    public async Task Inference_WhenCacheFileCorrupt_ThrowsInvalidOperationException()
    {
        // Arrange: create a cache file whose content does not deserialize to an InferenceResult.
        var inner = new FakeInferenceProvider();
        var provider = CreateProvider(inner);
        provider.AddContent("prompt");
        await provider.Inference();

        var cacheFile = Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories).Single();
        // Content that deserializes to null: the A4 corrupt-cache path.
        await File.WriteAllTextAsync(cacheFile, "null");

        // Act
        provider.AddContent("prompt");
        Func<Task> act = () => provider.Inference();

        // Assert: it throws, and no new inference happens (the earlier call already wrote the file).
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*could not be deserialized*");
        inner.InferenceCallCount.Should().Be(1);
    }

    private CachedInferenceProvider CreateProvider(FakeInferenceProvider inner)
        => new(inner, new FakePathProvider(_root), Log.Logger);
}

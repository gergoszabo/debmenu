using debmenu.Utils;

namespace debmenu.Tests;

/// <summary>
/// Tests for <see cref="AtomicFileWriter"/>: readers must never observe a partially written file.
/// </summary>
[TestClass]
public class AtomicFileWriterTests : TestContextBase
{
    [TestMethod]
    public async Task WriteTextAtomicAsync_CreatesMissingDirectories_AndWritesContent()
    {
        // Arrange
        var target = Path.Combine(_root, "nested", "dir", "file.json");

        // Act
        await AtomicFileWriter.WriteTextAtomicAsync(target, "{\"a\":1}");

        // Assert
        (await File.ReadAllTextAsync(target)).Should().Be("{\"a\":1}");
    }

    [TestMethod]
    public async Task WriteTextAtomicAsync_OverwritesExistingFile()
    {
        // Arrange
        Directory.CreateDirectory(_root);
        var target = Path.Combine(_root, "file.json");
        await File.WriteAllTextAsync(target, "old");

        // Act
        await AtomicFileWriter.WriteTextAtomicAsync(target, "new");

        // Assert
        (await File.ReadAllTextAsync(target)).Should().Be("new");
    }

    [TestMethod]
    public async Task WriteTextAtomicAsync_LeavesNoTempFilesBehind()
    {
        // Arrange
        var target = Path.Combine(_root, "file.json");

        // Act
        await AtomicFileWriter.WriteTextAtomicAsync(target, "content");

        // Assert: only the target file exists in the directory.
        Directory.EnumerateFiles(_root).Should().ContainSingle().Which.Should().Be(target);
    }
}

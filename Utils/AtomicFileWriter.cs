namespace debmenu.Utils;

/// <summary>
/// Helpers for writing files atomically (temp file + rename) so a crash mid-write never
/// leaves a truncated file behind that would poison later reads.
/// </summary>
public static class AtomicFileWriter
{
    /// <summary>
    /// Writes the text to <paramref name="filePath"/> by first writing to a temporary file in
    /// the same directory and then moving it over the target with overwrite, so readers never
    /// observe a partially written file. Creates the containing directory if needed.
    /// </summary>
    /// <param name="filePath">The destination file path.</param>
    /// <param name="content">The content to write.</param>
    public static async Task WriteTextAtomicAsync(string filePath, string content)
    {
        var dir = Path.GetDirectoryName(filePath)!;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var tmpPath = Path.Combine(dir, $".{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");
        await File.WriteAllTextAsync(tmpPath, content);

        // Atomic replace: readers see either the old or the new file, never a partial write.
        File.Move(tmpPath, filePath, overwrite: true);
    }
}

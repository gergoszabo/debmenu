namespace debmenu.Utils;

/// <summary>
/// Utility helpers for working with file paths and MIME types.
/// </summary>
public static class StringUtils
{
    /// <summary>
    /// Determines the MIME type of an image from its file extension.
    /// </summary>
    /// <param name="filePath">The path (or file name) whose extension determines the MIME type.</param>
    /// <returns>The corresponding image MIME type, e.g. <c>image/png</c>.</returns>
    /// <exception cref="NotSupportedException">Thrown when the file extension is not a supported image format.</exception>
    public static string GetMimeTypeFromFilePath(this string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".tiff" or ".tif" => "image/tiff",
            _ => throw new NotSupportedException($"File extension '{extension}' is not supported.")
        };
    }
}
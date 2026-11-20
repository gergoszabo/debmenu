namespace debmenu.Utils;

/// <summary>
/// Maps file extensions to MIME types.
/// </summary>
public static class MimeTypes
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
            _ => throw new NotSupportedException(string.Format(debmenu_Resources_ErrorMessages.UnsupportedFileExtension, extension))
        };
    }
}

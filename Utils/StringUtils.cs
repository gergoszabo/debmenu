namespace debmenu.Utils;

internal static class StringUtils
{
    public static string GetMimeTypeFromFilePath(this string filePath)
    {
        string extension = Path.GetExtension(filePath).ToUpperInvariant();
        return extension switch
        {
            ".JPG" or ".JPEG" => "image/jpeg",
            ".PNG" => "image/png",
            ".GIF" => "image/gif",
            ".BMP" => "image/bmp",
            ".TIFF" or ".TIF" => "image/tiff",
            _ => throw new NotSupportedException($"File extension '{extension}' is not supported.")
        };
    }
}
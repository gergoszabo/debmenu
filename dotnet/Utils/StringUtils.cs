namespace debmenu.Utils;

public static class StringUtils
{
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
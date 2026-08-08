using debmenu.Utils;

namespace Debmenu.Tests;

public class StringUtilsTests
{
    [Theory]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("photo.png", "image/png")]
    [InlineData("photo.gif", "image/gif")]
    [InlineData("photo.bmp", "image/bmp")]
    [InlineData("photo.tiff", "image/tiff")]
    [InlineData("photo.tif", "image/tiff")]
    public void GetMimeTypeFromFilePath_SupportedExtension_ReturnsMimeType(string path, string expected) =>
        Assert.Equal(expected, path.GetMimeTypeFromFilePath());

    [Theory]
    [InlineData("photo.PNG", "image/png")]
    [InlineData("photo.JPEG", "image/jpeg")]
    public void GetMimeTypeFromFilePath_UppercaseExtension_IsCaseInsensitive(string path, string expected)
        => Assert.Equal(expected, path.GetMimeTypeFromFilePath());

    [Theory]
    [InlineData("photo.webp")]
    [InlineData("photo.svg")]
    [InlineData("document.pdf")]
    [InlineData("noextension")]
    public void GetMimeTypeFromFilePath_UnsupportedExtension_Throws(string path)
        => Assert.Throws<NotSupportedException>(() => path.GetMimeTypeFromFilePath());
}
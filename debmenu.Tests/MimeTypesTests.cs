using debmenu.Utils;

namespace debmenu.Tests;

/// <summary>
/// Tests for <see cref="MimeTypes.GetMimeTypeFromFilePath(string)"/>.
/// </summary>
[TestClass]
public class MimeTypesTests
{
    [TestMethod]
    [DataRow("photo.jpg", "image/jpeg")]
    [DataRow("photo.jpeg", "image/jpeg")]
    [DataRow("photo.png", "image/png")]
    [DataRow("photo.gif", "image/gif")]
    [DataRow("photo.bmp", "image/bmp")]
    [DataRow("photo.tiff", "image/tiff")]
    [DataRow("photo.tif", "image/tiff")]
        public void GetMimeTypeFromFilePath_WithSupportedExtension_ReturnsMimeType(string filePath, string expected)
    {
        // Act
        var result = filePath.GetMimeTypeFromFilePath();

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void GetMimeTypeFromFilePath_WithUppercaseExtension_IsCaseInsensitive()
    {
        // Act
        var result = "PHOTO.JPG".GetMimeTypeFromFilePath();

        // Assert
        result.Should().Be("image/jpeg");
    }

    [TestMethod]
    public void GetMimeTypeFromFilePath_WithPathBeforeFile_UsesOnlyExtension()
{
        // Act
        var result = "/some/dir/photo.png".GetMimeTypeFromFilePath();

        // Assert
        result.Should().Be("image/png");
    }

    [TestMethod]
    [DataRow("menu.html")]
    [DataRow("data.csv")]
    [DataRow("noextension")]
    public void GetMimeTypeFromFilePath_WithUnsupportedExtension_ThrowsNotSupportedException(string filePath)
    {
        // Act
        Action act = () => filePath.GetMimeTypeFromFilePath();

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    [TestMethod]
    public void GetMimeTypeFromFilePath_WithNullPath_Throws()
    {
        // Arrange: the extension method dereferences its argument, so null fails with NRE.
        string? filePath = null;

        // Act
        Action act = () => filePath!.GetMimeTypeFromFilePath();

        // Assert
        act.Should().Throw<NullReferenceException>();
    }
}

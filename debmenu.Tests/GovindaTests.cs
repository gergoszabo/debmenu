using debmenu.Restaurants;

namespace debmenu.Tests;

/// <summary>
/// Tests for <see cref="Govinda.DissectMenuHtml"/>, which reduces the raw page to plain-text menu content.
/// </summary>
[TestClass]
public class GovindaTests
{
    [TestMethod]
    public void DissectMenuHtml_KeepsHeadingsAsMarkers_AndStripsMarkup()
    {
        // Arrange
        const string html = """
            <html><head><script>var x=1;</script></head>
            <body>
              <h2>Hétfő</h2>
              <p>Krémleves <img src="a.jpg" srcset="a-2x.jpg 2x"/></p>
              <style>.a{color:red}</style>
            </body></html>
            """;

        // Act
        var result = Govinda.DissectMenuHtml(html);

        // Assert
        result.Should().Contain("## Hétfő");
        result.Should().Contain("Krémleves");
        result.Should().NotContain("<h2>").And.NotContain("<script>").And.NotContain("<img").And.NotContain("<style>");
    }

    [TestMethod]
    public void DissectMenuHtml_DecodesHtmlEntities()
    {
        // Arrange
        const string html = "<html><body>Hétfő &amp; Kedd &uuml;</body></html>";

        // Act
        var result = Govinda.DissectMenuHtml(html);

        // Assert
        result.Should().Contain("Hétfő & Kedd ü");
    }

    [TestMethod]
    public void DissectMenuHtml_TrimsNavigationBeforeMenu_AndFooterAfterIt()
    {
        // Arrange: nav text lives in <head> (dropped by the body scope), footer after the marker.
        const string html = """
            <html><head><nav>Home About Contact</nav></head>
            <body>
              <h2>Hétfő</h2>
              <p>Lecsó</p>
              <footer>Gyere be ebédelni a belső udvarba!</footer>
            </body></html>
            """;

        // Act
        var result = Govinda.DissectMenuHtml(html);

        // Assert: nav (in head) is gone, everything from the marker onward is trimmed.
        var lines = result.Split('\n').ToList();
        lines[0].Should().Be("## Hétfő");
        lines.Should().ContainInOrder("## Hétfő", "Lecsó");
        result.Should().NotContain("Home About Contact");
    }

    [TestMethod]
    public void DissectMenuHtml_WithoutMarkerLines_ReturnsAllContent()
    {
        // Arrange: no "Hétfő"/"Gyere be ebédelni" markers, so no trimming happens.
        const string html = "<html><body><h2>Étcard</h2><p>Burger</p></body></html>";

        // Act
        var result = Govinda.DissectMenuHtml(html);

        // Assert
        result.Should().Contain("Étcard").And.Contain("Burger");
    }

    [TestMethod]
    public void DissectMenuHtml_EmptyInput_ReturnsEmptyString()
    {
        // Arrange/Act
        var result = Govinda.DissectMenuHtml(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void DissectMenuHtml_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? html = null;

        // Act
        Action act = () => Govinda.DissectMenuHtml(html!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}

using debmenu.Restaurants;

namespace debmenu.Tests;

/// <summary>
/// Tests for <see cref="Viktoria.DissectMenuHtml"/>, which scopes the page down to the menu section
/// and drops non-content tags.
/// </summary>
[TestClass]
public class ViktoriaTests
{
    [TestMethod]
    public void DissectMenuHtml_WithSiteMainSection_KeepsOnlyThatSection()
    {
        // Arrange
        const string html = """
            <html><body>
              <header>nav boilerplate</header>
              <div class="site-main">
                <h5>Hétfő</h5>
                <p>Krémleves</p>
              </div><!-- site-main end -->
              <footer>footer boilerplate</footer>
            </body></html>
            """;

        // Act
        var result = Viktoria.DissectMenuHtml(html);

        // Assert
        result.Should().Contain("Hétfő").And.Contain("Krémleves");
        result.Should().NotContain("nav boilerplate").And.NotContain("footer boilerplate");
    }

    [TestMethod]
    public void DissectMenuHtml_WithSiteMainSection_AndExplicitEndMarker_CutsAtMarker()
    {
        // Arrange
        const string html = """
            <html><body>
              <div class="site-main"><p>Menu</p></div>
              <aside class="site-main end">after marker</aside>
            </body></html>
            """;

        // Act
        var result = Viktoria.DissectMenuHtml(html);

        // Assert
        result.Should().Contain("Menu").And.NotContain("after marker");
    }

    [TestMethod]
    public void DissectMenuHtml_WithoutSiteMain_FallsBackToBody()
    {
        // Arrange
        const string html = """
            <html><head><title>t</title></head>
            <body><p>Body content</p></body>
            </html>
            """;

        // Act
        var result = Viktoria.DissectMenuHtml(html);

        // Assert
        result.Should().Contain("Body content").And.NotContain("<title>t</title>");
    }

    [TestMethod]
    public void DissectMenuHtml_WithoutSiteMainOrBody_ReturnsWholePage()
    {
        // Arrange
        const string html = "<p>Bare fragment</p>";

        // Act
        var result = Viktoria.DissectMenuHtml(html);

        // Assert
        result.Should().Be("<p>Bare fragment</p>");
    }

    [TestMethod]
    public void DissectMenuHtml_RemovesScripts_Styles_Noscripts_AndImages()
    {
        // Arrange
        const string html = """
            <div class="site-main">
              <script>alert(1)</script>
              <style>.x{}</style>
              <noscript>enable js</noscript>
              <img src="a.jpg" srcset="a-2x.jpg 2x"/>
              <p>Dish</p>
            </div>
            """;

        // Act
        var result = Viktoria.DissectMenuHtml(html);

        // Assert
        result.Should().Contain("Dish");
        result.Should().NotContain("alert(1)").And.NotContain(".x{}").And.NotContain("enable js")
            .And.NotContain("<img").And.NotContain("srcset");
    }

    [TestMethod]
    public void DissectMenuHtml_EmptyInput_ReturnsEmptyString()
    {
        // Arrange/Act
        var result = Viktoria.DissectMenuHtml(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    [TestMethod]
    public void DissectMenuHtml_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string? html = null;

        // Act
        Action act = () => Viktoria.DissectMenuHtml(html!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}

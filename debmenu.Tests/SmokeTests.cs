using debmenu.Restaurants;

namespace debmenu.Tests;

/// <summary>
/// Verifies the test harness itself: MSTest discovery, FluentAssertions,
/// and that InternalsVisibleTo grants access to internal members.
/// </summary>
[TestClass]
public class SmokeTests
{
    [TestMethod]
    public void InternalsAreVisible()
    {
        // Arrange
        const string html = "<html><body><h1>Menu</h1><p>Soup</p></body></html>";

        // Act
        var result = Govinda.DissectMenuHtml(html);

        // Assert
        result.Should().Contain("Menu");
    }
}

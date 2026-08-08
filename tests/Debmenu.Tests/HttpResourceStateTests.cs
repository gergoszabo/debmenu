using debmenu.Caching;

namespace Debmenu.Tests;

public class HttpResourceStateTests
{
    [Fact]
    public void Equals_SameEtagAndLastModified_AreEqual()
    {
        var a = new HttpResourceState("etag1", "2026-01-01");
        var b = new HttpResourceState("etag1", "2026-01-01");

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentValues_AreNotEqual()
    {
        var a = new HttpResourceState("etag1", "2026-01-01");
        var b = new HttpResourceState("etag2", "2026-01-01");
        var c = new HttpResourceState("etag1", "2026-02-02");

        Assert.NotEqual(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public void Equals_BothNull_AreEqual()
    {
        var a = new HttpResourceState(null, null);
        var b = new HttpResourceState(null, null);

        Assert.Equal(a, b);
    }
}
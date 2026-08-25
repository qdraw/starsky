using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public class NavigationServiceTests
{
    [Theory]
    [InlineData("http://localhost:4000/starsky?f=/photos", "")]
    [InlineData("http://localhost:9999", "")]
    public void IsAllowedOrigin_Localhost_ReturnsTrue(string url, string baseUrl)
    {
        Assert.True(NavigationService.IsAllowedOrigin(new Uri(url), baseUrl));
    }

    [Fact]
    public void IsAllowedOrigin_MatchingRemote_ReturnsTrue()
    {
        Assert.True(NavigationService.IsAllowedOrigin(new Uri("https://example.com/starsky?f=/"), "https://example.com"));
    }

    [Fact]
    public void IsAllowedOrigin_DifferentHost_ReturnsFalse()
    {
        Assert.False(NavigationService.IsAllowedOrigin(new Uri("https://evil.com"), "https://example.com"));
    }

    [Fact]
    public void BuildStartUrl_WithRoute_AppendsRoute()
    {
        var url = NavigationService.BuildStartUrl("http://localhost:4000", "?f=/photos");
        Assert.Equal("http://localhost:4000?f=/photos", url);
    }

    [Fact]
    public void BuildStartUrl_NullRoute_UsesDefault()
    {
        var url = NavigationService.BuildStartUrl("http://localhost:4000", null);
        Assert.Equal("http://localhost:4000?f=/", url);
    }

    [Fact]
    public void BuildStartUrl_TrailingSlash_Trimmed()
    {
        var url = NavigationService.BuildStartUrl("http://localhost:4000/", "?f=/");
        Assert.Equal("http://localhost:4000?f=/", url);
    }
}

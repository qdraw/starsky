using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public class NavigationServiceTests
{
    private static NavigationService CreateService(string remoteUrl = "")
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        settings.Current.RemoteBaseUrl = remoteUrl;
        return new NavigationService(settings);
    }

    [Theory]
    [InlineData("http://localhost:4000/starsky?f=/photos", "")]
    [InlineData("http://localhost:9999", "")]
    public void IsAllowedOrigin_Localhost_ReturnsTrue(string url, string baseUrl)
    {
        var svc = CreateService(baseUrl);
        Assert.True(svc.IsAllowedOrigin(new Uri(url), baseUrl));
    }

    [Fact]
    public void IsAllowedOrigin_MatchingRemote_ReturnsTrue()
    {
        var svc = CreateService("https://example.com");
        Assert.True(svc.IsAllowedOrigin(new Uri("https://example.com/starsky?f=/"), "https://example.com"));
    }

    [Fact]
    public void IsAllowedOrigin_DifferentHost_ReturnsFalse()
    {
        var svc = CreateService("https://example.com");
        Assert.False(svc.IsAllowedOrigin(new Uri("https://evil.com"), "https://example.com"));
    }

    [Fact]
    public void BuildStartUrl_WithRoute_AppendsRoute()
    {
        var svc = CreateService();
        var url = svc.BuildStartUrl("http://localhost:4000", "?f=/photos");
        Assert.Equal("http://localhost:4000?f=/photos", url);
    }

    [Fact]
    public void BuildStartUrl_NullRoute_UsesDefault()
    {
        var svc = CreateService();
        var url = svc.BuildStartUrl("http://localhost:4000", null);
        Assert.Equal("http://localhost:4000?f=/", url);
    }

    [Fact]
    public void BuildStartUrl_TrailingSlash_Trimmed()
    {
        var svc = CreateService();
        var url = svc.BuildStartUrl("http://localhost:4000/", "?f=/");
        Assert.Equal("http://localhost:4000?f=/", url);
    }
}

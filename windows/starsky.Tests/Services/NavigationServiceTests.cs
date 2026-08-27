using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
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

    [Fact]
    public void BuildStartUrl_RouteWithoutLeadingSlashOrQuery_PrependSlash()
    {
        var url = NavigationService.BuildStartUrl("http://localhost:4000", "starsky");
        Assert.Equal("http://localhost:4000/starsky", url);
    }

    [Fact]
    public void IsAllowedOrigin_EmptyBaseUrl_OnlyAllowsLocalhost()
    {
        Assert.False(NavigationService.IsAllowedOrigin(new Uri("https://example.com"), ""));
    }

    [Fact]
    public void IsAllowedOrigin_InvalidBaseUrl_ReturnsFalse()
    {
        Assert.False(NavigationService.IsAllowedOrigin(new Uri("https://example.com"), "not-a-url"));
    }

    [Fact]
    public void GetEffectiveBaseUrl_LocalMode_ReturnsLocalhostWithPort()
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        settings.Current.Mode = RuntimeMode.Local;
        var nav = new NavigationService(settings);

        var url = nav.GetEffectiveBaseUrl(9876);

        Assert.Equal("http://localhost:9876", url);
    }

    [Fact]
    public void GetEffectiveBaseUrl_RemoteMode_ReturnsRemoteUrl()
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        settings.Current.Mode = RuntimeMode.Remote;
        settings.Current.RemoteBaseUrl = "https://example.com/";
        var nav = new NavigationService(settings);

        var url = nav.GetEffectiveBaseUrl(9876);

        Assert.Equal("https://example.com", url);
    }

    [Fact]
    public void GetEffectiveBaseUrl_LocalModeNoPort_ReturnsRemoteUrl()
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        settings.Current.Mode = RuntimeMode.Local;
        settings.Current.RemoteBaseUrl = "https://example.com";
        var nav = new NavigationService(settings);

        var url = nav.GetEffectiveBaseUrl(null);

        Assert.Equal("https://example.com", url);
    }
}

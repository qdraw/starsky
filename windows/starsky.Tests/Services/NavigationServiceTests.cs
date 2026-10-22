using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

[TestClass]
public class NavigationServiceTests
{
    [TestMethod]
    [DataRow("http://localhost:4000/starsky?f=/photos", "")]
    [DataRow("http://localhost:9999", "")]
    public void IsAllowedOrigin_Localhost_ReturnsTrue(string url, string baseUrl)
    {
        Assert.IsTrue(NavigationService.IsAllowedOrigin(new Uri(url), baseUrl));
    }

    [TestMethod]
    public void IsAllowedOrigin_MatchingRemote_ReturnsTrue()
    {
        Assert.IsTrue(NavigationService.IsAllowedOrigin(new Uri("https://example.com/starsky?f=/"), "https://example.com"));
    }

    [TestMethod]
    public void IsAllowedOrigin_DifferentHost_ReturnsFalse()
    {
        Assert.IsFalse(NavigationService.IsAllowedOrigin(new Uri("https://evil.com"), "https://example.com"));
    }

    [TestMethod]
    public void BuildStartUrl_WithRoute_AppendsRoute()
    {
        var url = NavigationService.BuildStartUrl("http://localhost:4000", "?f=/photos");
        Assert.AreEqual("http://localhost:4000?f=/photos", url);
    }

    [TestMethod]
    public void BuildStartUrl_NullRoute_UsesDefault()
    {
        var url = NavigationService.BuildStartUrl("http://localhost:4000", null);
        Assert.AreEqual("http://localhost:4000?f=/", url);
    }

    [TestMethod]
    public void BuildStartUrl_TrailingSlash_Trimmed()
    {
        var url = NavigationService.BuildStartUrl("http://localhost:4000/", "?f=/");
        Assert.AreEqual("http://localhost:4000?f=/", url);
    }

    [TestMethod]
    public void BuildStartUrl_RouteWithoutLeadingSlashOrQuery_PrependSlash()
    {
        var url = NavigationService.BuildStartUrl("http://localhost:4000", "starsky");
        Assert.AreEqual("http://localhost:4000/starsky", url);
    }

    [TestMethod]
    public void IsAllowedOrigin_EmptyBaseUrl_OnlyAllowsLocalhost()
    {
        Assert.IsFalse(NavigationService.IsAllowedOrigin(new Uri("https://example.com"), ""));
    }

    [TestMethod]
    public void IsAllowedOrigin_InvalidBaseUrl_ReturnsFalse()
    {
        Assert.IsFalse(NavigationService.IsAllowedOrigin(new Uri("https://example.com"), "not-a-url"));
    }

    [TestMethod]
    public void GetEffectiveBaseUrl_LocalMode_ReturnsLocalhostWithPort()
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        settings.Current.Mode = RuntimeMode.Local;
        var nav = new NavigationService(settings);

        var url = nav.GetEffectiveBaseUrl(9876);

        Assert.AreEqual("http://localhost:9876", url);
    }

    [TestMethod]
    public void GetEffectiveBaseUrl_RemoteMode_ReturnsRemoteUrl()
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        settings.Current.Mode = RuntimeMode.Remote;
        settings.Current.RemoteBaseUrl = "https://example.com/";
        var nav = new NavigationService(settings);

        var url = nav.GetEffectiveBaseUrl(9876);

        Assert.AreEqual("https://example.com", url);
    }

    [TestMethod]
    public void GetEffectiveBaseUrl_LocalModeNoPort_ReturnsRemoteUrl()
    {
        var settings = new SettingsService(NullLogger<SettingsService>.Instance);
        settings.Current.Mode = RuntimeMode.Local;
        settings.Current.RemoteBaseUrl = "https://example.com";
        var nav = new NavigationService(settings);

        var url = nav.GetEffectiveBaseUrl(null);

        Assert.AreEqual("https://example.com", url);
    }
}

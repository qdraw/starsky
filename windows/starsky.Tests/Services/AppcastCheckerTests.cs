using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public class AppcastCheckerTests
{
    private const string Ns = "http://www.andymatuschak.org/xml-namespaces/sparkle";

    private static string BuildAppcast(string version, bool includeVersion = true, bool includeShortVersion = true) => $"""
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0" xmlns:sparkle="{Ns}">
          <channel>
            <title>Starsky</title>
            <item>
              <title>{version}</title>
              <pubDate>Thu, 03 Sep 2026 07:27:39 +0000</pubDate>
              {(includeShortVersion ? $"<sparkle:shortVersionString>{version}</sparkle:shortVersionString>" : "")}
              {(includeVersion ? $"<sparkle:version>{version}</sparkle:version>" : "")}
              <sparkle:minimumSystemVersion>13.0.0</sparkle:minimumSystemVersion>
              <enclosure url="https://github.com/qdraw/starsky/releases/download/v{version}/starsky-mac-universal-desktop.dmg"
                         sparkle:edSignature="sig==" length="12345" type="application/octet-stream"/>
            </item>
          </channel>
        </rss>
        """;

    [Fact]
    public void FindNewerRelease_ReturnsBeta3_WhenCurrentIsBeta1()
    {
        var xml = BuildAppcast("0.9.0-beta.3");

        var result = AppcastChecker.FindNewerRelease(xml, "0.9.0-beta.1");

        Assert.NotNull(result);
        Assert.Equal("0.9.0-beta.3", result.Value.Version);
        Assert.Equal("https://github.com/qdraw/starsky/releases/tag/v0.9.0-beta.3", result.Value.HtmlUrl);
    }

    [Fact]
    public void FindNewerRelease_ReturnsNull_WhenAlreadyOnLatest()
    {
        var xml = BuildAppcast("0.9.0-beta.1");

        var result = AppcastChecker.FindNewerRelease(xml, "0.9.0-beta.1");

        Assert.Null(result);
    }

    [Fact]
    public void FindNewerRelease_ReturnsNull_WhenCurrentIsNewer()
    {
        var xml = BuildAppcast("0.9.0-beta.1");

        var result = AppcastChecker.FindNewerRelease(xml, "0.9.0-beta.3");

        Assert.Null(result);
    }

    [Fact]
    public void FindNewerRelease_PrefersShortVersionString_OverVersion()
    {
        // shortVersionString = beta.3, sparkle:version = beta.1 (mismatch — shortVersionString wins)
        const string xml = $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0" xmlns:sparkle="{{Ns}}">
              <channel><item>
                <sparkle:shortVersionString>0.9.0-beta.3</sparkle:shortVersionString>
                <sparkle:version>0.9.0-beta.1</sparkle:version>
              </item></channel>
            </rss>
            """;

        var result = AppcastChecker.FindNewerRelease(xml, "0.9.0-beta.1");

        Assert.NotNull(result);
        Assert.Equal("0.9.0-beta.3", result.Value.Version);
    }

    [Fact]
    public void FindNewerRelease_FallsBackToVersion_WhenShortVersionStringAbsent()
    {
        var xml = BuildAppcast("0.9.0-beta.3", includeVersion: true, includeShortVersion: false);

        var result = AppcastChecker.FindNewerRelease(xml, "0.9.0-beta.1");

        Assert.NotNull(result);
        Assert.Equal("0.9.0-beta.3", result.Value.Version);
    }

    [Fact]
    public void FindNewerRelease_ReturnsNull_WhenNoVersionElements()
    {
        const string xml = """
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0"><channel><item><title>no version</title></item></channel></rss>
            """;

        var result = AppcastChecker.FindNewerRelease(xml, "0.9.0-beta.1");

        Assert.Null(result);
    }

    [Fact]
    public void FindNewerRelease_ReturnsNull_WhenXmlIsInvalid()
    {
        var result = AppcastChecker.FindNewerRelease("not xml at all", "0.9.0");

        Assert.Null(result);
    }

    [Fact]
    public void FindNewerRelease_ReturnsNull_WhenXmlIsEmpty()
    {
        var result = AppcastChecker.FindNewerRelease(string.Empty, "0.9.0");

        Assert.Null(result);
    }

    [Fact]
    public void FindNewerRelease_StableVersionNewerThanPreRelease()
    {
        var xml = BuildAppcast("0.9.0");

        var result = AppcastChecker.FindNewerRelease(xml, "0.9.0-beta.3");

        Assert.NotNull(result);
        Assert.Equal("0.9.0", result.Value.Version);
    }

    [Fact]
    public void FindNewerRelease_UsesFirstItemWithNewerVersion()
    {
        // Two items — first is newer
        const string xml = $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <rss version="2.0" xmlns:sparkle="{{Ns}}">
              <channel>
                <item><sparkle:shortVersionString>0.9.0-beta.3</sparkle:shortVersionString></item>
                <item><sparkle:shortVersionString>0.9.0-beta.2</sparkle:shortVersionString></item>
              </channel>
            </rss>
            """;

        var result = AppcastChecker.FindNewerRelease(xml, "0.9.0-beta.1");

        Assert.NotNull(result);
        Assert.Equal("0.9.0-beta.3", result.Value.Version);
    }

    [Fact]
    public void FindNewerRelease_HtmlUrlPointsToGitHubRelease()
    {
        var xml = BuildAppcast("1.0.0");

        var result = AppcastChecker.FindNewerRelease(xml, "0.9.0");

        Assert.NotNull(result);
        Assert.Equal("https://github.com/qdraw/starsky/releases/tag/v1.0.0", result.Value.HtmlUrl);
    }
}

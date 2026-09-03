using System.Net.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public sealed class UpdateServiceTests : IDisposable
{
    private readonly string _tempFile;
    private readonly SettingsService _settings;

    private const string Ns = "http://www.andymatuschak.org/xml-namespaces/sparkle";

    public UpdateServiceTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"starsky-update-{Guid.NewGuid()}.json");
        _settings = new SettingsService(NullLogger<SettingsService>.Instance, _tempFile);
        _settings.Load();
    }

    private UpdateService CreateService(Func<string, Task<string>>? httpGet = null) =>
        new UpdateService(_settings, NullLogger<UpdateService>.Instance, httpGet);

    private static string AppcastXml(string version) => $"""
        <?xml version="1.0" encoding="utf-8"?>
        <rss version="2.0" xmlns:sparkle="{Ns}">
          <channel><item>
            <sparkle:shortVersionString>{version}</sparkle:shortVersionString>
          </item></channel>
        </rss>
        """;

    // Bypasses Velopack + appcast entirely — used for testing CheckAsync routing only
    private sealed class FakeUpdateService(SettingsService settings, bool hasUpdate = false, bool canApply = false)
        : UpdateService(settings, NullLogger<UpdateService>.Instance)
    {
        protected override Task<bool> CheckWithVelopackAsync() => Task.FromResult(hasUpdate);
        protected override bool HasPendingUpdate => canApply;
        protected override Task DoApplyUpdateAsync() => Task.CompletedTask;
    }

    // --- CheckAsync routing ---

    [Fact]
    public async Task CheckAsync_WhenUpdateCheckDisabled_ReturnsFalse()
    {
        _settings.Current.UpdateCheckEnabled = false;

        Assert.False(await CreateService().CheckAsync());
    }

    [Fact]
    public async Task CheckAsync_WhenWarningShownRecently_ReturnsFalse()
    {
        _settings.Current.UpdateCheckEnabled = true;
        _settings.Current.LastUpdateWarningShown = DateTime.UtcNow.AddMinutes(-1);

        Assert.False(await CreateService().CheckAsync());
    }

    [Fact]
    public async Task CheckAsync_WhenUpdateAvailable_ReturnsTrue()
    {
        _settings.Current.UpdateCheckEnabled = true;

        Assert.True(await new FakeUpdateService(_settings, hasUpdate: true).CheckAsync());
    }

    [Fact]
    public async Task CheckAsync_WhenNoUpdate_ReturnsFalse()
    {
        _settings.Current.UpdateCheckEnabled = true;

        Assert.False(await new FakeUpdateService(_settings, hasUpdate: false).CheckAsync());
    }

    [Fact]
    public async Task CheckAsync_WhenWarningShownLongAgo_ProceedsToCheck()
    {
        _settings.Current.UpdateCheckEnabled = true;
        _settings.Current.LastUpdateWarningShown = DateTime.UtcNow.AddDays(-30);

        // Falls through to Velopack (unavailable in CI) then appcast stub — must not throw
        var svc = CreateService(httpGet: _ => Task.FromResult(AppcastXml("0.0.0")));
        var ex = await Record.ExceptionAsync(() => svc.CheckAsync());

        Assert.Null(ex);
    }

    // --- ApplyUpdateAsync ---

    [Fact]
    public async Task ApplyUpdateAsync_WhenNoPendingUpdate_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().ApplyUpdateAsync());
    }

    [Fact]
    public async Task ApplyUpdateAsync_WhenReadyToApply_DoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(() =>
            new FakeUpdateService(_settings, canApply: true).ApplyUpdateAsync());

        Assert.Null(ex);
    }

    // --- RecordWarningShown ---

    [Fact]
    public void RecordWarningShown_SetsTimestamp()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        CreateService().RecordWarningShown();

        var loaded = new SettingsService(NullLogger<SettingsService>.Instance, _tempFile);
        loaded.Load();
        Assert.NotNull(loaded.Current.LastUpdateWarningShown);
        Assert.True(loaded.Current.LastUpdateWarningShown >= before);
    }

    // --- UpdatePreRelease setting ---

    [Fact]
    public void UpdatePreRelease_DefaultsToFalse()
    {
        Assert.False(_settings.Current.UpdatePreRelease);
    }

    [Fact]
    public void UpdatePreRelease_IsPersisted()
    {
        _settings.Current.UpdatePreRelease = true;
        _settings.Save();

        var loaded = new SettingsService(NullLogger<SettingsService>.Instance, _tempFile);
        loaded.Load();

        Assert.True(loaded.Current.UpdatePreRelease);
    }

    [Fact]
    public async Task CheckAsync_WhenPreReleaseEnabled_ProceedsToCheck()
    {
        _settings.Current.UpdateCheckEnabled = true;
        _settings.Current.UpdatePreRelease = true;

        Assert.True(await new FakeUpdateService(_settings, hasUpdate: true).CheckAsync());
    }

    // --- TryAppcastFallbackAsync (internal — tests CheckAppcastAsync + result wiring) ---

    [Fact]
    public async Task TryAppcastFallbackAsync_WhenFeedReturnsNewerVersion_ReturnsTrueAndSetsUrl()
    {
        var svc = CreateService(httpGet: _ => Task.FromResult(AppcastXml("999.0.0")));

        var result = await svc.TryAppcastFallbackAsync();

        Assert.True(result);
        Assert.True(svc.IsGitHubFallbackUpdate);
        Assert.Equal("https://github.com/qdraw/starsky/releases/tag/v999.0.0", svc.PendingGitHubReleaseUrl);
    }

    [Fact]
    public async Task TryAppcastFallbackAsync_WhenFeedReturnsOlderVersion_ReturnsFalse()
    {
        var svc = CreateService(httpGet: _ => Task.FromResult(AppcastXml("0.0.0")));

        var result = await svc.TryAppcastFallbackAsync();

        Assert.False(result);
        Assert.Null(svc.PendingGitHubReleaseUrl);
    }

    [Fact]
    public async Task TryAppcastFallbackAsync_WhenPreReleaseEnabled_PassesQueryParam()
    {
        _settings.Current.UpdatePreRelease = true;
        string? capturedUrl = null;
        var svc = CreateService(httpGet: url =>
        {
            capturedUrl = url;
            return Task.FromResult(AppcastXml("0.0.0"));
        });

        await svc.TryAppcastFallbackAsync();

        Assert.NotNull(capturedUrl);
        Assert.Contains("?pre-release=1", capturedUrl);
    }

    [Fact]
    public async Task TryAppcastFallbackAsync_WhenPreReleaseDisabled_OmitsQueryParam()
    {
        _settings.Current.UpdatePreRelease = false;
        string? capturedUrl = null;
        var svc = CreateService(httpGet: url =>
        {
            capturedUrl = url;
            return Task.FromResult(AppcastXml("0.0.0"));
        });

        await svc.TryAppcastFallbackAsync();

        Assert.NotNull(capturedUrl);
        Assert.DoesNotContain("?pre-release=1", capturedUrl);
    }

    [Fact]
    public async Task TryAppcastFallbackAsync_WhenHttpThrows_PropagatesException()
    {
        var svc = CreateService(httpGet: _ => Task.FromException<string>(new HttpRequestException("network error")));

        await Assert.ThrowsAsync<HttpRequestException>(() => svc.TryAppcastFallbackAsync());
    }

    public void Dispose()
    {
        try { File.Delete(_tempFile); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}

using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public sealed class UpdateServiceTests : IDisposable
{
    private readonly string _tempFile;
    private readonly SettingsService _settings;

    public UpdateServiceTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"starsky-update-{Guid.NewGuid()}.json");
        _settings = new SettingsService(NullLogger<SettingsService>.Instance, _tempFile);
        _settings.Load();
    }

    private UpdateService CreateService() =>
        new UpdateService(_settings, NullLogger<UpdateService>.Instance);

    // Test subclass that bypasses Velopack runtime
    private sealed class FakeUpdateService(SettingsService settings, bool hasUpdate = false, bool canApply = false)
        : UpdateService(settings, NullLogger<UpdateService>.Instance)
    {
        protected override Task<bool> CheckWithVelopackAsync() => Task.FromResult(hasUpdate);
        protected override bool HasPendingUpdate => canApply;
        protected override Task DoApplyUpdateAsync() => Task.CompletedTask;
    }

    [Fact]
    public async Task CheckAsync_WhenUpdateCheckDisabled_ReturnsFalse()
    {
        _settings.Current.UpdateCheckEnabled = false;
        var svc = CreateService();

        var result = await svc.CheckAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task CheckAsync_WhenWarningShownRecently_ReturnsFalse()
    {
        _settings.Current.UpdateCheckEnabled = true;
        _settings.Current.LastUpdateWarningShown = DateTime.UtcNow.AddMinutes(-1);
        var svc = CreateService();

        var result = await svc.CheckAsync();

        Assert.False(result);
    }

    [Fact]
    public void RecordWarningShown_SetsTimestamp()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var svc = CreateService();

        svc.RecordWarningShown();

        var loaded = new SettingsService(NullLogger<SettingsService>.Instance, _tempFile);
        loaded.Load();
        Assert.NotNull(loaded.Current.LastUpdateWarningShown);
        Assert.True(loaded.Current.LastUpdateWarningShown >= before);
    }

    [Fact]
    public async Task ApplyUpdateAsync_WhenNoPendingUpdate_Throws()
    {
        var svc = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.ApplyUpdateAsync());
    }

    [Fact]
    public async Task CheckAsync_WhenEnabledAndNoRecentWarning_ReturnsFalse_WhenVelopackUnavailable()
    {
        _settings.Current.UpdateCheckEnabled = true;
        _settings.Current.LastUpdateWarningShown = null;
        var svc = CreateService();

        // Velopack is not installed in CI — manager gracefully returns false
        var result = await svc.CheckAsync();

        Assert.False(result);
    }

    [Fact]
    public async Task CheckAsync_WhenWarningShownLongAgo_ProceedsToCheck()
    {
        _settings.Current.UpdateCheckEnabled = true;
        _settings.Current.LastUpdateWarningShown = DateTime.UtcNow.AddDays(-30);
        var svc = CreateService();

        // Falls through to Velopack which is unavailable in CI — just must not throw
        var ex = await Record.ExceptionAsync(() => svc.CheckAsync());

        Assert.Null(ex);
    }

    [Fact]
    public async Task CheckAsync_WhenUpdateAvailable_ReturnsTrue()
    {
        _settings.Current.UpdateCheckEnabled = true;
        var svc = new FakeUpdateService(_settings, hasUpdate: true);

        Assert.True(await svc.CheckAsync());
    }

    [Fact]
    public async Task CheckAsync_WhenNoUpdate_ReturnsFalse()
    {
        _settings.Current.UpdateCheckEnabled = true;
        var svc = new FakeUpdateService(_settings, hasUpdate: false);

        Assert.False(await svc.CheckAsync());
    }

    [Fact]
    public async Task ApplyUpdateAsync_WhenReadyToApply_DoesNotThrow()
    {
        var svc = new FakeUpdateService(_settings, canApply: true);

        var ex = await Record.ExceptionAsync(() => svc.ApplyUpdateAsync());

        Assert.Null(ex);
    }

    public void Dispose()
    {
        try { File.Delete(_tempFile); } catch { /* best-effort */ }
        GC.SuppressFinalize(this);
    }
}

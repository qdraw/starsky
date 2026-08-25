using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Services;

namespace starsky.Tests.Services;

public class UpdateServiceTests : IDisposable
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

    public void Dispose()
    {
        try { File.Delete(_tempFile); } catch { /* best-effort */ }
    }
}

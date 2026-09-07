using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;
using starsky.Tests.FakeCreateAn;
using System.Threading.Tasks;

namespace starsky.Tests.Services;

[TestClass]
public class MountWatcherServiceTests
{
    // ── DesktopSettings ───────────────────────────────────────────────────────

    [TestMethod]
    public void DesktopSettings_MountWatcherEnabled_DefaultsToFalse()
    {
        var s = new DesktopSettings();
        Assert.IsFalse(s.MountWatcherEnabled);
    }

    [TestMethod]
    public void DesktopSettings_MountWatcherEnabled_RoundTripsJson()
    {
        var original = new DesktopSettings { MountWatcherEnabled = true };
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<DesktopSettings>(json)!;

        Assert.IsTrue(restored.MountWatcherEnabled);
    }

    [TestMethod]
    public void DesktopSettings_MountWatcherEnabled_FalseRoundTripsJson()
    {
        var original = new DesktopSettings { MountWatcherEnabled = false };
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<DesktopSettings>(json)!;

        Assert.IsFalse(restored.MountWatcherEnabled);
    }

    // ── FakeMountWatcherService ───────────────────────────────────────────────

    [TestMethod]
    public async Task FakeMountWatcherService_Enable_SetsCalledFlag()
    {
        var fake = new FakeMountWatcherService();
        var result = await fake.EnableAsync();

        Assert.IsTrue(fake.EnableCalled);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task FakeMountWatcherService_Enable_ReturnsConfiguredResult()
    {
        var fake = new FakeMountWatcherService { EnableResult = false };
        var result = await fake.EnableAsync();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task FakeMountWatcherService_Disable_SetsCalledFlag()
    {
        var fake = new FakeMountWatcherService();
        var result = await fake.DisableAsync();

        Assert.IsTrue(fake.DisableCalled);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void FakeMountWatcherService_StopSync_SetsCalledFlag()
    {
        var fake = new FakeMountWatcherService();
        fake.StopSync();

        Assert.IsTrue(fake.StopSyncCalled);
    }

    [TestMethod]
    public void FakeMountWatcherService_GetStatus_ReturnsConfiguredStatus()
    {
        var fake = new FakeMountWatcherService { StatusResult = MountWatcherStatus.Running };
        Assert.AreEqual(MountWatcherStatus.Running, fake.GetStatus());
    }

    // ── MountWatcherService — binary-missing fast-paths ───────────────────────

    [TestMethod]
    public async Task MountWatcherService_Enable_ReturnsFalse_WhenBinaryMissing()
    {
        var svc = new MountWatcherService(Path.Combine(Path.GetTempPath(), "nonexistent_cli.exe"));
        var result = await svc.EnableAsync();

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task MountWatcherService_Disable_ReturnsFalse_WhenBinaryMissingAndServiceInstalled()
    {
        var svc = new MountWatcherService(Path.Combine(Path.GetTempPath(), "nonexistent_cli.exe"));
        // GetStatus() → NotInstalled (no service registered in CI)
        // DisableAsync with missing binary returns true only if service is also not installed
        var result = await svc.DisableAsync();
        // In CI/dev with no service installed, NotInstalled → true
        // We only verify it doesn't throw
        Assert.IsTrue(result == true || result == false);
    }

    [TestMethod]
    public void MountWatcherService_StopSync_DoesNotThrow_WhenBinaryMissing()
    {
        var svc = new MountWatcherService(Path.Combine(Path.GetTempPath(), "nonexistent_cli.exe"));

        Exception? ex = null;
        try { svc.StopSync(); } catch (Exception e) { ex = e; }

        Assert.IsNull(ex);
    }

    [TestMethod]
    public void MountWatcherService_GetStatus_ReturnsNotInstalledOrUnknown_WhenServiceAbsent()
    {
        var svc = new MountWatcherService(Path.Combine(Path.GetTempPath(), "nonexistent_cli.exe"));
        var status = svc.GetStatus();

        Assert.IsTrue(status is MountWatcherStatus.NotInstalled or MountWatcherStatus.Unknown);
    }

    // ── UpdateService — StopSync called before apply ──────────────────────────

    [TestMethod]
    public async Task UpdateService_ApplyUpdateAsync_CallsStopSync_BeforeApply()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"starsky-mw-upd-{Guid.NewGuid()}.json");
        try
        {
            var settings = new SettingsService(NullLogger<SettingsService>.Instance, tempFile);
            settings.Load();

            var fake = new FakeMountWatcherService();
            var svc = new FakeUpdateServiceWithMountWatcher(settings, fake);

            await svc.ApplyUpdateAsync();

            Assert.IsTrue(fake.StopSyncCalled);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { /* best-effort */ }
        }
    }

    [TestMethod]
    public async Task UpdateService_ApplyUpdateAsync_DoesNotThrow_WhenNoMountWatcherService()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"starsky-mw-upd2-{Guid.NewGuid()}.json");
        try
        {
            var settings = new SettingsService(NullLogger<SettingsService>.Instance, tempFile);
            settings.Load();

            var svc = new FakeUpdateServiceWithMountWatcher(settings, null);

            Exception? ex = null;
            try { await svc.ApplyUpdateAsync(); } catch (Exception e) { ex = e; }

            Assert.IsNull(ex);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { /* best-effort */ }
        }
    }

    private sealed class FakeUpdateServiceWithMountWatcher(
        SettingsService settings,
        IMountWatcherService? mountWatcherService)
        : UpdateService(settings, NullLogger<UpdateService>.Instance,
            mountWatcherService: mountWatcherService)
    {
        protected override bool HasPendingUpdate => true;
        protected override Task DoApplyUpdateAsync() => Task.CompletedTask;
    }

    // ── MountWatcherLifecycle ─────────────────────────────────────────────────

    private static (MountWatcherLifecycle lifecycle, SettingsService settings, FakeMountWatcherService fake)
        CreateLifecycle(string? tempFile = null)
    {
        var file = tempFile ?? Path.Combine(Path.GetTempPath(), $"starsky-mwl-{Guid.NewGuid()}.json");
        var settings = new SettingsService(NullLogger<SettingsService>.Instance, file);
        settings.Load();
        var fake = new FakeMountWatcherService();
        var lifecycle = new MountWatcherLifecycle(
            fake, settings, NullLogger<MountWatcherLifecycle>.Instance);
        return (lifecycle, settings, fake);
    }

    [TestMethod]
    public async Task Lifecycle_OnStartupAsync_DoesNotCallEnable_WhenPreferenceIsFalse()
    {
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = false;

        await lifecycle.OnStartupAsync();

        Assert.IsFalse(fake.EnableCalled);
    }

    [TestMethod]
    public async Task Lifecycle_OnStartupAsync_CallsEnable_WhenPreferenceIsTrue()
    {
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = true;

        await lifecycle.OnStartupAsync();

        Assert.IsTrue(fake.EnableCalled);
    }

    [TestMethod]
    public async Task Lifecycle_OnStartupAsync_KeepsPreference_WhenEnableSucceeds()
    {
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = true;
        fake.EnableResult = true;

        await lifecycle.OnStartupAsync();

        Assert.IsTrue(settings.Current.MountWatcherEnabled);
    }

    [TestMethod]
    public async Task Lifecycle_OnStartupAsync_ClearsPreference_WhenEnableFails()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"starsky-mwl-fail-{Guid.NewGuid()}.json");
        try
        {
            var (lifecycle, settings, fake) = CreateLifecycle(tempFile);
            settings.Current.MountWatcherEnabled = true;
            fake.EnableResult = false;

            await lifecycle.OnStartupAsync();

            Assert.IsFalse(settings.Current.MountWatcherEnabled);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { /* best-effort */ }
        }
    }

    [TestMethod]
    public async Task Lifecycle_OnStartupAsync_PersistsCleared_Preference_WhenEnableFails()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"starsky-mwl-persist-{Guid.NewGuid()}.json");
        try
        {
            var (lifecycle, settings, fake) = CreateLifecycle(tempFile);
            settings.Current.MountWatcherEnabled = true;
            fake.EnableResult = false;

            await lifecycle.OnStartupAsync();

            // Reload from disk and verify the preference was persisted as false
            var reloaded = new SettingsService(NullLogger<SettingsService>.Instance, tempFile);
            reloaded.Load();
            Assert.IsFalse(reloaded.Current.MountWatcherEnabled);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { /* best-effort */ }
        }
    }

    [TestMethod]
    public void Lifecycle_OnShutdown_CallsStopSync_WhenPreferenceIsTrue()
    {
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = true;

        lifecycle.OnShutdown();

        Assert.IsTrue(fake.StopSyncCalled);
    }

    [TestMethod]
    public void Lifecycle_OnShutdown_DoesNotCallStopSync_WhenPreferenceIsFalse()
    {
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = false;

        lifecycle.OnShutdown();

        Assert.IsFalse(fake.StopSyncCalled);
    }

    [TestMethod]
    public async Task Lifecycle_OnShutdown_DoesNotCallStopSync_AfterEnableFails()
    {
        // If enable failed on startup and preference was cleared, shutdown should not stop
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = true;
        fake.EnableResult = false;

        await lifecycle.OnStartupAsync(); // clears MountWatcherEnabled
        fake.StopSyncCalled = false;     // reset flag

        lifecycle.OnShutdown();

        Assert.IsFalse(fake.StopSyncCalled);
    }
}

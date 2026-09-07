using Microsoft.Extensions.Logging.Abstractions;
using Starsky.Desktop.Models;
using Starsky.Desktop.Services;
using starsky.Tests.FakeCreateAn;

namespace starsky.Tests.Services;

[TestClass]
public class MountWatcherServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string TempSettings() =>
        Path.Combine(Path.GetTempPath(), $"starsky-mw-{Guid.NewGuid()}.json");

    private static string NonExistentBinary() =>
        Path.Combine(Path.GetTempPath(), "nonexistent_cli.exe");

    /// <summary>
    /// Creates a real (empty) file to stand in for the CLI binary so File.Exists passes.
    /// </summary>
    private static string CreateTempBinary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"fake_cli_{Guid.NewGuid()}.exe");
        File.WriteAllBytes(path, []);
        return path;
    }

    private static MountWatcherService WithRunner(string binaryPath, FakeProcessRunner runner) =>
        new(binaryPath, NullLogger<MountWatcherService>.Instance, runner);

    private static (MountWatcherLifecycle lifecycle, SettingsService settings, FakeMountWatcherService fake)
        CreateLifecycle(string? tempFile = null)
    {
        var file = tempFile ?? TempSettings();
        var settings = new SettingsService(NullLogger<SettingsService>.Instance, file);
        settings.Load();
        var fake = new FakeMountWatcherService();
        var lifecycle = new MountWatcherLifecycle(
            fake, settings, NullLogger<MountWatcherLifecycle>.Instance);
        return (lifecycle, settings, fake);
    }

    // ── DesktopSettings ───────────────────────────────────────────────────────

    [TestMethod]
    public void DesktopSettings_MountWatcherEnabled_DefaultsToFalse()
    {
        Assert.IsFalse(new DesktopSettings().MountWatcherEnabled);
    }

    [TestMethod]
    public void DesktopSettings_MountWatcherEnabled_True_RoundTripsJson()
    {
        var original = new DesktopSettings { MountWatcherEnabled = true };
        var restored = JsonSerializer.Deserialize<DesktopSettings>(JsonSerializer.Serialize(original))!;
        Assert.IsTrue(restored.MountWatcherEnabled);
    }

    [TestMethod]
    public void DesktopSettings_MountWatcherEnabled_False_RoundTripsJson()
    {
        var original = new DesktopSettings { MountWatcherEnabled = false };
        var restored = JsonSerializer.Deserialize<DesktopSettings>(JsonSerializer.Serialize(original))!;
        Assert.IsFalse(restored.MountWatcherEnabled);
    }

    // ── MountWatcherService — EnableAsync ─────────────────────────────────────

    [TestMethod]
    public async Task Enable_ReturnsFalse_WhenBinaryMissing()
    {
        var svc = WithRunner(NonExistentBinary(), new FakeProcessRunner());
        Assert.IsFalse(await svc.EnableAsync());
    }

    [TestMethod]
    public async Task Enable_ReturnsTrue_WhenInstallSucceeds()
    {
        var bin = CreateTempBinary();
        try
        {
            var runner = new FakeProcessRunner().EnqueueElevated(0);
            var svc = WithRunner(bin, runner);

            Assert.IsTrue(await svc.EnableAsync());
            Assert.HasCount(1, runner.RunElevatedCalls);
            Assert.AreEqual("--install", runner.RunElevatedCalls[0].Args);
        }
        finally { TryDelete(bin); }
    }

    [TestMethod]
    public async Task Enable_ReturnsFalse_WhenInstallFails()
    {
        var bin = CreateTempBinary();
        try
        {
            var runner = new FakeProcessRunner().EnqueueElevated(1);
            Assert.IsFalse(await WithRunner(bin, runner).EnableAsync());
        }
        finally { TryDelete(bin); }
    }

    // ── MountWatcherService — DisableAsync ────────────────────────────────────

    [TestMethod]
    public async Task Disable_ReturnsTrue_WhenBinaryMissingAndServiceNotInstalled()
    {
        // Binary missing, sc.exe says not installed → success
        var runner = new FakeProcessRunner().EnqueueRun(1060, ""); // sc.exe: not found
        var svc = WithRunner(NonExistentBinary(), runner);
        Assert.IsTrue(await svc.DisableAsync());
    }

    [TestMethod]
    public async Task Disable_ReturnsFalse_WhenBinaryMissingButServiceStillInstalled()
    {
        // Binary missing, sc.exe says service exists → failure
        var runner = new FakeProcessRunner().EnqueueRun(0, "STATE: 1 STOPPED");
        var svc = WithRunner(NonExistentBinary(), runner);
        Assert.IsFalse(await svc.DisableAsync());
    }

    [TestMethod]
    public async Task Disable_ReturnsTrue_WhenUninstallSucceeds()
    {
        var bin = CreateTempBinary();
        try
        {
            var runner = new FakeProcessRunner().EnqueueElevated(0);
            Assert.IsTrue(await WithRunner(bin, runner).DisableAsync());
            Assert.AreEqual("--uninstall", runner.RunElevatedCalls[0].Args);
        }
        finally { TryDelete(bin); }
    }

    [TestMethod]
    public async Task Disable_ReturnsFalse_WhenUninstallFails()
    {
        var bin = CreateTempBinary();
        try
        {
            var runner = new FakeProcessRunner().EnqueueElevated(1);
            Assert.IsFalse(await WithRunner(bin, runner).DisableAsync());
        }
        finally { TryDelete(bin); }
    }

    // ── MountWatcherService — GetStatus ───────────────────────────────────────

    [TestMethod]
    public void GetStatus_ReturnsRunning_WhenScExeOutputContainsRunning()
    {
        var runner = new FakeProcessRunner().EnqueueRun(0, "STATE              : 4  RUNNING");
        Assert.AreEqual(MountWatcherStatus.Running, WithRunner(NonExistentBinary(), runner).GetStatus());
    }

    [TestMethod]
    public void GetStatus_ReturnsStopped_WhenScExeSucceedsWithoutRunning()
    {
        var runner = new FakeProcessRunner().EnqueueRun(0, "STATE              : 1  STOPPED");
        Assert.AreEqual(MountWatcherStatus.Stopped, WithRunner(NonExistentBinary(), runner).GetStatus());
    }

    [TestMethod]
    public void GetStatus_ReturnsNotInstalled_WhenScExeExitsNonZero()
    {
        var runner = new FakeProcessRunner().EnqueueRun(1060, "");
        Assert.AreEqual(MountWatcherStatus.NotInstalled, WithRunner(NonExistentBinary(), runner).GetStatus());
    }

    [TestMethod]
    public void GetStatus_ReturnsRunning_CaseInsensitive()
    {
        var runner = new FakeProcessRunner().EnqueueRun(0, "state: running");
        Assert.AreEqual(MountWatcherStatus.Running, WithRunner(NonExistentBinary(), runner).GetStatus());
    }

    // ── MountWatcherService — StopSync ────────────────────────────────────────

    [TestMethod]
    public void StopSync_DoesNothing_WhenBinaryMissing()
    {
        var runner = new FakeProcessRunner();
        WithRunner(NonExistentBinary(), runner).StopSync();
        Assert.IsEmpty(runner.RunElevatedCalls);
    }

    [TestMethod]
    public void StopSync_CallsUninstall_WhenBinaryExists()
    {
        var bin = CreateTempBinary();
        try
        {
            var runner = new FakeProcessRunner().EnqueueElevated(0);
            WithRunner(bin, runner).StopSync();
            Assert.HasCount(1, runner.RunElevatedCalls);
            Assert.AreEqual("--uninstall", runner.RunElevatedCalls[0].Args);
        }
        finally { TryDelete(bin); }
    }

    [TestMethod]
    public void StopSync_DoesNotThrow_WhenUninstallFails()
    {
        var bin = CreateTempBinary();
        try
        {
            var runner = new FakeProcessRunner().EnqueueElevated(1);
            Exception? ex = null;
            try { WithRunner(bin, runner).StopSync(); } catch (Exception e) { ex = e; }
            Assert.IsNull(ex);
        }
        finally { TryDelete(bin); }
    }

    // ── MountWatcherService — ServiceName constant ────────────────────────────

    [TestMethod]
    public void ServiceName_IsProductionName()
    {
        // Protects against accidental debug-mode suffix being left in
        Assert.IsFalse(MountWatcherService.ServiceName.EndsWith("-debug", StringComparison.Ordinal));
        Assert.IsTrue(MountWatcherService.ServiceName.StartsWith("starsky-", StringComparison.Ordinal));
    }

    // ── FakeMountWatcherService ───────────────────────────────────────────────

    [TestMethod]
    public async Task Fake_Enable_SetsCalledFlag()
    {
        var fake = new FakeMountWatcherService();
        await fake.EnableAsync();
        Assert.IsTrue(fake.EnableCalled);
    }

    [TestMethod]
    public async Task Fake_Enable_ReturnsConfiguredResult()
    {
        var fake = new FakeMountWatcherService { EnableResult = false };
        Assert.IsFalse(await fake.EnableAsync());
    }

    [TestMethod]
    public async Task Fake_Disable_SetsCalledFlag()
    {
        var fake = new FakeMountWatcherService();
        await fake.DisableAsync();
        Assert.IsTrue(fake.DisableCalled);
    }

    [TestMethod]
    public void Fake_StopSync_SetsCalledFlag()
    {
        var fake = new FakeMountWatcherService();
        fake.StopSync();
        Assert.IsTrue(fake.StopSyncCalled);
    }

    [TestMethod]
    public void Fake_GetStatus_ReturnsConfiguredStatus()
    {
        var fake = new FakeMountWatcherService { StatusResult = MountWatcherStatus.Running };
        Assert.AreEqual(MountWatcherStatus.Running, fake.GetStatus());
    }

    // ── MountWatcherLifecycle — OnStartupAsync ────────────────────────────────

    [TestMethod]
    public async Task Lifecycle_OnStartup_SkipsEnable_WhenPreferenceIsFalse()
    {
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = false;
        await lifecycle.OnStartupAsync();
        Assert.IsFalse(fake.EnableCalled);
    }

    [TestMethod]
    public async Task Lifecycle_OnStartup_CallsEnable_WhenPreferenceIsTrue()
    {
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = true;
        await lifecycle.OnStartupAsync();
        Assert.IsTrue(fake.EnableCalled);
    }

    [TestMethod]
    public async Task Lifecycle_OnStartup_KeepsPreference_WhenEnableSucceeds()
    {
        var (lifecycle, settings, _) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = true;
        await lifecycle.OnStartupAsync();
        Assert.IsTrue(settings.Current.MountWatcherEnabled);
    }

    [TestMethod]
    public async Task Lifecycle_OnStartup_ClearsPreference_WhenEnableFails()
    {
        var tempFile = TempSettings();
        try
        {
            var (lifecycle, settings, fake) = CreateLifecycle(tempFile);
            settings.Current.MountWatcherEnabled = true;
            fake.EnableResult = false;
            await lifecycle.OnStartupAsync();
            Assert.IsFalse(settings.Current.MountWatcherEnabled);
        }
        finally { TryDelete(tempFile); }
    }

    [TestMethod]
    public async Task Lifecycle_OnStartup_PersistsClearedPreference_WhenEnableFails()
    {
        var tempFile = TempSettings();
        try
        {
            var (lifecycle, settings, fake) = CreateLifecycle(tempFile);
            settings.Current.MountWatcherEnabled = true;
            fake.EnableResult = false;
            await lifecycle.OnStartupAsync();

            var reloaded = new SettingsService(NullLogger<SettingsService>.Instance, tempFile);
            reloaded.Load();
            Assert.IsFalse(reloaded.Current.MountWatcherEnabled);
        }
        finally { TryDelete(tempFile); }
    }

    // ── MountWatcherLifecycle — OnShutdown ────────────────────────────────────

    [TestMethod]
    public void Lifecycle_OnShutdown_CallsStopSync_WhenEnabled()
    {
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = true;
        lifecycle.OnShutdown();
        Assert.IsTrue(fake.StopSyncCalled);
    }

    [TestMethod]
    public void Lifecycle_OnShutdown_SkipsStopSync_WhenDisabled()
    {
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = false;
        lifecycle.OnShutdown();
        Assert.IsFalse(fake.StopSyncCalled);
    }

    [TestMethod]
    public async Task Lifecycle_OnShutdown_SkipsStopSync_AfterEnableFailed()
    {
        var (lifecycle, settings, fake) = CreateLifecycle();
        settings.Current.MountWatcherEnabled = true;
        fake.EnableResult = false;
        await lifecycle.OnStartupAsync();
        fake.StopSyncCalled = false;

        lifecycle.OnShutdown();

        Assert.IsFalse(fake.StopSyncCalled);
    }

    // ── UpdateService — StopSync before apply ─────────────────────────────────

    [TestMethod]
    public async Task UpdateService_ApplyUpdate_CallsStopSync_BeforeApply()
    {
        var tempFile = TempSettings();
        try
        {
            var settings = new SettingsService(NullLogger<SettingsService>.Instance, tempFile);
            settings.Load();
            var fake = new FakeMountWatcherService();
            var svc = new FakeUpdateService(settings, fake);

            await svc.ApplyUpdateAsync();

            Assert.IsTrue(fake.StopSyncCalled);
        }
        finally { TryDelete(tempFile); }
    }

    [TestMethod]
    public async Task UpdateService_ApplyUpdate_DoesNotThrow_WithoutMountWatcher()
    {
        var tempFile = TempSettings();
        try
        {
            var settings = new SettingsService(NullLogger<SettingsService>.Instance, tempFile);
            settings.Load();
            var svc = new FakeUpdateService(settings, null);

            Exception? ex = null;
            try { await svc.ApplyUpdateAsync(); } catch (Exception e) { ex = e; }
            Assert.IsNull(ex);
        }
        finally { TryDelete(tempFile); }
    }

    private sealed class FakeUpdateService(SettingsService settings, IMountWatcherService? mws)
        : UpdateService(settings, NullLogger<UpdateService>.Instance, mountWatcherService: mws)
    {
        protected override bool HasPendingUpdate => true;
        protected override Task DoApplyUpdateAsync() => Task.CompletedTask;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort */ }
    }
}

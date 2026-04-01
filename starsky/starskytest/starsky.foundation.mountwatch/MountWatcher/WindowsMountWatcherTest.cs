using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.MountWatcher;

[TestClass]
public sealed class WindowsMountWatcherTest
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public void WindowsMountWatcher_OnConstruction_IsNotRunning()
	{
		// Arrange & Act
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);

		// Assert
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void WindowsMountWatcher_GetMountedVolumes_ReturnsEnumerable()
	{
		// Arrange
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);

		// Act
		var volumes = watcher.GetMountedVolumes();

		// Assert
		Assert.IsNotNull(volumes);
	}

	[TestMethod]
	public void WindowsMountWatcher_Stop_CanBeCalled()
	{
		// Arrange
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);

		// Act
		watcher.Stop();

		// Assert
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void WindowsMountWatcher_DetectNewMounts_ReturnsOnlyNewDrive()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);
		watcher.SeedKnownMounts(["C:\\"]);

		var newMounts = watcher.DetectNewMounts(["C:\\", "E:\\"]);

		CollectionAssert.AreEqual(new List<string> { "E:\\" }, newMounts);
	}

	[TestMethod]
	public void WindowsMountWatcher_DetectNewMounts_AfterRemoveAndReinsert_DetectsAgain()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);
		watcher.SeedKnownMounts(["C:\\", "E:\\"]);

		var removedSnapshot = watcher.DetectNewMounts(["C:\\"]);
		var reinsertedSnapshot = watcher.DetectNewMounts(["C:\\", "E:\\"]);

		Assert.IsEmpty(removedSnapshot);
		CollectionAssert.AreEqual(new List<string> { "E:\\" }, reinsertedSnapshot);
	}

	[TestMethod]
	public void WindowsMountWatcher_DetectNewMounts_IsCaseInsensitive()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);
		watcher.SeedKnownMounts(["E:\\"]);

		var newMounts = watcher.DetectNewMounts(["e:\\"]);

		Assert.IsEmpty(newMounts);
	}

	[TestMethod]
	public void WindowsMountWatcher_TryTrackEventDrive_NewDrive_TracksAndReturnsTrue()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);
		watcher.SeedKnownMounts(["C:\\"]);

		var tracked = watcher.TryTrackEventDrive("E:", out var normalized);

		Assert.IsTrue(tracked);
		Assert.AreEqual("E:\\", normalized);
	}

	[TestMethod]
	public void WindowsMountWatcher_TryTrackEventDrive_ExistingDrive_ReturnsFalse()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);
		watcher.SeedKnownMounts(["C:\\", "E:\\"]);

		var tracked = watcher.TryTrackEventDrive("E:", out _);

		Assert.IsFalse(tracked);
	}

	[TestMethod]
	public void WindowsMountWatcher_NormalizeDrive_E_ConvertsToRootPath()
	{
		Assert.AreEqual("E:\\", WindowsMountWatcher.NormalizeDrive("E:"));
		Assert.AreEqual("E:\\", WindowsMountWatcher.NormalizeDrive("E:\\"));
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	[OSCondition(ConditionMode.Exclude, OperatingSystems.Windows)]
	public async Task WindowsMountWatcher_Start_DoesNotThrow_WhenPlatformIsUnsupported()
	{
		// Arrange
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);

		// Act: run Start on a worker thread because polling fallback is a blocking loop.
		var startTask = Task.Run(watcher.Start, TestContext.CancellationToken);
		await Task.Delay(100, TestContext.CancellationToken);

		// Assert: Stop should cause Start to return promptly and without exceptions.
		watcher.Stop();
		var completedTask =
			await Task.WhenAny(startTask, Task.Delay(3000, TestContext.CancellationToken));
		Assert.AreSame(startTask, completedTask,
			"Start() did not complete after Stop() was called.");

		await startTask;
	}

	[TestMethod]
	public void NormalizeDrive_AddsBackslash_WhenColonOnly()
	{
		Assert.AreEqual("E:\\", WindowsMountWatcher.NormalizeDrive("E:"));
		Assert.AreEqual("E:\\", WindowsMountWatcher.NormalizeDrive("E:\\"));
	}

	[TestMethod]
	public void DetectNewMounts_FindsNewDrive_AndIsCaseInsensitive()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), () => OSPlatform.Windows, 10);
		watcher.SeedKnownMounts(new List<string> { "C:\\" });

		var newMounts = watcher.DetectNewMounts(new List<string> { "C:\\", "e:\\" });

		CollectionAssert.AreEqual(new List<string> { "e:\\" }, newMounts);
	}

	[TestMethod]
	public void TryTrackEventDrive_AddsNewDrive_ReturnsTrue()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), () => OSPlatform.Windows, 10);
		watcher.SeedKnownMounts(new List<string> { "C:\\" });

		var ok = watcher.TryTrackEventDrive("E:", out var normalized);

		Assert.IsTrue(ok);
		Assert.AreEqual("E:\\", normalized);
	}

	[TestMethod]
	public void HandleVolumeRemoval_RemovesFromBaseline()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), () => OSPlatform.Windows, 10);
		watcher.SeedKnownMounts(new List<string> { "C:\\", "E:\\" });

		watcher.HandleVolumeRemoval("E:");

		var mounts = watcher.DetectNewMounts(new List<string> { "C:\\", "E:\\" });
		// since we removed and then detect with same mounts, E:\ should be reported as new
		CollectionAssert.AreEqual(new List<string> { "E:\\" }, mounts);
	}

	[TestMethod]
	public void Start_OnNonWindows_UsesPollingFallback_AndStopReturns()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(),
			() => OSPlatform.Linux, 50);

		// Start will use polling fallback on non-windows; run in background so test can stop it.
		var t = Task.Run(watcher.Start, TestContext.CancellationToken);

		Thread.Sleep(120);
		watcher.Stop();

		var completed = t.Wait(100, TestContext.CancellationToken);
		Assert.IsTrue(completed, "Start did not return after Stop");
	}
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starskytest.FakeMocks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class WindowsMountWatcherTest
{
	[TestMethod]
	public void WindowsMountWatcher_OnConstruction_IsNotRunning()
	{
		// Arrange & Act
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());

		// Assert
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void WindowsMountWatcher_GetMountedVolumes_ReturnsEnumerable()
	{
		// Arrange
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());

		// Act
		var volumes = watcher.GetMountedVolumes();

		// Assert
		Assert.IsNotNull(volumes);
	}

	[TestMethod]
	public void WindowsMountWatcher_Stop_CanBeCalled()
	{
		// Arrange
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());

		// Act
		watcher.Stop();

		// Assert
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void WindowsMountWatcher_DetectNewMounts_ReturnsOnlyNewDrive()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());
		watcher.SeedKnownMounts(["C:\\"]);

		var newMounts = watcher.DetectNewMounts(["C:\\", "E:\\"]);

		CollectionAssert.AreEqual(new List<string> { "E:\\" }, newMounts);
	}

	[TestMethod]
	public void WindowsMountWatcher_DetectNewMounts_AfterRemoveAndReinsert_DetectsAgain()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());
		watcher.SeedKnownMounts(["C:\\", "E:\\"]);

		var removedSnapshot = watcher.DetectNewMounts(["C:\\"]);
		var reinsertedSnapshot = watcher.DetectNewMounts(["C:\\", "E:\\"]);

		Assert.IsEmpty(removedSnapshot);
		CollectionAssert.AreEqual(new List<string> { "E:\\" }, reinsertedSnapshot);
	}

	[TestMethod]
	public void WindowsMountWatcher_DetectNewMounts_IsCaseInsensitive()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());
		watcher.SeedKnownMounts(["E:\\"]);

		var newMounts = watcher.DetectNewMounts(["e:\\"]);

		Assert.IsEmpty(newMounts);
	}

	[TestMethod]
	public void WindowsMountWatcher_TryTrackEventDrive_NewDrive_TracksAndReturnsTrue()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());
		watcher.SeedKnownMounts(["C:\\"]);

		var tracked = watcher.TryTrackEventDrive("E:", out var normalized);

		Assert.IsTrue(tracked);
		Assert.AreEqual("E:\\", normalized);
	}

	[TestMethod]
	public void WindowsMountWatcher_TryTrackEventDrive_ExistingDrive_ReturnsFalse()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());
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
		var watcher = new WindowsMountWatcher(new FakeIWebLogger());

		// Act: run Start on a worker thread because polling fallback is a blocking loop.
		var startTask = Task.Run(watcher.Start, TestContext.CancellationToken);
		await Task.Delay(100, TestContext.CancellationToken);

		// Assert: Stop should cause Start to return promptly and without exceptions.
		watcher.Stop();
		var completedTask = await Task.WhenAny(startTask, Task.Delay(3000, TestContext.CancellationToken));
		Assert.AreSame(startTask, completedTask, "Start() did not complete after Stop() was called.");

		await startTask;
	}

	public TestContext TestContext { get; set; }
}

using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class WindowsMountWatcherUnitTest
{
	public TestContext TestContext { get; set; }

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

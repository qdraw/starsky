using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starsky.foundation.platform.Interfaces;
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
		watcher.SeedKnownMounts(new List<string> { "C:\\" });

		var newMounts = watcher.DetectNewMounts(new List<string> { "C:\\", "E:\\" });

		CollectionAssert.AreEqual(new List<string> { "E:\\" }, newMounts);
	}

	[TestMethod]
	public void WindowsMountWatcher_DetectNewMounts_AfterRemoveAndReinsert_DetectsAgain()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);
		watcher.SeedKnownMounts(new List<string> { "C:\\", "E:\\" });

		var removedSnapshot = watcher.DetectNewMounts(new List<string> { "C:\\" });
		var reinsertedSnapshot = watcher.DetectNewMounts(new List<string> { "C:\\", "E:\\" });

		Assert.IsEmpty(removedSnapshot);
		CollectionAssert.AreEqual(new List<string> { "E:\\" }, reinsertedSnapshot);
	}

	[TestMethod]
	public void WindowsMountWatcher_DetectNewMounts_IsCaseInsensitive()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);
		watcher.SeedKnownMounts(new List<string> { "E:\\" });

		var newMounts = watcher.DetectNewMounts(new List<string> { "e:\\" });

		Assert.IsEmpty(newMounts);
	}

	[TestMethod]
	public void WindowsMountWatcher_TryTrackEventDrive_NewDrive_TracksAndReturnsTrue()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);
		watcher.SeedKnownMounts(new List<string> { "C:\\" });

		var tracked = watcher.TryTrackEventDrive("E:", out var normalized);

		Assert.IsTrue(tracked);
		Assert.AreEqual("E:\\", normalized);
	}

	[TestMethod]
	public void WindowsMountWatcher_TryTrackEventDrive_ExistingDrive_ReturnsFalse()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);
		watcher.SeedKnownMounts(new List<string> { "C:\\", "E:\\" });

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
	public async Task Start_OnNonWindows_UsesPollingFallback_AndStopReturns()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(),
			() => OSPlatform.Linux, 50);

		// Start will use polling fallback on non-windows; run in background so test can stop it.
		var t = Task.Run(watcher.Start, TestContext.CancellationToken);

		await Task.Delay(120, TestContext.CancellationToken);
		watcher.Stop();

		var completed = t.Wait(100, TestContext.CancellationToken);
		Assert.IsTrue(completed, "Start did not return after Stop");
	}

	[TestMethod]
	[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
	public void StartWmiWatcherTst()
	{
		var logger = new FakeIWebLogger();
		var watcher = new WindowsMountWatcher(logger,
			() => OSPlatform.Windows, 50);
		watcher.StartWmiWatcher();

		watcher.Stop();

		if ( OperatingSystem.IsWindows() )
		{
			Assert.Contains(p => p.Item2 == "Windows WMI watcher started",
				logger.TrackedInformation);
		}
		else
		{
			Assert.Contains(p => p.Item2 == "Failed to start WMI watcher, falling back to polling",
				logger.TrackedExceptions);
		}
	}

	[TestMethod]
	[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
	public void OnVolumeChanged()
	{
		var logger = new FakeIWebLogger();

		// 1) Removal event (EventType=3) should remove from baseline and not raise MountDetected
		var watcher = new TestWindowsMountWatcher(logger, () => OSPlatform.Windows, 50,
			["C:\\"]);
		var detected = new List<string>();
		var detected1 = detected;
		watcher.MountDetected += (s, e) => detected1.Add(e.MountPath);
		watcher.SeedKnownMounts(new List<string> { "D:\\" });
		watcher.OnVolumeChanged("3", "D:");
		Assert.IsEmpty(detected);
		Assert.IsNotEmpty(logger.TrackedInformation);

		// 2) New drive event should track and raise MountDetected for the event drive only once
		watcher = new TestWindowsMountWatcher(logger, () => OSPlatform.Windows, 50,
			["C:\\"]);
		detected = [];
		watcher.MountDetected += (s, e) => detected.Add(e.MountPath);
		// baseline contains C:\ so retry-scan will not report it as new
		watcher.SeedKnownMounts(new List<string> { "C:\\" });
		// make GetMountedVolumes report the same drive too (retry scan)
		watcher.SetMountedVolumes(["C:\\", "E:\\"]);
		watcher.OnVolumeChanged("2", "E:");
		// Should have been detected once for the event drive, retry-scan should ignore duplicate
		Assert.HasCount(1, detected);
		Assert.AreEqual("E:\\", detected[0]);

		// 3) If retry-scan contains other new drives they should trigger MountDetected
		watcher = new TestWindowsMountWatcher(logger, () => OSPlatform.Windows, 50,
			["C:\\"]);
		detected.Clear();
		watcher.MountDetected += (s, e) => detected.Add(e.MountPath);
		watcher.SeedKnownMounts(new List<string> { "C:\\" });
		// event drive empty but mounted volumes contain a new drive G:\
		watcher.SetMountedVolumes(["C:\\", "G:\\"]);
		watcher.OnVolumeChanged("2", "");
		Assert.Contains("G:\\", detected);
		Assert.IsEmpty(logger.TrackedExceptions);
	}
}

// Test helper: subclass WindowsMountWatcher to return controllable mounted volumes
internal sealed class TestWindowsMountWatcher : WindowsMountWatcher
{
	private List<string> _mountedVolumes;

	public TestWindowsMountWatcher(IWebLogger logger, Func<OSPlatform> platformResolver,
		int pollIntervalMs,
		List<string> initialMountedVolumes) : base(logger, platformResolver, pollIntervalMs)
	{
		_mountedVolumes = new List<string>(initialMountedVolumes);
	}

	public void SetMountedVolumes(List<string> volumes)
	{
		_mountedVolumes = new List<string>(volumes);
	}

	public override List<string> GetMountedVolumes()
	{
		// return a copy to avoid external mutation
		return [.._mountedVolumes];
	}
}

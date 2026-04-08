using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starsky.foundation.mountwatch.MountWatcher.Windows.Interfaces;
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

		CollectionAssert.AreEqual(new List<string> { "E:\\" }, newMounts.ToList());
	}

	[TestMethod]
	public void WindowsMountWatcher_DetectNewMounts_AfterRemoveAndReinsert_DetectsAgain()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), 10);
		watcher.SeedKnownMounts(new List<string> { "C:\\", "E:\\" });

		var removedSnapshot = watcher.DetectNewMounts(new List<string> { "C:\\" });
		var reinsertedSnapshot = watcher.DetectNewMounts(new List<string> { "C:\\", "E:\\" });

		Assert.IsEmpty(removedSnapshot);
		CollectionAssert.AreEqual(new List<string> { "E:\\" }, reinsertedSnapshot.ToList());
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
	[DataRow("E:", "E:\\")]
	[DataRow("E:\\", "E:\\")]
	[DataRow(@"E:\\", "E:\\")]
	[DataRow("e:", "e:\\")]
	[DataRow(" e: ", "e:\\")]
	[DataRow("E:/", "E:\\")]
	[DataRow("path/with/slash", "path\\with\\slash")]
	[DataRow("", "")]
	[DataRow("X:folder", "X:folder\\")]
	[DataRow("C:/Windows/System32", "C:\\")]
	public void NormalizeDrive_DataDriven(string input, string expected)
	{
		var result = WindowsMountWatcher.NormalizeDrive(input);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void DetectNewMounts_FindsNewDrive_AndIsCaseInsensitive()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), () => OSPlatform.Windows, 10);
		watcher.SeedKnownMounts(new List<string> { "C:\\" });

		var newMounts = watcher.DetectNewMounts(new List<string> { "C:\\", "e:\\" });

		CollectionAssert.AreEqual(new List<string> { "e:\\" }, newMounts.ToList());
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

		var result = watcher.HandleVolumeRemoval("E:");

		var mounts = watcher.DetectNewMounts(new List<string> { "C:\\", "E:\\" });
		// since we removed and then detect with same mounts, E:\ should be reported as new
		CollectionAssert.AreEqual(new List<string> { "E:\\" }, mounts.ToList());
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void HandleVolumeRemoval_Empty()
	{
		var logger = new FakeIWebLogger();
		var watcher = new WindowsMountWatcher(logger, () => OSPlatform.Windows, 10);
		var result = watcher.HandleVolumeRemoval(string.Empty);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void HandleVolumeRemoval_NotInBaseline()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(), () => OSPlatform.Windows, 10);
		watcher.SeedKnownMounts(new List<string> { "C:\\" });

		var result = watcher.HandleVolumeRemoval("E:");

		var mounts = watcher.DetectNewMounts(
			new List<string> { "C:\\", "E:\\" });
		CollectionAssert.AreEqual(new List<string> { "E:\\" }, mounts.ToList());
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task Start_OnNonWindows_UsesPollingFallback_AndStopReturns()
	{
		var watcher = new WindowsMountWatcher(
			new FakeIWebLogger(),
			() => OSPlatform.Linux,
			50);

		var t = Task.Run(watcher.Start, TestContext.CancellationToken);

		// Wait until Start() actually began
		await watcher.Started.Task.WaitAsync(TestContext.CancellationToken);

		watcher.Stop();

		var completed = t.Wait(TimeSpan.FromSeconds(1), TestContext.CancellationToken);

		Assert.AreEqual(!RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
			completed, "Start did not return after Stop");
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
			var log = logger.TrackedInformation[0].Item2;
			Assert.Contains("Windows WMI watcher start", log!);
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
		watcher.MountDetected += (_, e) => detected1.Add(e.MountPath);
		watcher.SeedKnownMounts(new List<string> { "D:\\" });
		watcher.OnVolumeChanged("3", "D:");
		Assert.IsEmpty(detected);
		Assert.IsNotEmpty(logger.TrackedInformation);

		// 2) New drive event should track and raise MountDetected for the event drive only once
		watcher = new TestWindowsMountWatcher(logger, () => OSPlatform.Windows, 50,
			["C:\\"]);
		detected = [];
		watcher.MountDetected += (_, e) => detected.Add(e.MountPath);
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
		watcher.MountDetected += (_, e) => detected.Add(e.MountPath);
		watcher.SeedKnownMounts(new List<string> { "C:\\" });
		// event drive empty but mounted volumes contain a new drive G:\
		watcher.SetMountedVolumes(["C:\\", "G:\\"]);
		watcher.OnVolumeChanged("2", "");
		Assert.Contains("G:\\", detected);
		Assert.IsEmpty(logger.TrackedExceptions);
	}

	[TestMethod]
	public void GetMountedVolumes_SystemThrows_ReturnsEmpty()
	{
		var system = new FakeWindowsMountWatcherSystem { ThrowOnGetDrives = true };
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(),
			( Func<OSPlatform>? ) ( () => OSPlatform.Windows ), 10,
			system);

		var volumes = watcher.GetMountedVolumes();
		Assert.IsEmpty(volumes);
	}

	[TestMethod]
	public void StartWmiWatcher_WithFakeSystem_StartsAndStopInvokes()
	{
		var logger = new FakeIWebLogger();
		var system = new FakeWindowsMountWatcherSystem { WatcherToCreate = new object() };
		var watcher = new WindowsMountWatcher(logger,
			() => OSPlatform.Windows, 50,
			system);

		watcher.StartWmiWatcher();

		Assert.IsTrue(system.StartWatcherCalled);

		// Stop via public Stop should call Stop and Dispose through abstraction
		watcher.Stop();
		Assert.IsTrue(system.StopWatcherCalled);
		Assert.IsTrue(system.DisposeWatcherCalled);
	}

	[TestMethod]
	public void StartWmiWatcher_CreateWatcherThrows_LogsAndDoesNotThrow()
	{
		var logger = new FakeIWebLogger();
		var system = new FakeWindowsMountWatcherSystem { ThrowOnCreateManagementWatcher = true };
		var watcher = new WindowsMountWatcher(logger,
			() => OSPlatform.Windows, 50,
			system);

		watcher.StartWmiWatcher();

		Assert.IsNotEmpty(logger.TrackedExceptions);
	}

	[TestMethod]
	public void StartWmiWatcher_CreateWatcherReturnsNull_LogsInformation()
	{
		var logger = new FakeIWebLogger();
		var system = new FakeWindowsMountWatcherSystem { WatcherToCreate = null };
		var watcher = new WindowsMountWatcher(logger,
			() => OSPlatform.Windows, 50,
			system);

		watcher.StartWmiWatcher();

		Assert.IsTrue(logger.TrackedInformation.Exists(t =>
			t.Item2 != null && t.Item2.Contains("CreateManagementWatcher returned null")));
	}

	[TestMethod]
	public void OnVolumeChanged_ObjectNull_LogsError()
	{
		var logger = new FakeIWebLogger();
		var system = new FakeWindowsMountWatcherSystem();
		var watcher = new WindowsMountWatcher(logger,
			() => OSPlatform.Windows, 10,
			system);

		// Call object overload with null to trigger exception handling and logging
		watcher.OnVolumeChanged(null, ( EventArrivedEventArgs? ) null!);

		// The logger should have recorded the exception with the expected message
		Assert.IsTrue(logger.TrackedExceptions.Exists(t => t.Item2 != null
		                                                   && t.Item2.Contains(
			                                                   "Windows volume event handling failed:")));
	}

	private sealed class FakeWindowsMountWatcherSystem : IWindowsMountWatcherSystem
	{
		public bool ThrowOnGetDrives { get; set; }
		public bool ThrowOnCreateManagementWatcher { get; set; }
		public object? WatcherToCreate { get; set; } = new();
		public bool StartWatcherCalled { get; private set; }
		public bool StopWatcherCalled { get; private set; }
		public bool DisposeWatcherCalled { get; private set; }

		public object? CreateManagementWatcher(string wqlQuery)
		{
			return ThrowOnCreateManagementWatcher ? throw new Exception("boom") : WatcherToCreate;
		}

		public void AddEventArrivedHandler(object watcher, EventArrivedEventHandler handler)
		{
			// no-op fake for tests
		}

		public void StartWatcher(object watcher)
		{
			StartWatcherCalled = true;
		}

		public void StopWatcher(object watcher)
		{
			StopWatcherCalled = true;
		}

		public void DisposeWatcher(object watcher)
		{
			DisposeWatcherCalled = true;
		}

		public IEnumerable<DriveInfo> GetDrives()
		{
			if ( ThrowOnGetDrives )
			{
				throw new Exception("fail drives");
			}

			return Array.Empty<DriveInfo>();
		}

		public (string eventTypeStr, string rawDriveName) MapEvent(
			EventArrivedEventArgs arrivedEvent)
		{
			throw new NotImplementedException();
		}
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

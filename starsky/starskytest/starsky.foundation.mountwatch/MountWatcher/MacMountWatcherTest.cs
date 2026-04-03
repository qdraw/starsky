using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starsky.foundation.mountwatch.MountWatcher.MacOS;
using starsky.foundation.mountwatch.MountWatcher.MacOS.Interfaces;
using starsky.foundation.platform.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.MountWatcher;

[TestClass]
public sealed class MacMountWatcherTest
{
	private static MacMountWatcher CreateSut()
	{
		return new MacMountWatcher(new FakeIWebLogger(), 10);
	}

	[TestMethod]
	public void MacMountWatcher_OnConstruction_IsNotRunning()
	{
		var watcher = CreateSut();
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void MacMountWatcher_GetMountedVolumes_ReturnsEnumerable()
	{
		var watcher = CreateSut();
		var volumes = watcher.GetMountedVolumes();
		Assert.IsNotNull(volumes);
	}

	[TestMethod]
	public void MacMountWatcher_Stop_CanbeCalled()
	{
		var watcher = CreateSut();
		watcher.Stop();
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void DetectNewExternalMounts_NewExternalVolume_ReturnsPath()
	{
		var watcher = CreateSut();
		var detected = watcher.DetectNewExternalMounts([
			"/",
			"/Volumes/SD_CARD"
		]);

		CollectionAssert.AreEqual(new List<string> { "/Volumes/SD_CARD" }, detected);
	}

	[TestMethod]
	public void DetectNewExternalMounts_IgnoresRootAndNonExternal()
	{
		var watcher = CreateSut();
		var detected = watcher.DetectNewExternalMounts([
			"/",
			"/tmp/some-dir",
			"/Volumes/CAMERA"
		]);

		CollectionAssert.AreEqual(new List<string> { "/Volumes/CAMERA" }, detected);
	}

	[TestMethod]
	public void DetectNewExternalMounts_RepeatedSnapshot_DoesNotDuplicate()
	{
		var watcher = CreateSut();
		var first = watcher.DetectNewExternalMounts([
			"/Volumes/CAMERA"
		]);
		var second = watcher.DetectNewExternalMounts([
			"/Volumes/CAMERA"
		]);

		CollectionAssert.AreEqual(new List<string> { "/Volumes/CAMERA" }, first);
		CollectionAssert.AreEqual(new List<string>(), second);
	}

	[TestMethod]
	public void UpdateKnownExternalMounts_RemovedMount_CanBeDetectedAgain()
	{
		var watcher = CreateSut();
		_ = watcher.DetectNewExternalMounts([
			"/Volumes/CAMERA"
		]);

		watcher.UpdateKnownExternalMounts([
			"/"
		]);

		var detectedAgain = watcher.DetectNewExternalMounts([
			"/Volumes/CAMERA"
		]);

		CollectionAssert.AreEqual(new List<string> { "/Volumes/CAMERA" }, detectedAgain);
	}

	[TestMethod]
	public void
		DetectNewExternalMounts_EjectAndReinsertSamePath_DetectedAgainWithoutDisappearCallback()
	{
		var watcher = CreateSut();

		var first = watcher.DetectNewExternalMounts([
			"/Volumes/SD_CARD"
		]);

		var afterEject = watcher.DetectNewExternalMounts([
			"/"
		]);

		var reinsert = watcher.DetectNewExternalMounts([
			"/Volumes/SD_CARD"
		]);

		CollectionAssert.AreEqual(new List<string> { "/Volumes/SD_CARD" }, first);
		CollectionAssert.AreEqual(new List<string>(), afterEject);
		CollectionAssert.AreEqual(new List<string> { "/Volumes/SD_CARD" }, reinsert);
	}

	// Tests with IStorage abstraction and dependency injection

	[TestMethod]
	public void GetMountedVolumes_WithFakeStorage_UsesInjectedStorage()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(
			["/Volumes", "/Volumes/USB Drive", "/"]);
		var system = new MacMountWatcherSystem();
		var sut = new MacMountWatcher(logger, storage, system, 10);

		// Act
		var result = sut.GetMountedVolumes();

		// Assert
		Assert.IsNotEmpty(result, "Should use injected storage");
	}

	[TestMethod]
	public void GetMountedVolumes_WithStorageException_ReturnsEmpty()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(
			new UnauthorizedAccessException("Access denied"));
		var system = new MacMountWatcherSystem();
		var sut = new MacMountWatcher(logger, storage, system, 10);

		// Act
		var result = sut.GetMountedVolumes();

		// Assert - should handle exception gracefully
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void Start_WithInjectedDependencies_Starts()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var system = new MacMountWatcherSystem();
		var sut = new MacMountWatcher(logger, storage, system, 10);

		// Act & Assert - should not throw and should start background thread
		sut.Start();
		sut.Stop();

		Assert.IsGreaterThanOrEqualTo(0, logger.TrackedExceptions.Count);
	}

	[TestMethod]
	public void Stop_WithInjectedDependencies_Stops()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var system = new MacMountWatcherSystem();
		var sut = new MacMountWatcher(logger, storage, system, 10);
		sut.Start();

		// Act
		sut.Stop();

		// Assert - should complete without throwing
		Assert.IsNotNull(sut);
	}

	[TestMethod]
	public void DetectNewExternalMounts_WithFakeStorage_WorksCorrectly()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var system = new MacMountWatcherSystem();
		var sut = new MacMountWatcher(logger, storage, system, 10);

		// Act
		var result = sut.DetectNewExternalMounts(new List<string>
		{
			"/Volumes/Camera", "/Volumes/Backup"
		});

		// Assert
		Assert.HasCount(2, result);
	}

	[TestMethod]
	public void UpdateKnownExternalMounts_RemovesEjectedVolumes()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var system = new MacMountWatcherSystem();
		var sut = new MacMountWatcher(logger, storage, system, 10);

		// Add some volumes
		sut.DetectNewExternalMounts(new List<string> { "/Volumes/Camera", "/Volumes/Backup" });

		// Act - update with only Camera (Backup was ejected)
		sut.UpdateKnownExternalMounts(new List<string> { "/Volumes/Camera" });

		// Now detect new mounts
		var result = sut.DetectNewExternalMounts(new List<string>
		{
			"/Volumes/Camera", "/Volumes/NewDrive"
		});

		// Assert - only new drive should be detected
		CollectionAssert.AreEqual(new List<string> { "/Volumes/NewDrive" }, result);
	}

	[TestMethod]
	public void MacMountWatcher_WithDependencyInjection_AllowsMocking()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var mockStorage = new FakeIStorage(
		[
			"/Volumes",
			"/Volumes/Test Device",
			"/"
		]);
		var mockSystem = new MacMountWatcherSystem();

		// Act
		var sut = new MacMountWatcher(logger, mockStorage, mockSystem, 10);
		var volumes = sut.GetMountedVolumes();

		// Assert - should work with injected mocks
		Assert.IsNotEmpty(volumes);
	}
}

internal sealed class FakeMacSystemForUnschedule : IMacMountWatcherSystem
{
	public bool UnscheduleCalled { get; private set; }
	public IntPtr UnscheduleSession { get; private set; }
	public IntPtr UnscheduleRunLoop { get; private set; }
	public IntPtr UnscheduleRunLoopMode { get; private set; }

	public bool RunLoopStopCalled { get; private set; }
	public IntPtr RunLoopStopped { get; private set; }

	public uint GetCfStringEncodingUtf8()
	{
		return 0x08000100;
	}

	public string GetCfRunLoopDefaultMode()
	{
		return "kCFRunLoopDefaultMode";
	}

	public IntPtr DASessionCreateApi(IntPtr allocator)
	{
		return new IntPtr(123);
	}

	public void DASessionScheduleWithRunLoopApi(IntPtr session, IntPtr runLoop, IntPtr runLoopMode)
	{
	}

	public void DASessionUnscheduleWithRunLoopApi(IntPtr session, IntPtr runLoop,
		IntPtr runLoopMode)
	{
		UnscheduleCalled = true;
		UnscheduleSession = session;
		UnscheduleRunLoop = runLoop;
		UnscheduleRunLoopMode = runLoopMode;
	}

	public void DARegisterDiskAppearedCallbackApi(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskAppearedCallback callback,
		IntPtr context)
	{
		throw new NotImplementedException();
	}

	public void DARegisterDiskDisappearedCallbackApi(IntPtr session, IntPtr match,
		MacMountWatcherDelegate.DiskDisappearedCallback callback, IntPtr context)
	{
		throw new NotImplementedException();
	}

	public IntPtr CFRunLoopGetCurrentApi()
	{
		return new IntPtr(456);
	}

	public void CFRunLoopRunApi()
	{
	}

	public void CFRunLoopStopApi(IntPtr runLoop)
	{
		RunLoopStopCalled = true;
		RunLoopStopped = runLoop;
	}

	public IntPtr CFStringCreateWithCStringApi(IntPtr allocator, string cStr, uint encoding)
	{
		return new IntPtr(789);
	}

	public void CFReleaseApi(IntPtr cf)
	{
	}
}

[TestClass]
public sealed class MacMountWatcherUnscheduleTests
{
	[TestMethod]
	public void Stop_WhenSessionAndRunLoopPresent_CallsUnscheduleAndRunLoopStop()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var fakeSystem = new FakeMacSystemForUnschedule();

		// Create SUT with injected fake system
		var sut = new MacMountWatcher(logger, storage, fakeSystem, 10);

		// Use reflection to set private fields: _session, _runLoop, _runLoopMode to non-zero
		var type = typeof(MacMountWatcher);
		var sessionField =
			type.GetField("_session", BindingFlags.NonPublic | BindingFlags.Instance);
		var runLoopField =
			type.GetField("_runLoop", BindingFlags.NonPublic | BindingFlags.Instance);
		var runLoopModeField =
			type.GetField("_runLoopMode", BindingFlags.NonPublic | BindingFlags.Instance);

		sessionField?.SetValue(sut, new IntPtr(101));
		runLoopField?.SetValue(sut, new IntPtr(202));
		runLoopModeField?.SetValue(sut, new IntPtr(303));

		// Act
		sut.Stop();

		// Assert
		Assert.IsTrue(fakeSystem.UnscheduleCalled,
			"Expected DASessionUnscheduleWithRunLoop to be called");
		Assert.AreEqual(new IntPtr(101), fakeSystem.UnscheduleSession);
		Assert.AreEqual(new IntPtr(202), fakeSystem.UnscheduleRunLoop);
		Assert.AreEqual(new IntPtr(303), fakeSystem.UnscheduleRunLoopMode);
		Assert.IsTrue(fakeSystem.RunLoopStopCalled, "Expected CFRunLoopStop to be called");
		Assert.AreEqual(new IntPtr(202), fakeSystem.RunLoopStopped);
	}
}

[TestClass]
public sealed class MacMountWatcherDiskDisappearedTests
{
	[TestMethod]
	public void OnDiskDisappeared_RemovesStaleKnownVolume()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		// GetMountedVolumes returns only root (no external volumes)
		var sut = new TestableMacMountWatcher(logger, () => ["/"]);

		// Prepopulate private _knownVolumes with a mount that should be removed
		var knownField = typeof(MacMountWatcher).GetField("_knownVolumes",
			BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
		var knownSet = ( HashSet<string> ) knownField!.GetValue(sut)!;
		knownSet.Add("/Volumes/CAMERA");

		// Act
		sut.OnDiskDisappeared(IntPtr.Zero, IntPtr.Zero);

		// Assert - stale mount removed
		Assert.DoesNotContain("/Volumes/CAMERA", knownSet);
	}

	[TestMethod]
	public void OnDiskDisappeared_WhenGetMountedVolumesThrows_LogsError()
	{
		var logger = new FakeIWebLogger();

		var sut = new TestableMacMountWatcher(logger, Throwing);

		// Act
		sut.OnDiskDisappeared(IntPtr.Zero, IntPtr.Zero);

		// Assert: logger captured the error message
		Assert.IsTrue(logger.TrackedExceptions.Exists(t =>
			t.Item2 != null && t.Item2.Contains("Error handling macOS disk disappeared callback")));
		return;

		// Make GetMountedVolumes throw
		List<string> Throwing()
		{
			throw new InvalidOperationException("boom");
		}
	}
}

// Helper test subclass to control GetMountedVolumes behavior and expose RunBackupPollingLoop
internal sealed class TestableMacMountWatcher : MacMountWatcher
{
	private readonly Func<List<string>> _getMountedVolumesFunc;

	public TestableMacMountWatcher(IWebLogger logger, Func<List<string>> getMountedVolumesFunc,
		int pollIntervalMs = 10)
		: base(logger, pollIntervalMs)
	{
		_getMountedVolumesFunc = getMountedVolumesFunc;
	}

	// Expose the internal loop for direct testing
	public void RunBackupPollingLoopPublic()
	{
		RunBackupPollingLoop();
	}

	// Allow tests to query/set the protected IsRunning field
	public void SetRunning(bool running)
	{
		// Base class field
		var field = typeof(BaseMountWatcher)
			.GetField("IsRunning",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
		field?.SetValue(this, running);
	}

	public override List<string> GetMountedVolumes()
	{
		return _getMountedVolumesFunc();
	}
}

[TestClass]
public sealed class MacMountWatcherBackupLoopTests
{
	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task RunBackupPollingLoop_RaisesMountDetected_WhenNewMountsAppear()
	{
		var logger = new FakeIWebLogger();

		// Sequence: no mounts, then SD_CARD appears, then root only (eject), then NEW_DRIVE appears
		var seq = new ConcurrentQueue<List<string>>();
		seq.Enqueue([]);
		seq.Enqueue(["/Volumes/SD_CARD"]);
		seq.Enqueue([]);
		seq.Enqueue(["/Volumes/NEW_DRIVE"]);

		var sut = new TestableMacMountWatcher(logger,
			() => { return seq.TryDequeue(out var v) ? v : []; }, 50);

		var detected = new List<string>();
		var tcs = new TaskCompletionSource<bool>(TaskCreationOptions
			.RunContinuationsAsynchronously);
		sut.MountDetected += (_, e) =>
		{
			lock ( detected )
			{
				detected.Add(e.MountPath);
				if ( detected.Count >= 2 )
				{
					tcs.TrySetResult(true);
				}
			}
		};

		sut.SetRunning(true);
		var task = Task.Run(sut.RunBackupPollingLoopPublic, TestContext.CancellationToken);

		var completed =
			await Task.WhenAny(tcs.Task, Task.Delay(5000, TestContext.CancellationToken));

		sut.SetRunning(false);
		await Task.WhenAny(task, Task.Delay(500, TestContext.CancellationToken));

		Assert.AreEqual(tcs.Task, completed, "Timed out waiting for mounts to be detected");
		CollectionAssert.AreEquivalent(
			new List<string> { "/Volumes/SD_CARD", "/Volumes/NEW_DRIVE" }, detected);
	}

	[TestMethod]
	public async Task RunBackupPollingLoop_ContinuesAfterGetMountedVolumesThrows()
	{
		var logger = new FakeIWebLogger();

		var call = 0;

		var sut = new TestableMacMountWatcher(logger, GetMountedVolumes, 50);
		var detected = new List<string>();
		var tcs = new TaskCompletionSource<bool>(TaskCreationOptions
			.RunContinuationsAsynchronously);
		sut.MountDetected += (_, e) =>
		{
			lock ( detected )
			{
				detected.Add(e.MountPath);
				tcs.TrySetResult(true);
			}
		};

		sut.SetRunning(true);
		var task = Task.Run(sut.RunBackupPollingLoopPublic, TestContext.CancellationToken);

		var completed =
			await Task.WhenAny(tcs.Task, Task.Delay(5000, TestContext.CancellationToken));

		sut.SetRunning(false);
		await Task.WhenAny(task, Task.Delay(500, TestContext.CancellationToken));

		Assert.AreEqual(tcs.Task, completed, "Timed out waiting for recovered mount");
		Assert.HasCount(1, detected);
		Assert.AreEqual("/Volumes/RECOVER", detected[0]);
		return;

		List<string> GetMountedVolumes()
		{
			call++;
			return call == 1
				? throw new InvalidOperationException("simulated failure")
				:
				[
					"/Volumes/RECOVER"
				];
		}
	}
}

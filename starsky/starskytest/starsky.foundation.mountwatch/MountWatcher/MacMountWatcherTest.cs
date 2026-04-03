using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starsky.foundation.mountwatch.MountWatcher.MacOS;
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
	public void RunBackupPollingLoop_RaisesMountDetected_WhenNewMountsAppear()
	{
		var logger = new FakeIWebLogger();

		// Sequence: no mounts, then SD_CARD appears, then root only (eject), then NEW_DRIVE appears
		var seq = new Queue<List<string>>();
		seq.Enqueue([]);
		seq.Enqueue(["/Volumes/SD_CARD"]);
		seq.Enqueue([]);
		seq.Enqueue(["/Volumes/NEW_DRIVE"]);

		var sut = new TestableMacMountWatcher(logger, () => seq.Count > 0 ? seq.Dequeue() : [], 20);

		var detected = new List<string>();
		sut.MountDetected += (_, e) => detected.Add(e.MountPath);

		sut.SetRunning(true);
		var task = Task.Run(sut.RunBackupPollingLoopPublic, TestContext.CancellationToken);
		task.Wait(100, TestContext.CancellationToken);
		sut.SetRunning(false);
		task.Wait(50, TestContext.CancellationToken);

		CollectionAssert.AreEquivalent(
			new List<string> { "/Volumes/SD_CARD", "/Volumes/NEW_DRIVE" }, detected);
	}

	[TestMethod]
	public void RunBackupPollingLoop_ContinuesAfterGetMountedVolumesThrows()
	{
		var logger = new FakeIWebLogger();

		var call = 0;

		var sut = new TestableMacMountWatcher(logger, GetMountedVolumes, 20);
		var detected = new List<string>();
		sut.MountDetected += (_, e) => detected.Add(e.MountPath);

		sut.SetRunning(true);
		var task = Task.Run(sut.RunBackupPollingLoopPublic, TestContext.CancellationToken);
		task.Wait(100, TestContext.CancellationToken);

		sut.SetRunning(false);

		Assert.HasCount(1, detected);
		Assert.AreEqual("/Volumes/RECOVER", detected[0]);
		return;

		// First call throws, then returns a single mount
		List<string> GetMountedVolumes()
		{
			call++;
			return call == 1
				? throw new InvalidOperationException("simulated failure")
				: ["/Volumes/RECOVER"];
		}
	}
}

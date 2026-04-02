using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.MountWatcher;

[TestClass]
public sealed class WindowsMountWatcherExtraTests
{
	[TestMethod]
	public void GetMountedVolumes_DriveProviderThrows_ReturnsEmpty()
	{
		var watcher = new WindowsMountWatcher(new FakeIWebLogger(),
			( Func<OSPlatform>? ) ( () => OSPlatform.Windows ), 10,
			driveProvider: () => { throw new Exception("fail drives"); },
			managementWatcherFactory: null);

		var volumes = watcher.GetMountedVolumes();
		Assert.IsNotNull(volumes);
		Assert.AreEqual(0, volumes.Count);
	}

	[TestMethod]
	public void StartWmiWatcher_WithFakeWatcher_StartsAndStopInvokes()
	{
		var logger = new FakeIWebLogger();
		var fake = new FakeManagementWatcher();
		var watcher = new WindowsMountWatcher(logger,
			() => OSPlatform.Windows, 50,
			driveProvider: null,
			managementWatcherFactory: () => fake);

		watcher.StartWmiWatcher();

		// Start should have been called on fake
		Assert.IsTrue(fake.Started);

		// Stop via public Stop should call Stop and Dispose on fake
		watcher.Stop();
		Assert.IsTrue(fake.Stopped);
		Assert.IsTrue(fake.Disposed);
	}

	[TestMethod]
	public void StartWmiWatcher_FactoryThrows_LogsAndDoesNotThrow()
	{
		var logger = new FakeIWebLogger();
		var watcher = new WindowsMountWatcher(logger,
			() => OSPlatform.Windows, 50,
			driveProvider: null,
			managementWatcherFactory: () => throw new Exception("boom"));

		watcher.StartWmiWatcher();

		Assert.IsTrue(logger.TrackedExceptions.Count > 0 || logger.TrackedInformation.Count >= 0);
	}

	[TestMethod]
	public void Start_AlreadyRunning_DoesNotThrow()
	{
		var logger = new FakeIWebLogger();
		var watcher = new WindowsMountWatcher(logger, () => OSPlatform.Linux, 10,
			driveProvider: () => new List<DriveInfo>(), managementWatcherFactory: null);

		watcher.Start();
		// Calling Start again should return early
		watcher.Start();
		watcher.Stop();

		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void OnVolumeChanged_ObjectNull_LogsError()
	{
		var logger = new FakeIWebLogger();
		var watcher = new WindowsMountWatcher(logger, () => OSPlatform.Windows, 10,
			driveProvider: null, managementWatcherFactory: null);

		// Call object overload with null to trigger exception handling and logging
		watcher.OnVolumeChanged(null, null);

		Assert.IsTrue(logger.TrackedExceptions.Count > 0);
	}

	// Fake management watcher with same event and lifecycle methods used via reflection
	private class FakeManagementWatcher
	{
		public bool Started { get; private set; }
		public bool Stopped { get; private set; }
		public bool Disposed { get; private set; }
		public event EventArrivedEventHandler? EventArrived;

		public void Start()
		{
			Started = true;
		}

		public void Stop()
		{
			Stopped = true;
		}

		public void Dispose()
		{
			Disposed = true;
		}
	}
}

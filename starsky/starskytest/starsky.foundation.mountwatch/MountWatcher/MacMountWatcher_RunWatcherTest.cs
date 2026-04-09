using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starsky.foundation.mountwatch.MountWatcher.MacOS;
using starsky.foundation.mountwatch.MountWatcher.MacOS.Interfaces;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.MountWatcher;

[TestClass]
public sealed class MacMountWatcher_RunWatcherTest
{
	[TestMethod]
	public void RunWatcher_SessionUnavailable_LogsError_AndSkipsCleanup()
	{
		var logger = new FakeIWebLogger();
		var fakeSystem = new FakeMacSystem { SessionToReturn = IntPtr.Zero };
		var sut = new MacMountWatcher(logger,
			new StorageHostFullPathFilesystem(logger), fakeSystem, 10);

		sut.RunWatcher();

		// Expect error logged about session unavailable
		Assert.IsTrue(logger.TrackedExceptions.Exists(t =>
			t.Item2 != null && t.Item2.Contains("DiskArbitration session unavailable")));

		// No releases or unschedule should be called when session was never created
		Assert.IsFalse(fakeSystem.DASessionUnscheduleCalled);
		Assert.IsEmpty(fakeSystem.CfReleased);
	}

	[TestMethod]
	public void RunWatcher_RunLoopModeUnavailable_LogsError_ReleasesSession()
	{
		var logger = new FakeIWebLogger();
		var fakeSystem = new FakeMacSystem
		{
			SessionToReturn = new IntPtr(1),
			RunLoopToReturn = new IntPtr(2),
			RunLoopModeToReturn = IntPtr.Zero
		};

		var sut = new MacMountWatcher(logger,
			new StorageHostFullPathFilesystem(logger), fakeSystem, 10);

		sut.RunWatcher();

		Assert.IsTrue(logger.TrackedExceptions.Exists(t =>
			t.Item2 != null && t.Item2.Contains("Unable to create CFRunLoop default mode")));

		// Session was created and should be released in finally
		Assert.Contains(new IntPtr(1), fakeSystem.CfReleased);
	}

	[TestMethod]
	public void RunWatcher_NormalPath_RegistersCallbacks_UnschedulesAndReleases()
	{
		var logger = new FakeIWebLogger();
		var fakeSystem = new FakeMacSystem
		{
			SessionToReturn = new IntPtr(10),
			RunLoopToReturn = new IntPtr(20),
			RunLoopModeToReturn = new IntPtr(30),
			CfRunLoopRunShouldThrow = false
		};

		var sut = new MacMountWatcher(logger,
			new StorageHostFullPathFilesystem(logger), fakeSystem, 10);

		sut.RunWatcher();

		Assert.IsTrue(fakeSystem.DARegisterDiskAppearedCalled);
		Assert.IsTrue(fakeSystem.DARegisterDiskDisappearedCalled);
		Assert.IsTrue(fakeSystem.CFRunLoopRunCalled);
		Assert.IsTrue(fakeSystem.DASessionUnscheduleCalled);
		// both runLoopMode and session should be released
		Assert.Contains(new IntPtr(30), fakeSystem.CfReleased);
		Assert.Contains(new IntPtr(10), fakeSystem.CfReleased);
	}

	[TestMethod]
	public void RunWatcher_RunLoopThrows_LogsError_AndCleansUp()
	{
		var logger = new FakeIWebLogger();
		var fakeSystem = new FakeMacSystem
		{
			SessionToReturn = new IntPtr(100),
			RunLoopToReturn = new IntPtr(200),
			RunLoopModeToReturn = new IntPtr(300),
			CfRunLoopRunShouldThrow = true
		};

		var sut = new MacMountWatcher(logger,
			new StorageHostFullPathFilesystem(logger), fakeSystem, 10);

		sut.RunWatcher();

		Assert.IsTrue(logger.TrackedExceptions.Exists(t =>
			t.Item2 != null && t.Item2.Contains("DiskArbitration watcher failed")));
		Assert.IsTrue(fakeSystem.DASessionUnscheduleCalled);
		Assert.Contains(new IntPtr(300), fakeSystem.CfReleased);
		Assert.Contains(new IntPtr(100), fakeSystem.CfReleased);
	}

	[TestMethod]
	public void OnDiskAppeared_Appeared()
	{
		var logger = new FakeIWebLogger();
		var fakeSystem = new FakeMacSystem
		{
			SessionToReturn = new IntPtr(100),
			RunLoopToReturn = new IntPtr(200),
			RunLoopModeToReturn = new IntPtr(300),
			CfRunLoopRunShouldThrow = true
		};

		var sut = new MacMountWatcher(logger,
			new StorageHostFullPathFilesystem(logger), fakeSystem, 10);

		sut.OnDiskAppeared(IntPtr.Zero, IntPtr.Zero);

		if ( OperatingSystem.IsMacOS() )
		{
			Assert.IsTrue(logger.TrackedInformation.Exists(t
				=> t.Item2 != null && t.Item2.Contains("macOS volume appeared")));
		}
		else
		{
			Assert.IsEmpty(logger.TrackedInformation);
			Assert.IsEmpty(logger.TrackedExceptions);
		}
	}

	[SuppressMessage("ReSharper", "InconsistentNaming")]
	private sealed class FakeMacSystem : IMacMountWatcherSystem
	{
		public IntPtr SessionToReturn { get; set; } = IntPtr.Zero;
		public IntPtr RunLoopToReturn { get; set; } = new(111);
		public IntPtr RunLoopModeToReturn { get; set; } = IntPtr.Zero;
		public bool CfRunLoopRunShouldThrow { get; set; }

		public bool DASessionUnscheduleCalled { get; private set; }
		public bool DARegisterDiskAppearedCalled { get; private set; }
		public bool DARegisterDiskDisappearedCalled { get; private set; }
		public bool CFRunLoopRunCalled { get; private set; }
		public List<IntPtr> CfReleased { get; } = new();

		public uint GetCfStringEncodingUtf8()
		{
			return 0u;
		}

		public string GetCfRunLoopDefaultMode()
		{
			return "kCFRunLoopDefaultMode";
		}

		public IntPtr DASessionCreateApi(IntPtr allocator)
		{
			return SessionToReturn;
		}

		public void DASessionScheduleWithRunLoopApi(IntPtr session, IntPtr runLoop,
			IntPtr runLoopMode)
		{
			// no-op
		}

		public void DASessionUnscheduleWithRunLoopApi(IntPtr session, IntPtr runLoop,
			IntPtr runLoopMode)
		{
			DASessionUnscheduleCalled = true;
		}

		public void DARegisterDiskAppearedCallbackApi(IntPtr session, IntPtr match,
			MacMountWatcherDelegate.DiskAppearedCallback callback, IntPtr context)
		{
			DARegisterDiskAppearedCalled = true;
		}

		public void DARegisterDiskDisappearedCallbackApi(IntPtr session, IntPtr match,
			MacMountWatcherDelegate.DiskDisappearedCallback callback, IntPtr context)
		{
			DARegisterDiskDisappearedCalled = true;
		}

		public IntPtr CFRunLoopGetCurrentApi()
		{
			return RunLoopToReturn;
		}

		public void CFRunLoopRunApi()
		{
			CFRunLoopRunCalled = true;
			if ( CfRunLoopRunShouldThrow )
			{
				throw new Exception("boom");
			}
			// otherwise return immediately to avoid blocking tests
		}

		public void CFRunLoopStopApi(IntPtr runLoop)
		{
			// no-op
		}

		public IntPtr CFStringCreateWithCStringApi(IntPtr allocator, string cStr, uint encoding)
		{
			return RunLoopModeToReturn;
		}

		public void CFReleaseApi(IntPtr cf)
		{
			CfReleased.Add(cf);
		}
	}
}

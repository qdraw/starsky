using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher;
using starsky.foundation.mountwatch.MountWatcher.Helpers.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.Services;

[TestClass]
public sealed class LinuxMountWatcherTest
{
	private static LinuxMountWatcher CreateSut(FakeLinuxMountWatcherSystem? system = null)
	{
		return new LinuxMountWatcher(new FakeIWebLogger(),
			system ?? new FakeLinuxMountWatcherSystem());
	}

	private static void SetIsRunning(LinuxMountWatcher watcher, bool value)
	{
		var field = typeof(BaseMountWatcher).GetField("IsRunning",
			BindingFlags.Instance | BindingFlags.NonPublic);
		field?.SetValue(watcher, value);
	}

	[TestMethod]
	public void LinuxMountWatcher_OnConstruction_IsNotNull()
	{
		var watcher = CreateSut();
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void LinuxMountWatcher_Start_ReturnsEarly_WhenAlreadyRunning()
	{
		var watcher = CreateSut();
		SetIsRunning(watcher, true);

		watcher.Start();

		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void LinuxMountWatcher_Stop_CanBeCalled_BeforeStart()
	{
		var watcher = CreateSut();
		watcher.Stop();
		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void LinuxMountWatcher_GetMountedVolumes_ReturnsEmpty_WhenProcMountsMissing()
	{
		var watcher = CreateSut(new FakeLinuxMountWatcherSystem { FileExistsResult = false });
		var volumes = watcher.GetMountedVolumes();
		Assert.IsEmpty(volumes);
	}

	[TestMethod]
	public void LinuxMountWatcher_GetMountedVolumes_ReturnsEmpty_WhenReadThrows()
	{
		var watcher = CreateSut(new FakeLinuxMountWatcherSystem
		{
			FileExistsResult = true, ThrowOnReadAllLines = true
		});
		var volumes = watcher.GetMountedVolumes();
		Assert.IsEmpty(volumes);
	}

	[TestMethod]
	public void LinuxMountWatcher_GetMountedVolumes_FiltersExpectedMounts()
	{
		var watcher = CreateSut(new FakeLinuxMountWatcherSystem
		{
			FileExistsResult = true,
			LinesToReturn =
			[
				"rootfs / rootfs rw 0 0",
				"sdb1 /media/usb ext4 rw 0 0",
				"tmpfs /tmp tmpfs rw 0 0",
				"sdc1 /mnt/share ext4 rw 0 0",
				"sdd1 /home/dion/drive ext4 rw 0 0",
				"invalid-line"
			]
		});

		var volumes = watcher.GetMountedVolumes();

		CollectionAssert.AreEqual(
			new List<string> { "/media/usb", "/mnt/share", "/home/dion/drive" }, volumes);
	}

	[TestMethod]
	public void LinuxMountWatcher_TryRunUdevWatcher_ReturnsFalse_WhenUdevUnavailable()
	{
		var watcher = CreateSut(new FakeLinuxMountWatcherSystem { UdevHandle = IntPtr.Zero });
		Assert.IsFalse(watcher.TryRunUdevWatcher());
	}

	[TestMethod]
	public void
		LinuxMountWatcher_TryRunUdevWatcher_ReturnsFalse_WhenMonitorUnavailable_AndUnrefsUdev()
	{
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11), MonitorHandle = IntPtr.Zero
		};
		var watcher = CreateSut(system);

		var result = watcher.TryRunUdevWatcher();

		Assert.IsFalse(result);
		Assert.AreEqual(1, system.UdevUnrefCalls);
	}

	[TestMethod]
	public void
		LinuxMountWatcher_TryRunUdevWatcher_ReturnsFalse_WhenFilterFails_AndUnrefsAllHandles()
	{
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11), MonitorHandle = new IntPtr(22), FilterResult = -1
		};
		var watcher = CreateSut(system);

		var result = watcher.TryRunUdevWatcher();

		Assert.IsFalse(result);
		Assert.AreEqual(1, system.MonitorUnrefCalls);
		Assert.AreEqual(1, system.UdevUnrefCalls);
	}

	[TestMethod]
	public void LinuxMountWatcher_TryRunUdevWatcher_ReturnsFalse_WhenEnableReceivingFails()
	{
		var watcher = CreateSut(new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11),
			MonitorHandle = new IntPtr(22),
			EnableReceivingResult = -1
		});

		Assert.IsFalse(watcher.TryRunUdevWatcher());
	}

	[TestMethod]
	public void LinuxMountWatcher_TryRunUdevWatcher_ReturnsFalse_WhenFdInvalid()
	{
		var watcher = CreateSut(new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11), MonitorHandle = new IntPtr(22), MonitorFd = -1
		});

		Assert.IsFalse(watcher.TryRunUdevWatcher());
	}

	[TestMethod]
	public void LinuxMountWatcher_TryRunUdevWatcher_HandlesDeviceEvents_AndReturnsTrue()
	{
		var mountEvents = new List<string>();
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11),
			MonitorHandle = new IntPtr(22),
			MonitorFd = 3,
			DevicesToReturn = new Queue<IntPtr>([new IntPtr(99)]),
			DeviceNodesToReturn = new Queue<string>(["/media/usb"])
		};
		var watcher = CreateSut(system);
		watcher.MountDetected += (_, args) => mountEvents.Add(args.MountPath);

		SetIsRunning(watcher, true);
		system.SleepCallback = () => SetIsRunning(watcher, false);

		var result = watcher.TryRunUdevWatcher();

		Assert.IsTrue(result);
		CollectionAssert.AreEqual(new List<string> { "/media/usb" }, mountEvents);
		Assert.AreEqual(1, system.DeviceUnrefCalls);
	}

	[TestMethod]
	public void LinuxMountWatcher_TryRunUdevWatcher_IgnoresEmptyDeviceNode()
	{
		var mountEvents = new List<string>();
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11),
			MonitorHandle = new IntPtr(22),
			MonitorFd = 3,
			DevicesToReturn = new Queue<IntPtr>([new IntPtr(44)]),
			DeviceNodesToReturn = new Queue<string>([string.Empty])
		};
		var watcher = CreateSut(system);
		watcher.MountDetected += (_, args) => mountEvents.Add(args.MountPath);

		SetIsRunning(watcher, true);
		system.SleepCallback = () => SetIsRunning(watcher, false);

		_ = watcher.TryRunUdevWatcher();

		Assert.IsEmpty(mountEvents);
		Assert.AreEqual(1, system.DeviceUnrefCalls);
	}

	[TestMethod]
	public void LinuxMountWatcher_RunWatcher_FallsBack_WhenUdevReturnsFalse()
	{
		var watcher = CreateSut(new FakeLinuxMountWatcherSystem { UdevHandle = IntPtr.Zero });
		SetIsRunning(watcher, false);

		watcher.RunWatcher();

		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void LinuxMountWatcher_RunWatcher_FallsBack_WhenUdevThrows()
	{
		var watcher = CreateSut(new FakeLinuxMountWatcherSystem { ThrowOnUdevNew = true });
		SetIsRunning(watcher, false);

		watcher.RunWatcher();

		Assert.IsNotNull(watcher);
	}

	[TestMethod]
	public void LinuxMountWatcher_Start_SetsRunningAndStartsThread()
	{
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11), MonitorHandle = new IntPtr(22), MonitorFd = 3
		};
		var watcher = CreateSut(system);
		system.SleepCallback = () => SetIsRunning(watcher, false);

		watcher.Start();
		Thread.Sleep(20);
		watcher.Stop();

		Assert.AreEqual(1, system.UdevUnrefCalls);
	}

	[TestMethod]
	public void LinuxMountWatcher_RunWatcher_Returns_WhenUdevWatcherSucceeds()
	{
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11), MonitorHandle = new IntPtr(22), MonitorFd = 3
		};
		var watcher = CreateSut(system);
		SetIsRunning(watcher, true);
		system.SleepCallback = () => SetIsRunning(watcher, false);

		watcher.RunWatcher();

		Assert.AreEqual(1, system.UdevUnrefCalls);
	}

	[TestMethod]
	public void LinuxMountWatcher_ParseMountLines_IgnoresInvalidEntries()
	{
		var parsed = LinuxMountWatcher.ParseMountLines([
			"invalid",
			"sda1 /proc proc rw 0 0",
			"sdb1 /mnt/drive ext4 rw 0 0"
		]);

		CollectionAssert.AreEqual(new List<string> { "/mnt/drive" }, parsed);
	}

	[TestMethod]
	public void LinuxMountWatcher_ShouldIncludeMount_RespectsIncludeAndExcludeRules()
	{
		Assert.IsFalse(LinuxMountWatcher.ShouldIncludeMount("/sys/kernel"));
		Assert.IsFalse(LinuxMountWatcher.ShouldIncludeMount("/tmp/cache"));
		Assert.IsTrue(LinuxMountWatcher.ShouldIncludeMount("/media/cam"));
		Assert.IsTrue(LinuxMountWatcher.ShouldIncludeMount("/mnt/share"));
		Assert.IsTrue(LinuxMountWatcher.ShouldIncludeMount("/home/dion/disk"));
		Assert.IsFalse(LinuxMountWatcher.ShouldIncludeMount("/opt/other"));
	}

	private sealed class FakeLinuxMountWatcherSystem : ILinuxMountWatcherSystem
	{
		public bool ThrowOnUdevNew { get; set; }
		public IntPtr UdevHandle { get; set; } = new(1);
		public IntPtr MonitorHandle { get; set; } = new(2);
		public int FilterResult { get; set; }
		public int EnableReceivingResult { get; set; }
		public int MonitorFd { get; set; } = 5;
		public Queue<IntPtr> DevicesToReturn { get; set; } = new();
		public Queue<string> DeviceNodesToReturn { get; set; } = new();
		public int MonitorUnrefCalls { get; private set; }
		public int UdevUnrefCalls { get; private set; }
		public int DeviceUnrefCalls { get; private set; }
		public bool FileExistsResult { get; set; } = true;
		public bool ThrowOnReadAllLines { get; set; }
		public string[] LinesToReturn { get; set; } = [];
		public Action? SleepCallback { get; set; }

		public IntPtr UdevNew()
		{
			if ( ThrowOnUdevNew )
			{
				throw new InvalidOperationException("boom");
			}

			return UdevHandle;
		}

		public void UdevUnref(IntPtr udev)
		{
			UdevUnrefCalls++;
		}

		public IntPtr UdevMonitorNewFromNetlink(IntPtr udev, string name)
		{
			return MonitorHandle;
		}

		public int UdevMonitorFilterAddMatchSubsystemDevtype(IntPtr monitor, string subsystem,
			IntPtr devtype)
		{
			return FilterResult;
		}

		public int UdevMonitorEnableReceiving(IntPtr monitor)
		{
			return EnableReceivingResult;
		}

		public int UdevMonitorGetFd(IntPtr monitor)
		{
			return MonitorFd;
		}

		public IntPtr UdevMonitorReceiveDevice(IntPtr monitor)
		{
			return DevicesToReturn.Count > 0 ? DevicesToReturn.Dequeue() : IntPtr.Zero;
		}

		public void UdevDeviceUnref(IntPtr device)
		{
			DeviceUnrefCalls++;
		}

		public string UdevDeviceGetDevnode(IntPtr device)
		{
			return DeviceNodesToReturn.Count > 0 ? DeviceNodesToReturn.Dequeue() : string.Empty;
		}

		public void UdevMonitorUnref(IntPtr monitor)
		{
			MonitorUnrefCalls++;
		}

		public bool FileExists(string path)
		{
			return FileExistsResult;
		}

		public string[] ReadAllLines(string path)
		{
			if ( ThrowOnReadAllLines )
			{
				throw new InvalidOperationException("io");
			}

			return LinesToReturn;
		}

		public void Sleep(int milliseconds)
		{
			SleepCallback?.Invoke();
		}
	}
}

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.mountwatch.MountWatcher.Linux;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.mountwatch.MountWatcher.Linux;

[TestClass]
public sealed class UdevWatcherTest
{
	[TestMethod]
	public void TryRunUdevWatcher_ReturnsFalse_WhenUdevUnavailable()
	{
		var system = new FakeLinuxMountWatcherSystem { UdevHandle = IntPtr.Zero };
		const bool running = false;

		var watcher = new UdevWatcher(system, _ => null, _ => { }, () => running);

		Assert.IsFalse(watcher.TryRunUdevWatcher());
	}

	[TestMethod]
	public void TryRunUdevWatcher_ReturnsFalse_WhenMonitorUnavailable_AndUnrefsUdev()
	{
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11), MonitorHandle = IntPtr.Zero
		};
		var running = false;

		var watcher = new UdevWatcher(system, _ => null, _ => { }, () => running);

		var result = watcher.TryRunUdevWatcher();

		Assert.IsFalse(result);
		Assert.AreEqual(1, system.UdevUnrefCalls);
	}

	[TestMethod]
	public void TryRunUdevWatcher_ReturnsFalse_WhenFilterFails_AndUnrefsAllHandles()
	{
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11), MonitorHandle = new IntPtr(22), FilterResult = -1
		};
		var running = false;

		var watcher = new UdevWatcher(system, _ => null, _ => { }, () => running);

		var result = watcher.TryRunUdevWatcher();

		Assert.IsFalse(result);
		Assert.AreEqual(1, system.MonitorUnrefCalls);
		Assert.AreEqual(1, system.UdevUnrefCalls);
	}

	[TestMethod]
	public void TryRunUdevWatcher_ReturnsFalse_WhenEnableReceivingFails()
	{
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11),
			MonitorHandle = new IntPtr(22),
			EnableReceivingResult = -1
		};
		var running = false;

		var watcher = new UdevWatcher(system, _ => null, _ => { }, () => running);

		Assert.IsFalse(watcher.TryRunUdevWatcher());
	}

	[TestMethod]
	public void TryRunUdevWatcher_ReturnsFalse_WhenFdInvalid()
	{
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11), MonitorHandle = new IntPtr(22), MonitorFd = -1
		};
		var running = false;

		var watcher = new UdevWatcher(system, _ => null, _ => { }, () => running);

		Assert.IsFalse(watcher.TryRunUdevWatcher());
	}

	[TestMethod]
	public void TryRunUdevWatcher_HandlesDeviceEvents_AndReturnsTrue()
	{
		var mountEvents = new List<string>();
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11),
			MonitorHandle = new IntPtr(22),
			MonitorFd = 3,
			DevicesToReturn = new Queue<IntPtr>(new[] { new IntPtr(99) }),
			DeviceNodesToReturn = new Queue<string>(new[] { "/dev/sdb1" }),
			FileExistsResult = true,
			LinesToReturn = new[] { "/dev/sdb1 /media/usb ext4 rw 0 0" }
		};

		var running = true;
		system.SleepCallback = () => running = false;

		var watcher = new UdevWatcher(system, dn => system.MapDeviceToMount(dn),
			m => mountEvents.Add(m), () => running);

		var result = watcher.TryRunUdevWatcher();

		Assert.IsTrue(result);
		CollectionAssert.AreEqual(new List<string> { "/media/usb" }, mountEvents);
		Assert.AreEqual(1, system.DeviceUnrefCalls);
	}

	[TestMethod]
	public void TryRunUdevWatcher_IgnoresEmptyDeviceNode()
	{
		var mountEvents = new List<string>();
		var system = new FakeLinuxMountWatcherSystem
		{
			UdevHandle = new IntPtr(11),
			MonitorHandle = new IntPtr(22),
			MonitorFd = 3,
			DevicesToReturn = new Queue<IntPtr>(new[] { new IntPtr(44) }),
			DeviceNodesToReturn = new Queue<string>(new[] { string.Empty })
		};

		var running = true;
		system.SleepCallback = () => running = false;

		var watcher = new UdevWatcher(system, _ => null, m => mountEvents.Add(m), () => running);

		_ = watcher.TryRunUdevWatcher();

		Assert.IsEmpty(mountEvents);
		Assert.AreEqual(1, system.DeviceUnrefCalls);
	}
}

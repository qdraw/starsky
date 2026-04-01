using System;
using System.Collections.Generic;
using starsky.foundation.mountwatch.MountWatcher.Helpers.Interfaces;

namespace starskytest.FakeMocks;

public sealed class FakeLinuxMountWatcherSystem : ILinuxMountWatcherSystem
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
	public string[] LinesToReturn { get; set; } = Array.Empty<string>();
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

	public string? UdevDeviceGetDevnode(IntPtr device)
	{
		return DeviceNodesToReturn.Count > 0 ? DeviceNodesToReturn.Dequeue() : null;
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

	public string? MapDeviceToMount(string deviceNode)
	{
		if ( !FileExists("/proc/mounts") )
		{
			return null;
		}

		foreach ( var line in LinesToReturn )
		{
			var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if ( parts.Length < 2 )
			{
				continue;
			}

			if ( parts[0].Equals(deviceNode, StringComparison.OrdinalIgnoreCase) )
			{
				return parts[1];
			}
		}

		return null;
	}
}

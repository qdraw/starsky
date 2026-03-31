using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using starsky.foundation.mountwatch.MountWatcher.Helpers.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher.Linux;

internal sealed class LinuxMountWatcherSystem : ILinuxMountWatcherSystem
{
	public IntPtr UdevNew()
	{
		return udev_new();
	}

	public void UdevUnref(IntPtr udev)
	{
		udev_unref(udev);
	}

	public IntPtr UdevMonitorNewFromNetlink(IntPtr udev, string name)
	{
		return udev_monitor_new_from_netlink(udev, name);
	}

	public int UdevMonitorFilterAddMatchSubsystemDevtype(IntPtr monitor, string subsystem,
		IntPtr devtype)
	{
		return udev_monitor_filter_add_match_subsystem_devtype(monitor, subsystem, devtype);
	}

	public int UdevMonitorEnableReceiving(IntPtr monitor)
	{
		return udev_monitor_enable_receiving(monitor);
	}

	public int UdevMonitorGetFd(IntPtr monitor)
	{
		return udev_monitor_get_fd(monitor);
	}

	public IntPtr UdevMonitorReceiveDevice(IntPtr monitor)
	{
		return udev_monitor_receive_device(monitor);
	}

	public void UdevDeviceUnref(IntPtr device)
	{
		udev_device_unref(device);
	}

	public string UdevDeviceGetDevnode(IntPtr device)
	{
		return udev_device_get_devnode(device);
	}

	public void UdevMonitorUnref(IntPtr monitor)
	{
		udev_monitor_unref(monitor);
	}

	public bool FileExists(string path)
	{
		return File.Exists(path);
	}

	public string[] ReadAllLines(string path)
	{
		return File.ReadAllLines(path);
	}

	public void Sleep(int milliseconds)
	{
		Thread.Sleep(milliseconds);
	}

	[DllImport("libudev.so.1", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr udev_new();

	[DllImport("libudev.so.1", CallingConvention = CallingConvention.Cdecl)]
	private static extern void udev_unref(IntPtr udev);

	[DllImport("libudev.so.1", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr udev_monitor_new_from_netlink(IntPtr udev, string name);

	[DllImport("libudev.so.1", CallingConvention = CallingConvention.Cdecl)]
	private static extern int udev_monitor_filter_add_match_subsystem_devtype(
		IntPtr monitor, string subsystem, IntPtr devtype);

	[DllImport("libudev.so.1", CallingConvention = CallingConvention.Cdecl)]
	private static extern int udev_monitor_enable_receiving(IntPtr monitor);

	[DllImport("libudev.so.1", CallingConvention = CallingConvention.Cdecl)]
	private static extern int udev_monitor_get_fd(IntPtr monitor);

	[DllImport("libudev.so.1", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr udev_monitor_receive_device(IntPtr monitor);

	[DllImport("libudev.so.1", CallingConvention = CallingConvention.Cdecl)]
	private static extern void udev_device_unref(IntPtr device);

	[DllImport("libudev.so.1", CallingConvention = CallingConvention.Cdecl)]
	private static extern string udev_device_get_devnode(IntPtr device);

	[DllImport("libudev.so.1", CallingConvention = CallingConvention.Cdecl)]
	private static extern void udev_monitor_unref(IntPtr monitor);
}

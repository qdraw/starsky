using System;
using starsky.foundation.mountwatch.MountWatcher.Helpers.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher.Linux;

internal class UdevWatcher
{
	private readonly Func<bool> _isRunning;
	private readonly Action<string> _onMountDetected;
	private readonly Func<string, string?> _resolveMountPoint;
	private readonly ILinuxMountWatcherSystem _system;
	private IntPtr _monitor;

	private IntPtr _udev;

	public UdevWatcher(
		ILinuxMountWatcherSystem system,
		Func<string, string?> resolveMountPoint,
		Action<string> onMountDetected,
		Func<bool> isRunning)
	{
		_system = system;
		_resolveMountPoint = resolveMountPoint;
		_onMountDetected = onMountDetected;
		_isRunning = isRunning;
	}

	/// <summary>
	///     Attempt to use udev for event-driven mount detection
	/// </summary>
	internal bool TryRunUdevWatcher()
	{
		try
		{
			if ( !InitializeMonitor() )
			{
				return false;
			}

			while ( _isRunning() )
			{
				var device = _system.UdevMonitorReceiveDevice(_monitor);
				if ( device != IntPtr.Zero )
				{
					HandleDevice(device);
				}

				_system.Sleep(100);
			}

			return true;
		}
		finally
		{
			if ( _monitor != IntPtr.Zero )
			{
				_system.UdevMonitorUnref(_monitor);
			}

			if ( _udev != IntPtr.Zero )
			{
				_system.UdevUnref(_udev);
			}
		}
	}

	private bool InitializeMonitor()
	{
		_udev = _system.UdevNew();
		if ( _udev == IntPtr.Zero )
		{
			return false;
		}

		_monitor = _system.UdevMonitorNewFromNetlink(_udev, "udev");
		if ( _monitor == IntPtr.Zero )
		{
			return false;
		}

		// Monitor block device changes
		if ( _system.UdevMonitorFilterAddMatchSubsystemDevtype(_monitor, "block", IntPtr.Zero) < 0 )
		{
			return false;
		}

		if ( _system.UdevMonitorEnableReceiving(_monitor) < 0 )
		{
			return false;
		}

		var fd = _system.UdevMonitorGetFd(_monitor);
		return fd >= 0;
	}

	private void HandleDevice(IntPtr device)
	{
		// IMPORTANT: Get devnode BEFORE unreffing the device, since udev owns the string pointer
		var devNode = _system.UdevDeviceGetDevnode(device);

		// Now safe to unref the device
		_system.UdevDeviceUnref(device);

		if ( string.IsNullOrEmpty(devNode) )
		{
			return;
		}

		// Resolve the device node to its mount point.
		// udev reports /dev/sda1, but we need /mnt/usb or /media/camera
		var mountPoint = _resolveMountPoint(devNode);
		if ( string.IsNullOrEmpty(mountPoint) )
		{
			return;
		}

		_onMountDetected(mountPoint);
	}
}

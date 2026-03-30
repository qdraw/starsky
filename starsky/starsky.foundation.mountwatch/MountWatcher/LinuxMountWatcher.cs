using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     Linux mount watcher using udev for event-driven notifications (with polling fallback)
/// </summary>
internal class LinuxMountWatcher : BaseMountWatcher
{
	/// <summary>
	///     Start watching for mount events using udev or polling fallback
	/// </summary>
	public override void Start()
	{
		if ( IsRunning )
		{
			return;
		}

		IsRunning = true;
		WatchThread = new Thread(RunWatcher) { IsBackground = true };
		WatchThread.Start();
	}

	/// <summary>
	///     Stop watching for mount events
	/// </summary>
	public override void Stop()
	{
		IsRunning = false;
		WatchThread?.Join(TimeSpan.FromSeconds(5));
	}

	/// <summary>
	///     Get currently mounted volumes
	/// </summary>
	public override List<string> GetMountedVolumes()
	{
		return GetCurrentMounts();
	}

	// udev library P/Invoke declarations
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

	/// <summary>
	///     Run the udev event watcher
	/// </summary>
	private void RunWatcher()
	{
		try
		{
			// Try to use udev
			if ( TryRunUdevWatcher() )
			{
				return;
			}
		}
		catch
		{
			// Fallback to polling
		}

		// Fallback to polling
		RunPollingFallback();
	}

	/// <summary>
	///     Attempt to use udev for event-driven mount detection
	/// </summary>
	private bool TryRunUdevWatcher()
	{
		var udev = IntPtr.Zero;
		var monitor = IntPtr.Zero;

		try
		{
			udev = udev_new();
			if ( udev == IntPtr.Zero )
			{
				return false;
			}

			monitor = udev_monitor_new_from_netlink(udev, "udev");
			if ( monitor == IntPtr.Zero )
			{
				return false;
			}

			// Monitor block device changes
			if ( udev_monitor_filter_add_match_subsystem_devtype(monitor, "block", IntPtr.Zero) <
			     0 )
			{
				return false;
			}

			if ( udev_monitor_enable_receiving(monitor) < 0 )
			{
				return false;
			}

			var fd = udev_monitor_get_fd(monitor);
			if ( fd < 0 )
			{
				return false;
			}

			// Monitor for events
			while ( IsRunning )
			{
				var device = udev_monitor_receive_device(monitor);
				if ( device != IntPtr.Zero )
				{
					var devNode = udev_device_get_devnode(device);
					if ( !string.IsNullOrEmpty(devNode) )
					{
						OnMountDetected(devNode);
					}

					udev_device_unref(device);
				}

				Thread.Sleep(100);
			}

			return true;
		}
		finally
		{
			if ( monitor != IntPtr.Zero )
			{
				udev_monitor_unref(monitor);
			}

			if ( udev != IntPtr.Zero )
			{
				udev_unref(udev);
			}
		}
	}


	/// <summary>
	///     Get list of current mount points from /proc/mounts
	/// </summary>
	private static List<string> GetCurrentMounts()
	{
		var mounts = new List<string>();

		try
		{
			if ( !File.Exists("/proc/mounts") )
			{
				return mounts;
			}

			var lines = File.ReadAllLines("/proc/mounts");
			foreach ( var line in lines )
			{
				var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
				if ( parts.Length < 2 )
				{
					continue;
				}

				var mountPath = parts[1];

				// Skip system mounts
				if ( ShouldIncludeMount(mountPath) )
				{
					mounts.Add(mountPath);
				}
			}
		}
		catch
		{
			// Ignore errors
		}

		return mounts;
	}

	/// <summary>
	///     Determine if a mount path should be monitored
	/// </summary>
	private static bool ShouldIncludeMount(string mountPath)
	{
		// Skip system mounts
		var excludePrefixes = new[] { "/sys", "/proc", "/dev", "/run", "/boot", "/var", "/tmp" };
		if ( excludePrefixes.Any(prefix =>
			    mountPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) )
		{
			return false;
		}

		// Include user-mounted filesystems
		return mountPath.StartsWith("/media", StringComparison.OrdinalIgnoreCase) ||
		       mountPath.StartsWith("/mnt", StringComparison.OrdinalIgnoreCase) ||
		       mountPath.StartsWith("/home", StringComparison.OrdinalIgnoreCase);
	}
}

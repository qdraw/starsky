using System;
using System.Collections.Generic;
using System.Threading;
using starsky.foundation.mountwatch.MountWatcher.Helpers.Interfaces;
using starsky.foundation.mountwatch.MountWatcher.Linux;
using starsky.foundation.platform.Interfaces;

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     Linux mount watcher using udev for event-driven notifications (with polling fallback)
/// </summary>
internal class LinuxMountWatcher(IWebLogger logger, int pollIntervalMs)
	: BaseMountWatcher(logger, pollIntervalMs)
{
	private readonly ILinuxMountWatcherSystem _system = new LinuxMountWatcherSystem();

	internal LinuxMountWatcher(IWebLogger logger, ILinuxMountWatcherSystem system,
		int pollIntervalMs) : this(logger, pollIntervalMs)
	{
		_system = system;
	}

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

	/// <summary>
	///     Run the udev event watcher
	/// </summary>
	internal void RunWatcher()
	{
		try
		{
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

	internal bool TryRunUdevWatcher()
	{
		var watcher = new UdevWatcher(_system,
			ResolveMountPoint,
			OnMountDetected,
			() => IsRunning);

		return watcher.TryRunUdevWatcher();
	}


	/// <summary>
	///     Get list of current mount points from /proc/mounts
	/// </summary>
	internal List<string> GetCurrentMounts()
	{
		var mounts = new List<string>();

		try
		{
			if ( !_system.FileExists("/proc/mounts") )
			{
				return new List<string>();
			}

			var lines = _system.ReadAllLines("/proc/mounts");
			mounts.AddRange(ParseMountLines(lines));
		}
		catch
		{
			// Ignore errors
		}

		return mounts;
	}

	internal static List<string> ParseMountLines(IEnumerable<string> lines)
	{
		var mounts = new List<string>();

		foreach ( var line in lines )
		{
			var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if ( parts.Length < 2 )
			{
				continue;
			}

			var mountPath = parts[1];
			if ( ShouldIncludeMount(mountPath) )
			{
				mounts.Add(mountPath);
			}
		}

		return mounts;
	}

	/// <summary>
	///     Resolve a device node (e.g., /dev/sda1) to its mount point (e.g., /mnt/usb)
	///     by searching /proc/mounts
	/// </summary>
	internal string? ResolveMountPoint(string deviceNode)
	{
		try
		{
			if ( !_system.FileExists("/proc/mounts") )
			{
				return null;
			}

			var lines = _system.ReadAllLines("/proc/mounts");
			foreach ( var line in lines )
			{
				var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if ( parts.Length < 2 )
				{
					continue;
				}

				var device = parts[0];
				var mountPath = parts[1];

				// Match the device node (exact match or with partition number variations)
				if ( device.Equals(deviceNode, StringComparison.OrdinalIgnoreCase) &&
				     ShouldIncludeMount(mountPath) )
				{
					return mountPath;
				}
			}
		}
		catch
		{
			// Fallback: return null if we can't read /proc/mounts
		}

		return null;
	}

	/// <summary>
	///     Determine if a mount path should be monitored
	/// </summary>
	internal static bool ShouldIncludeMount(string mountPath)
	{
		// Include user-mounted filesystems
		return mountPath.StartsWith("/media", StringComparison.OrdinalIgnoreCase) ||
		       mountPath.StartsWith("/mnt", StringComparison.OrdinalIgnoreCase) ||
		       mountPath.StartsWith("/home", StringComparison.OrdinalIgnoreCase);
	}
}

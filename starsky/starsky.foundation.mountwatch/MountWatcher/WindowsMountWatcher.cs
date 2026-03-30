using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     Windows mount watcher using WMI for event-driven drive detection
/// </summary>
internal class WindowsMountWatcher : BaseMountWatcher
{
	// ManagementEventWatcher is Windows-only – held as object to avoid
	// CA1416 on the field itself when the containing class is cross-platform.
	private object? _watcher;

	/// <summary>
	///     Start watching for mount events using WMI
	/// </summary>
	public override void Start()
	{
		if ( IsRunning )
		{
			return;
		}

		IsRunning = true;

		if ( OperatingSystem.IsWindows() )
		{
			StartWmiWatcher();
		}
		else
		{
			RunPollingFallback();
		}
	}

	/// <summary>
	///     Stop watching for mount events
	/// </summary>
	public override void Stop()
	{
		IsRunning = false;

		if ( OperatingSystem.IsWindows() )
		{
			StopWmiWatcher();
		}
	}

	/// <summary>
	///     Get currently mounted volumes
	/// </summary>
	public override List<string> GetMountedVolumes()
	{
		try
		{
			var drives = DriveInfo.GetDrives()
				.Where(d => d.IsReady)
				.Select(d => d.RootDirectory.FullName)
				.ToList();

			return drives;
		}
		catch
		{
			return [];
		}
	}

	[SupportedOSPlatform("windows")]
	private void StartWmiWatcher()
	{
		try
		{
			var query = new WqlEventQuery(
				"SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
			var mgmtWatcher = new ManagementEventWatcher(query);
			mgmtWatcher.EventArrived += OnVolumeChanged;
			mgmtWatcher.Start();
			_watcher = mgmtWatcher;
		}
		catch
		{
			RunPollingFallback();
		}
	}

	[SupportedOSPlatform("windows")]
	private void StopWmiWatcher()
	{
		try
		{
			if ( _watcher is not ManagementEventWatcher mgmt )
			{
				return;
			}

			mgmt.Stop();
			mgmt.Dispose();
		}
		catch
		{
			// Ignore cleanup errors
		}
	}

	/// <summary>
	///     Handle WMI volume change events (Windows only)
	/// </summary>
	[SupportedOSPlatform("windows")]
	private void OnVolumeChanged(object? sender, EventArrivedEventArgs e)
	{
		try
		{
			var newDrives = GetMountedVolumes();
			if ( newDrives.Count > 0 )
			{
				OnMountDetected(newDrives.First());
			}
		}
		catch
		{
			// Ignore errors
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using starsky.foundation.platform.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     Windows mount watcher using WMI for event-driven drive detection
/// </summary>
internal class WindowsMountWatcher(IWebLogger logger) : BaseMountWatcher(logger)
{
	// ManagementEventWatcher is Windows-only – held as object to avoid
	// CA1416 on the field itself when the containing class is cross-platform.
	private object? _watcher;
	private readonly HashSet<string> _knownMountedVolumes =
		new(StringComparer.OrdinalIgnoreCase);

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
		SeedKnownMounts(GetMountedVolumes());

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
		catch ( Exception ex )
		{
			logger.LogError(ex, 
				"Failed to start WMI watcher, falling back to polling");
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
			if ( TryTrackEventDrive(e, out var eventDrive) )
			{
				OnMountDetected(eventDrive);
			}

			var newDrives = DetectNewMounts(GetMountedVolumes());
			foreach ( var drive in newDrives )
			{
				OnMountDetected(drive);
			}
		}
		catch
		{
			// Ignore errors
		}
	}

	internal void SeedKnownMounts(IEnumerable<string> mountedVolumes)
	{
		_knownMountedVolumes.Clear();
		foreach ( var volume in mountedVolumes.Where(v => !string.IsNullOrWhiteSpace(v)) )
		{
			_knownMountedVolumes.Add(NormalizeDrive(volume));
		}
	}

	internal List<string> DetectNewMounts(IEnumerable<string> mountedVolumes)
	{
		var currentMounts = mountedVolumes
			.Where(v => !string.IsNullOrWhiteSpace(v))
			.Select(NormalizeDrive)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		var currentSet = new HashSet<string>(currentMounts, StringComparer.OrdinalIgnoreCase);
		var newMounts = currentMounts
			.Where(m => !_knownMountedVolumes.Contains(m))
			.ToList();

		_knownMountedVolumes.Clear();
		foreach ( var mount in currentSet )
		{
			_knownMountedVolumes.Add(mount);
		}

		return newMounts;
	}

	internal bool TryTrackEventDrive(string? driveName, out string normalizedDrive)
	{
		normalizedDrive = string.Empty;
		if ( string.IsNullOrWhiteSpace(driveName) )
		{
			return false;
		}

		normalizedDrive = NormalizeDrive(driveName);
		return _knownMountedVolumes.Add(normalizedDrive);
	}

	[SupportedOSPlatform("windows")]
	private bool TryTrackEventDrive(EventArrivedEventArgs e, out string normalizedDrive)
	{
		normalizedDrive = string.Empty;
		try
		{
			var driveName = e.NewEvent?.Properties?["DriveName"]?.Value as string;
			return TryTrackEventDrive(driveName, out normalizedDrive);
		}
		catch
		{
			return false;
		}
	}

	internal static string NormalizeDrive(string driveName)
	{
		var normalized = driveName.Trim();
		if ( normalized is [_, ':'] )
		{
			return normalized + "\\";
		}

		return normalized;
	}
}

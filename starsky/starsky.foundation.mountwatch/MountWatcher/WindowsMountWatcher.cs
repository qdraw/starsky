using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
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
		logger.LogInformation(
			$"Windows mount watcher baseline seeded: {string.Join(", ", _knownMountedVolumes)}");

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
				"SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 OR EventType = 3");
			var mgmtWatcher = new ManagementEventWatcher(query);
			mgmtWatcher.EventArrived += OnVolumeChanged;
			mgmtWatcher.Start();
			_watcher = mgmtWatcher;
			logger.LogInformation("Windows WMI watcher started for Win32_VolumeChangeEvent EventType=2 OR EventType=3");
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
			var eventTypeStr = e.NewEvent?.Properties?["EventType"]?.Value?.ToString() ?? "<null>";
			var rawDriveName = e.NewEvent?.Properties?["DriveName"]?.Value?.ToString() ?? "<null>";
			logger.LogInformation(
				$"Windows volume event received: EventType={eventTypeStr}, DriveName={rawDriveName}");

			if ( int.TryParse(eventTypeStr, out var eventType) && eventType == 3 )
			{
				HandleVolumeRemoval(rawDriveName);
				return;
			}

			var eventTracked = TryTrackEventDrive(e, out var eventDrive);

			if ( eventTracked )
			{
				logger.LogInformation($"Windows volume event tracked as new mount: {eventDrive}");
				OnMountDetected(eventDrive);
			}
			else
			{
				logger.LogInformation("Windows volume event drive was empty or already known");
			}

			var newDrives = DetectNewMountsWithRetry(
				GetMountedVolumes, 3, 250);
			if ( newDrives.Count == 0 )
			{
				logger.LogInformation("Windows retry mount scan found no new drives");
			}

			foreach ( var drive in newDrives )
			{
				if ( eventTracked &&
				     drive.Equals(eventDrive, StringComparison.OrdinalIgnoreCase) )
				{
					logger.LogInformation($"Windows retry mount scan duplicate ignored: {drive}");
					continue;
				}

				logger.LogInformation($"Windows retry mount scan detected new drive: {drive}");
				OnMountDetected(drive);
			}
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Windows volume event handling failed: {ex.Message}");
		}
	}

	/// <summary>
	///     Remove an ejected drive from the known-mounts baseline so that
	///     re-inserting the same drive is recognised as a new mount.
	/// </summary>
	internal void HandleVolumeRemoval(string? rawDriveName)
	{
		if ( string.IsNullOrWhiteSpace(rawDriveName) )
		{
			logger.LogInformation("Windows volume removal event: drive name was empty, baseline unchanged");
			return;
		}

		var normalized = NormalizeDrive(rawDriveName);
		var removed = _knownMountedVolumes.Remove(normalized);
		logger.LogInformation(removed
			? $"Windows volume removal event: removed '{normalized}' from baseline"
			: $"Windows volume removal event: '{normalized}' was not in baseline");
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

	internal List<string> DetectNewMountsWithRetry(
		Func<List<string>> getMountedVolumes,
		int attempts,
		int delayMilliseconds,
		Action<int>? sleepAction = null)
	{
		sleepAction ??= Thread.Sleep;

		for ( var attempt = 0; attempt < attempts; attempt++ )
		{
			var snapshot = getMountedVolumes();
			logger.LogInformation(
				$"Windows retry mount scan attempt {attempt + 1}/{attempts}: [{string.Join(", ", snapshot)}]");

			var newMounts = DetectNewMounts(snapshot);
			if ( newMounts.Count > 0 )
			{
				logger.LogInformation(
					$"Windows retry mount scan success on attempt {attempt + 1}: [{string.Join(", ", newMounts)}]");
				return newMounts;
			}

			if ( attempt < attempts - 1 )
			{
				logger.LogInformation(
					$"Windows retry mount scan sleeping {delayMilliseconds}ms before next attempt");
				sleepAction(delayMilliseconds);
			}
		}

		return [];
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
			var driveName = e.NewEvent?.Properties?["DriveName"]?.Value?.ToString();
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using starsky.foundation.mountwatch.MountWatcher.Windows;
using starsky.foundation.mountwatch.MountWatcher.Windows.Interfaces;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;

[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.mountwatch.MountWatcher;

/// <summary>
///     Windows mount watcher using WMI for event-driven drive detection
/// </summary>
internal class WindowsMountWatcher : BaseMountWatcher
{
	private readonly HashSet<string> _knownMountedVolumes =
		new(StringComparer.OrdinalIgnoreCase);

	private readonly Func<OSPlatform> _platformResolver;
	private readonly IWindowsMountWatcherSystem _system;

	// ManagementEventWatcher is Windows-only – held as object to avoid
	// CA1416 on the field itself when the containing class is cross-platform.
	private object? _watcher;

	public WindowsMountWatcher(IWebLogger logger, int pollIntervalMs) :
		this(logger, OperatingSystemHelper.GetPlatform, pollIntervalMs)
	{
	}

	internal WindowsMountWatcher(IWebLogger logger,
		Func<OSPlatform>? platformResolver, int pollIntervalMs,
		IWindowsMountWatcherSystem? system = null) :
		base(logger, pollIntervalMs)
	{
		_platformResolver = platformResolver ?? OperatingSystemHelper.GetPlatform;
		_system = system ?? new WindowsMountWatcherSystem();
	}

	internal TaskCompletionSource Started { get; } = new();

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

		if ( IsWindows() )
		{
			StartWmiWatcher();
		}
		else
		{
			Started.TrySetResult();
			RunPollingFallback();
		}
	}

	/// <summary>
	///     Stop watching for mount events
	/// </summary>
	public override void Stop()
	{
		IsRunning = false;

		if ( IsWindows() )
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
			var drives = _system.GetDrives()
				.Where(d => d.IsReady)
				.Select(d => d.RootDirectory.FullName)
				.ToList();
			return drives;
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, "Failed to get mounted volumes");
			return new List<string>();
		}
	}

	private bool IsWindows()
	{
		return _platformResolver().Equals(OSPlatform.Windows);
	}

	internal void StartWmiWatcher()
	{
		try
		{
			var mgmtWatcher = _system.CreateManagementWatcher(
				"SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 OR EventType = 3");

			if ( mgmtWatcher == null )
			{
				logger.LogInformation("CreateManagementWatcher returned null");
				return;
			}

			_system.AddEventArrivedHandler(mgmtWatcher, OnVolumeChanged);
			_system.StartWatcher(mgmtWatcher);
			Started.TrySetResult();

			_watcher = mgmtWatcher;
			logger.LogInformation(
				"Windows WMI watcher started for Win32_VolumeChangeEvent EventType=2 OR EventType=3");
		}
		catch ( Exception ex )
		{
			logger.LogError(ex,
				"Failed to start WMI watcher, falling back to polling");
			RunPollingFallback();
		}
	}

	private void StopWmiWatcher()
	{
		if ( _watcher == null )
		{
			return;
		}

		_system.StopWatcher(_watcher);
		_system.DisposeWatcher(_watcher);
	}

	/// <summary>
	///     Handle WMI volume change events (Windows only)
	/// </summary>
	[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
	internal void OnVolumeChanged(object? sender, EventArrivedEventArgs arrivedEvent)
	{
		try
		{
			var (eventTypeStr, rawDriveName) = _system.MapEvent(arrivedEvent);
			OnVolumeChanged(eventTypeStr, rawDriveName);
		}
		catch ( Exception ex )
		{
			logger.LogError(ex, $"Windows volume event handling failed: {ex.Message}");
		}
	}

	internal void OnVolumeChanged(string eventTypeStr, string rawDriveName)
	{
		logger.LogInformation(
			$"Windows volume event received: EventType={eventTypeStr}, DriveName={rawDriveName}");

		if ( int.TryParse(eventTypeStr, out var eventType) && eventType == 3 )
		{
			HandleVolumeRemoval(rawDriveName);
			return;
		}

		var eventTracked = TryTrackEventDrive(rawDriveName, out var eventDrive);

		if ( eventTracked )
		{
			logger.LogInformation($"Windows volume event tracked as new mount: {eventDrive}");
			OnMountDetected(eventDrive);
		}
		else
		{
			logger.LogInformation("Windows volume event drive was empty or already known");
		}

		var newDrives = DetectNewMounts(GetMountedVolumes());
		if ( newDrives.Count == 0 )
		{
			logger.LogInformation("Windows retry mount scan found no new drives");
		}

		foreach ( var drive in newDrives )
		{
			OnMountDetected(drive);
		}
	}

	/// <summary>
	///     Remove an ejected drive from the known-mounts baseline so that
	///     re-inserting the same drive is recognised as a new mount.
	/// </summary>
	internal bool HandleVolumeRemoval(string rawDriveName)
	{
		if ( string.IsNullOrWhiteSpace(rawDriveName) )
		{
			logger.LogInformation(
				"Windows volume removal event: drive name was empty, baseline unchanged");
			return false;
		}

		var normalized = NormalizeDrive(rawDriveName);
		var removed = _knownMountedVolumes.Remove(normalized);
		logger.LogInformation(
			removed
				? $"Windows volume removal event: removed '{normalized}' from baseline"
				: $"Windows volume removal event: '{normalized}' was not in baseline");

		return removed;
	}

	internal void SeedKnownMounts(IEnumerable<string> mountedVolumes)
	{
		_knownMountedVolumes.Clear();
		foreach ( var volume in mountedVolumes.Where(v => !string.IsNullOrWhiteSpace(v)) )
		{
			_knownMountedVolumes.Add(NormalizeDrive(volume));
		}
	}

	internal HashSet<string> DetectNewMounts(IEnumerable<string> mountedVolumes)
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

		return [.. newMounts];
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

	internal static string NormalizeDrive(string driveName)
	{
		var normalized = driveName.Trim();
		if ( normalized.Length < 2 || normalized[1] != ':' )
		{
			return normalized.Replace('/', '\\');
		}

		// Ensure single trailing backslash and use backslash separator
		// Keep the drive letter case as provided.
		// If input is just "E:" add a backslash. If it already has a backslash
		// (e.g. "E:\\" or "E:\") ensure it's reduced to a single trailing backslash.
		normalized = normalized.Replace('/', '\\');
		return normalized.Length switch
		{
			2 => normalized + "\\",
			>= 3 when normalized[2] == '\\' => normalized[..3],
			_ => normalized + "\\"
		};
	}
}

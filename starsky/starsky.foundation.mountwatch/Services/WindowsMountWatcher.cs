using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using starsky.foundation.mountwatch.Interfaces;

namespace starsky.foundation.mountwatch.Services;

/// <summary>
///     Windows mount watcher using WMI for event-driven drive detection
/// </summary>
public class WindowsMountWatcher : IMountWatcher
{
	private bool _isRunning;
	private ManagementEventWatcher? _watcher;

	public event EventHandler<MountDetectedEventArgs>? MountDetected;

	/// <summary>
	///     Start watching for mount events using WMI
	/// </summary>
	public void Start()
	{
		if ( _isRunning )
		{
			return;
		}

		_isRunning = true;

		try
		{
			// Monitor Win32_VolumeChangeEvent for drive connections (EventType = 2)
			var query =
				new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
			_watcher = new ManagementEventWatcher(query);
			_watcher.EventArrived += OnVolumeChanged;
			_watcher.Start();
		}
		catch
		{
			// Fallback to polling if WMI fails
			RunPollingFallback();
		}
	}

	/// <summary>
	///     Stop watching for mount events
	/// </summary>
	public void Stop()
	{
		_isRunning = false;

		try
		{
			_watcher?.Stop();
			_watcher?.Dispose();
		}
		catch
		{
			// Ignore cleanup errors
		}
	}

	/// <summary>
	///     Get currently mounted volumes
	/// </summary>
	public IEnumerable<string> GetMountedVolumes()
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
			return Enumerable.Empty<string>();
		}
	}

	/// <summary>
	///     Handle WMI volume change events
	/// </summary>
	private void OnVolumeChanged(object? sender, EventArrivedEventArgs e)
	{
		try
		{
			// Get the newly available drive
			var newDrives = GetMountedVolumes();
			if ( newDrives.Any() )
			{
				OnMountDetected(newDrives.First());
			}
		}
		catch
		{
			// Ignore errors
		}
	}

	/// <summary>
	///     Fallback polling implementation if WMI fails
	/// </summary>
	private void RunPollingFallback()
	{
		var previousDrives = new HashSet<string>(GetMountedVolumes());
		const int pollInterval = 2000;

		new Thread(() =>
		{
			while ( _isRunning )
			{
				try
				{
					Thread.Sleep(pollInterval);

					var currentDrives = GetMountedVolumes();
					var newDrives = currentDrives.Except(previousDrives).ToList();

					if ( newDrives.Count > 0 )
					{
						foreach ( var drive in newDrives )
						{
							previousDrives.Add(drive);
							OnMountDetected(drive);
						}
					}

					// Check for disconnected drives
					var removedDrives = previousDrives.Except(currentDrives).ToList();
					foreach ( var drive in removedDrives )
					{
						previousDrives.Remove(drive);
					}
				}
				catch
				{
					Thread.Sleep(pollInterval);
				}
			}
		}) { IsBackground = true }.Start();
	}

	/// <summary>
	///     Raise MountDetected event
	/// </summary>
	private void OnMountDetected(string drivePath)
	{
		MountDetected?.Invoke(this,
			new MountDetectedEventArgs { MountPath = drivePath, DetectedAt = DateTime.UtcNow });
	}
}

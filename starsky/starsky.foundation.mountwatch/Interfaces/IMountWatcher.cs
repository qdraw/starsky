using System;
using System.Collections.Generic;

namespace starsky.foundation.mountwatch.Interfaces;

/// <summary>
///     Event arguments for mount detection
/// </summary>
public class MountDetectedEventArgs : EventArgs
{
	public required string MountPath { get; set; }
	public required DateTime DetectedAt { get; set; }
}

/// <summary>
///     Abstraction for OS-specific mount watching
/// </summary>
public interface IMountWatcher
{
	/// <summary>
	///     Event fired when a new mount is detected
	/// </summary>
	event EventHandler<MountDetectedEventArgs>? MountDetected;

	/// <summary>
	///     Start watching for mount events
	/// </summary>
	void Start();

	/// <summary>
	///     Stop watching for mount events
	/// </summary>
	void Stop();

	/// <summary>
	///     Get currently mounted volumes
	/// </summary>
	/// <returns>List of mount paths</returns>
	IEnumerable<string> GetMountedVolumes();
}


using System;
using System.IO;

namespace starsky.foundation.connect.Sync;

/// <summary>
/// Notifies the sync engine about local file system changes.
/// Implemented by adapters that bridge to Starsky's existing <see cref="System.IO.FileSystemWatcher"/>
/// infrastructure or a standalone watcher.
/// </summary>
public interface ISyncLocalChangeSource
{
	event EventHandler<SyncLocalChangeEventArgs> LocalFileChanged;
}

public sealed class SyncLocalChangeEventArgs : EventArgs
{
	public required string FullPath { get; init; }

	/// <summary>Previous path, only set for rename events.</summary>
	public string? OldFullPath { get; init; }

	public required WatcherChangeTypes ChangeType { get; init; }
}

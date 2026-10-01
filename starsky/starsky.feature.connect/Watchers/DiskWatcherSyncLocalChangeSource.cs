using System;
using System.IO;
using starsky.foundation.connect.Sync;
using starsky.foundation.sync.WatcherInterfaces;

namespace starsky.feature.connect.Watchers;

/// <summary>
/// Adapts the existing Starsky <see cref="IFileSystemWatcherWrapper"/> singleton to
/// <see cref="ISyncLocalChangeSource"/>. Subscribes to the same watcher instance that
/// <c>DiskWatcher</c> uses, so no second <c>FileSystemWatcher</c> is created for the same path.
/// </summary>
public sealed class DiskWatcherSyncLocalChangeSource : ISyncLocalChangeSource, IDisposable
{
	private readonly IFileSystemWatcherWrapper _watcher;

	public event EventHandler<SyncLocalChangeEventArgs>? LocalFileChanged;

	public DiskWatcherSyncLocalChangeSource(IFileSystemWatcherWrapper watcher)
	{
		_watcher = watcher;

		_watcher.Created += OnCreated;
		_watcher.Changed += OnChanged;
		_watcher.Deleted += OnDeleted;
		_watcher.Renamed += OnRenamed;
	}

	private void OnCreated(object sender, FileSystemEventArgs e) =>
		RaiseChanged(e.FullPath, null, WatcherChangeTypes.Created);

	private void OnChanged(object sender, FileSystemEventArgs e) =>
		RaiseChanged(e.FullPath, null, WatcherChangeTypes.Changed);

	private void OnDeleted(object sender, FileSystemEventArgs e) =>
		RaiseChanged(e.FullPath, null, WatcherChangeTypes.Deleted);

	private void OnRenamed(object sender, RenamedEventArgs e) =>
		RaiseChanged(e.FullPath, e.OldFullPath, WatcherChangeTypes.Renamed);

	private void RaiseChanged(string fullPath, string? oldFullPath, WatcherChangeTypes changeType) =>
		LocalFileChanged?.Invoke(this, new SyncLocalChangeEventArgs
		{
			FullPath = fullPath,
			OldFullPath = oldFullPath,
			ChangeType = changeType,
		});

	public void Dispose()
	{
		_watcher.Created -= OnCreated;
		_watcher.Changed -= OnChanged;
		_watcher.Deleted -= OnDeleted;
		_watcher.Renamed -= OnRenamed;
	}
}

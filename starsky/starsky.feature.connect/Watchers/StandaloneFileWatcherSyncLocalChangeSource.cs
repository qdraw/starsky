using System;
using System.IO;
using starsky.foundation.connect.Sync;
using starsky.foundation.sync.WatcherServices;

namespace starsky.feature.connect.Watchers;

/// <summary>
/// Creates its own <see cref="BufferingFileSystemWatcher"/> for use when the Connect sync engine
/// runs outside the main Starsky web process (e.g. a standalone CLI).
/// </summary>
public sealed class StandaloneFileWatcherSyncLocalChangeSource : ISyncLocalChangeSource, IDisposable
{
	private readonly BufferingFileSystemWatcher _watcher;

	public event EventHandler<SyncLocalChangeEventArgs>? LocalFileChanged;

	public StandaloneFileWatcherSyncLocalChangeSource(string folderPath)
	{
		_watcher = new BufferingFileSystemWatcher(folderPath, "*")
		{
			IncludeSubdirectories = true,
			NotifyFilter = NotifyFilters.FileName
			               | NotifyFilters.DirectoryName
			               | NotifyFilters.Size
			               | NotifyFilters.LastWrite,
			InternalBufferSize = 64 * 1024,
		};

		_watcher.Created += OnCreated;
		_watcher.Changed += OnChanged;
		_watcher.Deleted += OnDeleted;
		_watcher.Renamed += OnRenamed;

		_watcher.EnableRaisingEvents = true;
	}

	private void OnCreated(object? sender, FileSystemEventArgs e) =>
		RaiseChanged(e.FullPath, null, WatcherChangeTypes.Created);

	private void OnChanged(object? sender, FileSystemEventArgs e) =>
		RaiseChanged(e.FullPath, null, WatcherChangeTypes.Changed);

	private void OnDeleted(object? sender, FileSystemEventArgs e) =>
		RaiseChanged(e.FullPath, null, WatcherChangeTypes.Deleted);

	private void OnRenamed(object? sender, RenamedEventArgs e) =>
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
		_watcher.Dispose();
	}
}

using System.IO;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherHelpers;
using starsky.foundation.sync.WatcherInterfaces;

namespace starsky.foundation.sync.WatcherServices
{
	/// <summary>
	/// Service is created only once, and used everywhere
	/// </summary>
	[Service(typeof(IDiskWatcher), InjectionLifetime = InjectionLifetime.Singleton)]
	public class DiskWatcher : IDiskWatcher
	{
		private readonly FileProcessor _fileProcessor;
		private readonly IFileSystemWatcherWrapper _fileSystemWatcherWrapper;

		public DiskWatcher(IFileSystemWatcherWrapper fileSystemWatcherWrapper,
			IServiceScopeFactory scopeFactory)
		{

			// File Processor has an endless loop
			_fileProcessor = new FileProcessor(new SyncWatcherPreflight(scopeFactory).Sync);
			_fileSystemWatcherWrapper = fileSystemWatcherWrapper;
		}

		/// <summary>
		/// @see: https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=netcore-3.1
		/// </summary>
		public void Watcher(string fullFilePath)
		{
			// Create a new FileSystemWatcher and set its properties.

			_fileSystemWatcherWrapper.Path = fullFilePath;
			_fileSystemWatcherWrapper.Filter = "*";
			_fileSystemWatcherWrapper.IncludeSubdirectories = true;
			_fileSystemWatcherWrapper.NotifyFilter =   NotifyFilters.LastAccess
			                                         | NotifyFilters.Size
			                                         | NotifyFilters.LastWrite
			                                         | NotifyFilters.FileName
			                                         | NotifyFilters.DirectoryName
			                                         | NotifyFilters.CreationTime;


			// Watch for changes in LastAccess and LastWrite times, and
			// the renaming of files or directories.

			// Add event handlers.
			_fileSystemWatcherWrapper.Changed += OnChanged;
			_fileSystemWatcherWrapper.Created += OnChanged;
			_fileSystemWatcherWrapper.Deleted += OnChanged;
			_fileSystemWatcherWrapper.Renamed += OnRenamed;

			// Begin watching.
			_fileSystemWatcherWrapper.EnableRaisingEvents = true;
		}
		
		// Define the event handlers.
		private void OnChanged(object source, FileSystemEventArgs e)
		{
			_fileProcessor.QueueInput(e.FullPath, e.ChangeType);
			// Specify what is done when a file is changed, created, or deleted.
			// e.FullPath e.ChangeType
		}

		private void OnRenamed(object source, RenamedEventArgs e)
		{
			_fileProcessor.QueueInput(e.OldFullPath, WatcherChangeTypes.Deleted);
			_fileProcessor.QueueInput(e.FullPath, WatcherChangeTypes.Created);
			// Specify what is done when a file is renamed. e.OldFullPath to e.FullPath
		}

	}
}

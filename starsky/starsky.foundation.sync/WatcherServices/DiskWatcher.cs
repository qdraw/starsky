using System.IO;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
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
		private readonly IWebLogger _webLogger;

		public DiskWatcher(IFileSystemWatcherWrapper fileSystemWatcherWrapper,
			IServiceScopeFactory scopeFactory)
		{

			// File Processor has an endless loop
			_fileProcessor = new FileProcessor(new SyncWatcherConnector(scopeFactory).Sync);
			_fileSystemWatcherWrapper = fileSystemWatcherWrapper;
			_webLogger = scopeFactory.CreateScope().ServiceProvider.GetService<IWebLogger>();
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
			_fileSystemWatcherWrapper.NotifyFilter = NotifyFilters.FileName
			                                         | NotifyFilters.DirectoryName
			                                         | NotifyFilters.Attributes
			                                         | NotifyFilters.Size
			                                         | NotifyFilters.LastWrite
			                                         // NotifyFilters.LastAccess is removed
			                                         | NotifyFilters.CreationTime
			                                         | NotifyFilters.Security;

			// Watch for changes in LastAccess and LastWrite times, and
			// the renaming of files or directories.

			// Add event handlers.
			_fileSystemWatcherWrapper.Created += OnChanged;
			_fileSystemWatcherWrapper.Changed += OnChanged;
			_fileSystemWatcherWrapper.Deleted += OnChanged;
			_fileSystemWatcherWrapper.Renamed += OnRenamed;
			_fileSystemWatcherWrapper.Error += OnError;
				
			// Begin watching.
			_fileSystemWatcherWrapper.EnableRaisingEvents = true;
		}
		
		private void OnError(object source, ErrorEventArgs e)
		{
			//  Show that an error has been detected.
			_webLogger.LogError("The FileSystemWatcher has detected an error");
			//  Give more information if the error is due to an internal buffer overflow.
			if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
			{
				//  This can happen if Windows is reporting many file system events quickly 
				//  and internal buffer of the  FileSystemWatcher is not large enough to handle this
				//  rate of events. The InternalBufferOverflowException error informs the application
				//  that some of the file system events are being lost.
				_webLogger.LogError(e.GetException(),"The file system watcher experienced an internal buffer overflow ");
			}
		}
		
		// Define the event handlers.
		private void OnChanged(object source, FileSystemEventArgs e)
		{
			_webLogger.LogInformation($"DiskWatcher {e.FullPath} OnChanged ChangeType is: {e.ChangeType}");
			_fileProcessor.QueueInput(e.FullPath, null, e.ChangeType);
			// Specify what is done when a file is changed, created, or deleted.
		}

		/// <summary>
		/// Specify what is done when a file is renamed. e.OldFullPath to e.FullPath
		/// </summary>
		/// <param name="source">object source (ignored)</param>
		/// <param name="e">arguments</param>
		private void OnRenamed(object source, RenamedEventArgs e)
		{
			_webLogger.LogInformation($"DiskWatcher {e.OldFullPath} OnRenamed to: {e.FullPath}");
			_fileProcessor.QueueInput(e.OldFullPath, e.FullPath, WatcherChangeTypes.Renamed);
		}

	}
}

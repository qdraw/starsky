using System;
using System.IO;
using starsky.foundation.sync.Interfaces;

namespace starsky.foundation.sync.Services
{
	public class DiskWatcher : IDiskWatcher
	{
		private readonly IFileProcessor _fileProcessor;
		private readonly IFileSystemWatcherWrapper _fileSystemWatcherWrapper;

		public DiskWatcher(IFileSystemWatcherWrapper fileSystemWatcherWrapper, IFileProcessor fileProcessor)
		{
			_fileProcessor = fileProcessor;
			_fileSystemWatcherWrapper = fileSystemWatcherWrapper;
		}

		/// <summary>
		/// @see: https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=netcore-3.1
		/// </summary>
		public void Watcher(string fullFilePath)
		{
			// Create a new FileSystemWatcher and set its properties.

			_fileSystemWatcherWrapper.Path = fullFilePath;
			_fileSystemWatcherWrapper.Filter = "*.txt";
			_fileSystemWatcherWrapper.IncludeSubdirectories = true;
			_fileSystemWatcherWrapper.NotifyFilter = NotifyFilters.LastAccess
			                                         | NotifyFilters.LastWrite
			                                         | NotifyFilters.FileName
			                                         | NotifyFilters.DirectoryName;


			// Watch for changes in LastAccess and LastWrite times, and
			// the renaming of files or directories.

			// Only watch text files.

			// Add event handlers.
			_fileSystemWatcherWrapper.Changed += OnChanged;
			_fileSystemWatcherWrapper.Created += OnChanged;
			_fileSystemWatcherWrapper.Deleted += OnChanged;
			_fileSystemWatcherWrapper.Renamed += OnRenamed;

			// Begin watching.
			_fileSystemWatcherWrapper.EnableRaisingEvents = true;

			// // Wait for the user to quit the program.
			// Console.WriteLine("Press 'q' to quit the sample.");
			// while (Console.Read() != 'q') ;
		}
		
		// Define the event handlers.
		private void OnChanged(object source, FileSystemEventArgs e)
		{
			_fileProcessor.QueueInput(e.FullPath);
			// Specify what is done when a file is changed, created, or deleted.
			Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
		}

		private void OnRenamed(object source, RenamedEventArgs e) =>
			// Specify what is done when a file is renamed.
			Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");
	}
}

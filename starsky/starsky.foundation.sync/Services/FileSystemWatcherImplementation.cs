using System;
using System.IO;

namespace starsky.foundation.sync.Services
{
	public class FileSystemWatcherImplementation
	{
		/// <summary>
		/// @see: https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher?view=netcore-3.1
		/// </summary>
		public void Test()
		{
			// Create a new FileSystemWatcher and set its properties.
			using FileSystemWatcher watcher = new FileSystemWatcher
			{
				Path = "args[1]",
				NotifyFilter = NotifyFilters.LastAccess
				               | NotifyFilters.LastWrite
				               | NotifyFilters.FileName
				               | NotifyFilters.DirectoryName,
				Filter = "*.txt"
			};

			// Watch for changes in LastAccess and LastWrite times, and
			// the renaming of files or directories.

			// Only watch text files.

			// Add event handlers.
			watcher.Changed += OnChanged;
			watcher.Created += OnChanged;
			watcher.Deleted += OnChanged;
			watcher.Renamed += OnRenamed;

			// Begin watching.
			watcher.EnableRaisingEvents = true;

			// Wait for the user to quit the program.
			Console.WriteLine("Press 'q' to quit the sample.");
			while (Console.Read() != 'q') ;
		}
		
		// Define the event handlers.
		private void OnChanged(object source, FileSystemEventArgs e) =>
			// Specify what is done when a file is changed, created, or deleted.
			Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

		private void OnRenamed(object source, RenamedEventArgs e) =>
			// Specify what is done when a file is renamed.
			Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");
	}
}

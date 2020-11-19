using System;
using System.IO;

namespace starsky.foundation.sync.WatcherInterfaces
{
	/// <summary>
	/// A wrapper around FileSystemWatcher
	/// @see: https://docs.microsoft.com/en-us/dotnet/api/system.io.filesystemwatcher.notifyfilter?view=netcore-3.1
	/// @see: https://stackoverflow.com/a/50948255/8613589
	/// </summary>
	public interface IFileSystemWatcherWrapper:  IDisposable
	{
		event FileSystemEventHandler Created;
		event FileSystemEventHandler Deleted;
		event RenamedEventHandler Renamed;
		event FileSystemEventHandler Changed;
		event ErrorEventHandler Error;

		bool EnableRaisingEvents { get; set; }

		bool IncludeSubdirectories { get; set; }

		string Path { get; set; }
		string Filter { get; set; }
		public NotifyFilters NotifyFilter { get; set; }
	}
}

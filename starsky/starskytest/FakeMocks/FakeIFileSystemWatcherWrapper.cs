using System;
using System.IO;
using starsky.foundation.sync.WatcherInterfaces;

namespace starskytest.FakeMocks
{
	public class FakeIFileSystemWatcherWrapper : IFileSystemWatcherWrapper
	{
		public event FileSystemEventHandler Created;
		public event FileSystemEventHandler Deleted;
		public event RenamedEventHandler Renamed;
		public event FileSystemEventHandler Changed;
		public bool EnableRaisingEvents { get; set; }
		public bool IncludeSubdirectories { get; set; }
		public string Path { get; set; }
		public string Filter { get; set; }
		public NotifyFilters NotifyFilter { get; set; }
		
		void IFileSystemWatcherWrapper.Dispose()
		{
		}

		void IDisposable.Dispose()
		{
		}
	}
}

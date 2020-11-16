using System;
using System.IO;
using starsky.foundation.sync.WatcherInterfaces;

namespace starskytest.FakeMocks
{
	public class FakeIFileSystemWatcherWrapper : IFileSystemWatcherWrapper
	{
		public void TriggerOnChanged(FileSystemEventArgs args)
		{
			Created?.Invoke(this, args);
		}
		
		public event FileSystemEventHandler Created;
		
		public void TriggerOnDeleted(RenamedEventArgs args)
		{
			Deleted?.Invoke(this, args);
		}
		
		public event FileSystemEventHandler Deleted;
		
		public void TriggerOnRename(RenamedEventArgs args)
		{
			Renamed?.Invoke(this, args);
		}
		
		public event RenamedEventHandler Renamed;
		
		public void TriggerOnChanged(RenamedEventArgs args)
		{
			Changed?.Invoke(this, args);
		}
		
		public event FileSystemEventHandler Changed;
		public bool EnableRaisingEvents { get; set; }
		public bool IncludeSubdirectories { get; set; }
		public string Path { get; set; }
		public string Filter { get; set; }
		public NotifyFilters NotifyFilter { get; set; }
		
		void IDisposable.Dispose()
		{
		}
	}
}

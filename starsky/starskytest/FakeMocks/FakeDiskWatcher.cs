using System.Collections.Generic;
using starsky.foundation.sync.WatcherInterfaces;

namespace starskytest.FakeMocks
{
	public class FakeDiskWatcher : IDiskWatcher
	{
		public List<string> AddedItems { get; set; } = new List<string>();
		public void Watcher(string fullFilePath)
		{
			AddedItems.Add(fullFilePath);
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using starsky.foundation.sync.WatcherInterfaces;

namespace starskytest.FakeMocks
{
	public class FakeIQueueProcessor : IQueueProcessor
	{
		public List<Tuple<string, string, WatcherChangeTypes>>
			Data { get; set; } =
			new();
		
		public void QueueInput(string filepath, string toPath, WatcherChangeTypes changeTypes)
		{
			Data.Add(new Tuple<string, string, WatcherChangeTypes>(filepath, toPath, changeTypes));
		}
	}
}

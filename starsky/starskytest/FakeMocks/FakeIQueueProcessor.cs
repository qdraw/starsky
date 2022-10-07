using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using starsky.foundation.sync.WatcherInterfaces;

namespace starskytest.FakeMocks
{
	public class FakeIQueueProcessor : IQueueProcessor
	{
		[SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeEvident")]
		public List<Tuple<string, string, WatcherChangeTypes>>
			Data { get; set; } = new List<Tuple<string, string, WatcherChangeTypes>>();
		
		public Task QueueInput(string filepath, string toPath,
			WatcherChangeTypes changeTypes)
		{
			Data.Add(new Tuple<string, string, WatcherChangeTypes>(filepath, toPath, changeTypes));
			return Task.CompletedTask;
		}
	}
}

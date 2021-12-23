using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.sync.WatcherHelpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherHelpers
{
	[TestClass]
	public class QueueProcessorTest
	{
		[TestMethod]
		public void QueueProcessorTest_QueueInput()
		{
			var diskWatcherBackgroundTaskQueue = new FakeDiskWatcherUpdateBackgroundTaskQueue();

			Task<List<FileIndexItem>> Local(Tuple<string, string, WatcherChangeTypes> value)
			{
				return Task.FromResult(new List<FileIndexItem>());
			}

			var memoryCache = new FakeMemoryCache();
			var queueProcessor = new QueueProcessor(diskWatcherBackgroundTaskQueue, Local, memoryCache, TimeSpan.FromSeconds(5));

			queueProcessor.QueueInput("t","T", WatcherChangeTypes.All);
			Assert.IsTrue(diskWatcherBackgroundTaskQueue.QueueBackgroundWorkItemCalled);
		}
		
		[TestMethod]
		public void QueueProcessorTest_QueueInput_Counter()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			var diskWatcherBackgroundTaskQueue = new FakeDiskWatcherUpdateBackgroundTaskQueue();

			Task<List<FileIndexItem>> Local(Tuple<string, string, WatcherChangeTypes> value)
			{
				return Task.FromResult(new List<FileIndexItem>());
			}
			var queueProcessor = new QueueProcessor(diskWatcherBackgroundTaskQueue, Local, memoryCache, TimeSpan.FromSeconds(5));
			
			// Run 3 times & 1 time different
			queueProcessor.QueueInput("t","T", WatcherChangeTypes.All);
			queueProcessor.QueueInput("t","T", WatcherChangeTypes.All);
			queueProcessor.QueueInput("t","T", WatcherChangeTypes.All);
			queueProcessor.QueueInput("1","T", WatcherChangeTypes.All);

			Assert.AreEqual(2, diskWatcherBackgroundTaskQueue.QueueBackgroundWorkItemCalledCounter);
		}
		
				
		[TestMethod]
		public async Task QueueProcessorTest_QueueInput_Counter_NoCache()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			var diskWatcherBackgroundTaskQueue = new FakeDiskWatcherUpdateBackgroundTaskQueue();

			Task<List<FileIndexItem>> Local(Tuple<string, string, WatcherChangeTypes> value)
			{
				return Task.FromResult(new List<FileIndexItem>());
			}
			var queueProcessor = new QueueProcessor(diskWatcherBackgroundTaskQueue, Local, memoryCache, TimeSpan.FromMilliseconds(1));
			
			// Run 3 times & 1 time different
			queueProcessor.QueueInput("t","T", WatcherChangeTypes.All);
			await Task.Delay(TimeSpan.FromMilliseconds(2)); // Sleep async
			queueProcessor.QueueInput("t","T", WatcherChangeTypes.All);

			Assert.AreEqual(2, diskWatcherBackgroundTaskQueue.QueueBackgroundWorkItemCalledCounter);
		}
	}
}

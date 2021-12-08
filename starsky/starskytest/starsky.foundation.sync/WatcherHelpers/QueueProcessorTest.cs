using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Extensions;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.sync.WatcherHelpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Services;
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
			new QueueProcessor(diskWatcherBackgroundTaskQueue, Local).QueueInput("t","T", WatcherChangeTypes.All);
			Assert.IsTrue(diskWatcherBackgroundTaskQueue.QueueBackgroundWorkItemCalled);
		}
	}
}

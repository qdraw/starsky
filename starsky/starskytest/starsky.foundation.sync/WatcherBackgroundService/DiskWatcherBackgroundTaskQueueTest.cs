using System.Threading;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.sync.WatcherBackgroundService;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherBackgroundService
{
	[TestClass]
	public class DiskWatcherBackgroundTaskQueueTest
	{
		[TestMethod]
		public void Test01()
		{
			var queue = new DiskWatcherBackgroundTaskQueue();
#pragma warning disable 1998
			queue.QueueBackgroundWorkItem(async token =>
#pragma warning restore 1998
			{
				
			});
			var token = new CancellationToken();
			queue.DequeueAsync(token);
			Assert.IsNotNull(token);

		}

		[TestMethod]
		public void AppInsights_InjectClient()
		{
			var taskQueue = new DiskWatcherBackgroundTaskQueue(
				new TelemetryClient(new TelemetryConfiguration()));
			var result = taskQueue.TrackQueue();
			Assert.IsTrue(result);
		}
		
		[TestMethod]
		public void AppInsights_DoNotInjectClient()
		{
			var taskQueue = new DiskWatcherBackgroundTaskQueue(
				new TelemetryClient(new TelemetryConfiguration()));
			var result = taskQueue.TrackQueue();
			Assert.IsFalse(result);
		}
	}
}

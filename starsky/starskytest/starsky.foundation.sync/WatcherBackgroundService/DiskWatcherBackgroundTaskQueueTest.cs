using System.Threading;
using System.Threading.Tasks;
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
#pragma warning disable CS1998
			queue.QueueBackgroundWorkItemAsync(async _ =>
#pragma warning restore CS1998
			{
				
			}, string.Empty);
			var token = new CancellationToken();
#pragma warning disable CS4014
			queue!.DequeueAsync(token);
#pragma warning restore CS4014
			Assert.IsNotNull(token);

		}
		
		[TestMethod]
		public async Task Count_AddOneForCount()
		{
			var backgroundQueue = new DiskWatcherBackgroundTaskQueue();
			await backgroundQueue!.QueueBackgroundWorkItemAsync(_ => ValueTask.CompletedTask, string.Empty);
			var count = backgroundQueue.Count();
			Assert.AreEqual(1,count);
		}
		

		// [TestMethod]
		// public void AppInsights_InjectClient()
		// {
		// 	var taskQueue = new DiskWatcherBackgroundTaskQueue(
		// 		new TelemetryClient(new TelemetryConfiguration()));
		// 	var result = taskQueue.TrackQueue();
		// 	Assert.IsTrue(result);
		// }
		//
		// [TestMethod]
		// public void AppInsights_DoNotInjectClient()
		// {
		// 	var taskQueue = new DiskWatcherBackgroundTaskQueue();
		// 	var result = taskQueue.TrackQueue();
		// 	Assert.IsFalse(result);
		// }
	}
}

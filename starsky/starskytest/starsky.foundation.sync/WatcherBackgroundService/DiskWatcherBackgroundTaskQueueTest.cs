using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.sync.WatcherBackgroundService;

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
	}
}

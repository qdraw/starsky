using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.sync.WatcherBackgroundService;

namespace starskytest.starsky.foundation.sync.WatcherBackgroundService
{
	[TestClass]
	public sealed class DiskWatcherBackgroundTaskQueueTest
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public DiskWatcherBackgroundTaskQueueTest()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public void Test01()
		{
			var queue = new DiskWatcherBackgroundTaskQueue(_scopeFactory);
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
			var backgroundQueue = new DiskWatcherBackgroundTaskQueue(_scopeFactory);
			await backgroundQueue!.QueueBackgroundWorkItemAsync(_ => ValueTask.CompletedTask, string.Empty);
			var count = backgroundQueue.Count();
			Assert.AreEqual(1,count);
		}
	}
}

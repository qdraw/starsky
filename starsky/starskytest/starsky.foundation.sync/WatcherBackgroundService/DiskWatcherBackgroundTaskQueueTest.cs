using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.sync.Metrics;
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
			services.AddSingleton<DiskWatcherBackgroundTaskQueueMetrics>();
			var serviceProvider = services.BuildServiceProvider();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task QueueBackgroundWorkItemAsync_DequeueAsync()
		{
			var queue = new DiskWatcherBackgroundTaskQueue(_scopeFactory);
			await queue.QueueBackgroundWorkItemAsync(_ => 
				ValueTask.CompletedTask, 
				string.Empty);
			
			Assert.AreEqual(1,queue.Count());

			var token = new CancellationToken();
			await queue!.DequeueAsync(token);
			
			Assert.AreEqual(0,queue.Count());
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

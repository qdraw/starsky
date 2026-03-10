using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.sync.Metrics;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.worker.Helpers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherBackgroundService
{
	[TestClass]
	public sealed class DiskWatcherBackgroundTaskQueueTest
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public DiskWatcherBackgroundTaskQueueTest()
		{
			var services = new ServiceCollection();
			services.AddSingleton<IMeterFactory, FakeIMeterFactory>();
			services.AddSingleton<DiskWatcherBackgroundTaskQueueMetrics>();
			var serviceProvider = services.BuildServiceProvider();
			_scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		[TestMethod]
		public async Task QueueBackgroundWorkItemAsync_DequeueAsync()
		{
			var queue = new DiskWatcherBackgroundTaskQueue(_scopeFactory);
			await queue.QueueJobAsync(InMemoryBackgroundJobCallbackRegistry.Register(
				_ => ValueTask.CompletedTask,
				string.Empty,
				null,
				ProcessTaskQueue.PriorityLaneDiskWatcher,
				nameof(IDiskWatcherBackgroundTaskQueue)));

			Assert.AreEqual(1, queue.Count());

			var token = new CancellationToken();
			await queue.DequeueJobAsync(token);

			Assert.AreEqual(0, queue.Count());
		}

		[TestMethod]
		public async Task Count_AddOneForCount()
		{
			var backgroundQueue = new DiskWatcherBackgroundTaskQueue(_scopeFactory);
			await backgroundQueue.QueueJobAsync(InMemoryBackgroundJobCallbackRegistry.Register(
				_ => ValueTask.CompletedTask,
				string.Empty,
				null,
				ProcessTaskQueue.PriorityLaneDiskWatcher,
				nameof(IDiskWatcherBackgroundTaskQueue)));
			var count = backgroundQueue.Count();
			Assert.AreEqual(1, count);
		}
	}
}

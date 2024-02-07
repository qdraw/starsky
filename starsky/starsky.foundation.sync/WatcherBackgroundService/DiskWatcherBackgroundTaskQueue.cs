using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.sync.Metrics;
using starsky.foundation.worker.Helpers;

namespace starsky.foundation.sync.WatcherBackgroundService
{
	/// <summary>
	/// @see: https://learn.microsoft.com/en-us/dotnet/core/extensions/queue-service
	/// </summary>
	[Service(typeof(IDiskWatcherBackgroundTaskQueue),
		InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class DiskWatcherBackgroundTaskQueue : IDiskWatcherBackgroundTaskQueue
	{
		private readonly Channel<Tuple<Func<CancellationToken, ValueTask>, string>> _queue;
		private readonly DiskWatcherBackgroundTaskQueueMetrics _metrics;

		public DiskWatcherBackgroundTaskQueue(IServiceScopeFactory scopeFactory)
		{
			_queue = Channel.CreateBounded<Tuple<Func<CancellationToken, ValueTask>, string>>(
				ProcessTaskQueue.DefaultBoundedChannelOptions);
			_metrics = scopeFactory.CreateScope().ServiceProvider
				.GetRequiredService<DiskWatcherBackgroundTaskQueueMetrics>();
		}

		public int Count()
		{
			return _queue.Reader.Count;
		}

		public ValueTask QueueBackgroundWorkItemAsync(
			Func<CancellationToken, ValueTask> workItem, string metaData)
		{
			_metrics.Value = Count();
			return ProcessTaskQueue.QueueBackgroundWorkItemAsync(_queue, workItem, metaData);
		}

		public async ValueTask<Tuple<Func<CancellationToken, ValueTask>, string>> DequeueAsync(
			CancellationToken cancellationToken)
		{
			var workItem =
				await _queue.Reader.ReadAsync(cancellationToken);

			_metrics.Value = Count();
			return workItem;
		}
	}
}

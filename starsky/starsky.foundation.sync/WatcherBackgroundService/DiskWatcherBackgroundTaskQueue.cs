using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using starsky.foundation.injection;
using starsky.foundation.worker.Helpers;

namespace starsky.foundation.sync.WatcherBackgroundService
{
	/// <summary>
	/// @see: microsoft docs
	/// </summary>
	[Service(typeof(IDiskWatcherBackgroundTaskQueue), InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class DiskWatcherBackgroundTaskQueue : IDiskWatcherBackgroundTaskQueue
	{
		private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

		public DiskWatcherBackgroundTaskQueue()
		{
			BoundedChannelOptions options = new(int.MaxValue)
			{
				FullMode = BoundedChannelFullMode.Wait
			};
			_queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
		}

		public async ValueTask QueueBackgroundWorkItemAsync(
			Func<CancellationToken, ValueTask> workItem)
		{
			if (workItem is null)
			{
				throw new ArgumentNullException(nameof(workItem));
			}
			await _queue.Writer.WriteAsync(workItem);
		}

		public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
			CancellationToken cancellationToken)
		{
			var workItem =
				await _queue.Reader.ReadAsync(cancellationToken);
			return workItem;
		}
	}
}

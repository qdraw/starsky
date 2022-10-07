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
		private readonly Channel<Tuple<Func<CancellationToken, ValueTask>, string>> _queue;

		public DiskWatcherBackgroundTaskQueue()
		{
			BoundedChannelOptions options = new(int.MaxValue)
			{
				FullMode = BoundedChannelFullMode.Wait
			};
			_queue = Channel.CreateBounded<Tuple<Func<CancellationToken, ValueTask>, string>>(options);
		}

		public int Count()
		{
			return _queue.Reader.Count;
		}

		public ValueTask QueueBackgroundWorkItemAsync(
			Func<CancellationToken, ValueTask> workItem, string metaData)
		{
			if (workItem is null)
			{
				throw new ArgumentNullException(nameof(workItem));
			}

			return QueueBackgroundWorkItemInternalAsync(workItem, metaData);
		}

		private async ValueTask QueueBackgroundWorkItemInternalAsync(
			Func<CancellationToken, ValueTask> workItem, string metaData)
		{
			await _queue.Writer.WriteAsync(new Tuple<Func<CancellationToken, ValueTask>, string>(workItem,metaData));
		}

		public async ValueTask<Tuple<Func<CancellationToken, ValueTask>, string>> DequeueAsync(
			CancellationToken cancellationToken)
		{
			var workItem =
				await _queue.Reader.ReadAsync(cancellationToken);
			return workItem;
		}
	}
}

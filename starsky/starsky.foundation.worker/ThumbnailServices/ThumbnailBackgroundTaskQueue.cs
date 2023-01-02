using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

namespace starsky.foundation.worker.ThumbnailServices
{
	/// <summary>
	/// @see: https://learn.microsoft.com/en-us/dotnet/core/extensions/queue-service
	/// </summary>
	[Service(typeof(IThumbnailQueuedHostedService), InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class ThumbnailBackgroundTaskQueue : IThumbnailQueuedHostedService
	{
		private readonly Channel<Tuple<Func<CancellationToken, ValueTask>, string>> _queue;

		public ThumbnailBackgroundTaskQueue()
		{
			_queue = Channel.CreateBounded<Tuple<Func<CancellationToken, ValueTask>, string>>(ProcessTaskQueue.DefaultBoundedChannelOptions);
		}

		public int Count()
		{
			return _queue.Reader.Count;
		}

		public ValueTask QueueBackgroundWorkItemAsync(
			Func<CancellationToken, ValueTask> workItem, string metaData)
		{
			return ProcessTaskQueue.QueueBackgroundWorkItemAsync(_queue, workItem, metaData);
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

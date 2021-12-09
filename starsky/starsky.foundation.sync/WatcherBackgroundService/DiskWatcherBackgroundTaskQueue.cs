using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.injection;
using starsky.foundation.worker.Helpers;

namespace starsky.foundation.sync.WatcherBackgroundService
{
	/// <summary>
	/// @see: https://www.c-sharpcorner.com/article/how-to-call-background-service-from-net-core-web-api/
	/// </summary>
	[Service(typeof(IDiskWatcherBackgroundTaskQueue), InjectionLifetime = InjectionLifetime.Singleton)]
	public sealed class DiskWatcherBackgroundTaskQueue : IDiskWatcherBackgroundTaskQueue
	{
		private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = 
			new ConcurrentQueue<Func<CancellationToken, Task>>();
		private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

		public void QueueBackgroundWorkItem(
			Func<CancellationToken, Task> workItem)
		{
			BaseBackgroundTaskQueue.QueueBackgroundWorkItem(workItem, _workItems, _signal);
		}

		public async Task<Func<CancellationToken, Task>> DequeueAsync(
			CancellationToken cancellationToken)
		{
			return await BaseBackgroundTaskQueue.DequeueAsync(cancellationToken,
				_workItems, _signal);
		}
	}
}

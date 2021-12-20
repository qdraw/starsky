using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.worker.Helpers
{
	public static class BaseBackgroundTaskQueue
	{
		public static void QueueBackgroundWorkItem(Func<CancellationToken, 
			Task> workItem, 
			ConcurrentQueue<Func<CancellationToken, Task>> workItems, 
			SemaphoreSlim signal)
		{
			if (workItem == null)
			{
				throw new ArgumentNullException(nameof(workItem));
			}
			workItems.Enqueue(workItem);
			signal.Release();
		}

		public static async Task<Func<CancellationToken, Task>> DequeueAsync(
			CancellationToken cancellationToken,
			ConcurrentQueue<Func<CancellationToken, Task>> workItems,
			SemaphoreSlim signal)
		{
			await signal.WaitAsync(cancellationToken);
			workItems.TryDequeue(out var workItem);

			return workItem;
		}
	}
}

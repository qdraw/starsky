using System;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.worker.Interfaces
{
	public interface IBaseBackgroundTaskQueue
	{
		ValueTask QueueBackgroundWorkItemAsync(
			Func<CancellationToken, ValueTask> workItem);
		
		ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
			CancellationToken cancellationToken);
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.worker.Interfaces
{
	public interface IBaseBackgroundTaskQueue
	{
		void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

		Task<Func<CancellationToken, Task>> DequeueAsync(
			CancellationToken cancellationToken);
	}
}

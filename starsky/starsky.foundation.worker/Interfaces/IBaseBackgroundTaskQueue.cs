using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.Models;

namespace starsky.foundation.worker.Interfaces;

public interface IBaseBackgroundTaskQueue
{
	public int Count();

	ValueTask QueueJobAsync(BackgroundTaskQueueJob job);
	ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(CancellationToken cancellationToken);
}

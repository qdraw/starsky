using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.Models;
using starsky.foundation.worker.ThumbnailServices.Exceptions;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

namespace starskytest.FakeMocks;

public class FakeThumbnailBackgroundTaskQueue(bool cpuOverload = false)
	: IThumbnailQueuedHostedService
{
	public int QueueBackgroundWorkItemCalledCounter { get; set; }

	public bool QueueBackgroundWorkItemCalled { get; set; }

	public int Count()
	{
		return QueueBackgroundWorkItemCalledCounter;
	}

	public async ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		await Task.Yield();
		QueueBackgroundWorkItemCalled = true;
		QueueBackgroundWorkItemCalledCounter++;
	}

	public ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(
		CancellationToken cancellationToken)
	{
		return ValueTask.FromResult(new BackgroundTaskQueueJob
		{
			JobType = "Fake.Noop",
			PayloadJson = "{}",
			MetaData = string.Empty,
			TraceParentId = string.Empty
		});
	}

	public bool ThrowExceptionIfCpuUsageIsToHigh(string metaData)
	{
		if ( cpuOverload )
		{
			throw new ToManyUsageException($"CPU is to high, skip thumbnail generation {metaData}");
		}

		return true;
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.Helpers;
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
		await InMemoryBackgroundJobCallbackRegistry.TryExecuteAsync(job, CancellationToken.None);
		QueueBackgroundWorkItemCalled = true;
		QueueBackgroundWorkItemCalledCounter++;
	}

	public ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(
		CancellationToken cancellationToken)
	{
		var job = InMemoryBackgroundJobCallbackRegistry.Register(
			_ => ValueTask.CompletedTask,
			string.Empty,
			string.Empty,
			ProcessTaskQueue.PriorityLaneThumbnail,
			nameof(FakeThumbnailBackgroundTaskQueue));
		return ValueTask.FromResult(job);
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

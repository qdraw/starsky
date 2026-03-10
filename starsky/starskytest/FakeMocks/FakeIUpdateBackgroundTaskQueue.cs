using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starskytest.FakeMocks;

public class FakeIUpdateBackgroundTaskQueue : IUpdateBackgroundTaskQueue
{
	public int QueueBackgroundWorkItemCalledCounter { get; set; }

	public bool QueueBackgroundWorkItemCalled { get; set; }

	public int Count()
	{
		return 0;
	}

	public async ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		await InMemoryBackgroundJobCallbackRegistry.TryExecuteAsync(job, CancellationToken.None);
		QueueBackgroundWorkItemCalled = true;
		QueueBackgroundWorkItemCalledCounter++;
	}

	public ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(CancellationToken cancellationToken)
	{
		var job = InMemoryBackgroundJobCallbackRegistry.Register(
			_ => ValueTask.CompletedTask,
			string.Empty,
			string.Empty,
			ProcessTaskQueue.PriorityLaneUpdate,
			nameof(FakeIUpdateBackgroundTaskQueue));
		return ValueTask.FromResult(job);
	}
}

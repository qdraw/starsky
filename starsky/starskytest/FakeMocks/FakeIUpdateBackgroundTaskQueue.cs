using System.Threading;
using System.Threading.Tasks;
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
		await Task.Yield();
		QueueBackgroundWorkItemCalled = true;
		QueueBackgroundWorkItemCalledCounter++;
	}

	public ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(CancellationToken cancellationToken)
	{
		return ValueTask.FromResult(new BackgroundTaskQueueJob
		{
			JobType = "Fake.Noop",
			PayloadJson = "{}",
			MetaData = string.Empty,
			TraceParentId = string.Empty
		});
	}
}

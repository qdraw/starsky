using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Models;

namespace starskytest.FakeMocks;

/// <summary>
///     @see: FakeIBackgroundTaskQueue
/// </summary>
public class FakeDiskWatcherUpdateBackgroundTaskQueue : IDiskWatcherBackgroundTaskQueue
{
	private int _count;

	public FakeDiskWatcherUpdateBackgroundTaskQueue(int count = 0)
	{
		_count = count;
	}

	public bool QueueBackgroundWorkItemCalled { get; set; }
	public int QueueBackgroundWorkItemCalledCounter { get; set; }
	public int DequeueAsyncCounter { get; set; }

	public int Count()
	{
		return _count;
	}

	public async ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		QueueBackgroundWorkItemCalled = true;
		QueueBackgroundWorkItemCalledCounter++;
		await InMemoryBackgroundJobCallbackRegistry.TryExecuteAsync(job, CancellationToken.None);
	}

	public async ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(CancellationToken cancellationToken)
	{
		_count--;
		DequeueAsyncCounter++;
		await Task.Yield();
		return InMemoryBackgroundJobCallbackRegistry.Register(
			_ => ValueTask.CompletedTask,
			string.Empty,
			string.Empty,
			ProcessTaskQueue.PriorityLaneDiskWatcher,
			nameof(FakeDiskWatcherUpdateBackgroundTaskQueue));
	}
}

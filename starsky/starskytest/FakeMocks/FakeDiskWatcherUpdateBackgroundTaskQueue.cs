using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starskytest.FakeMocks;

/// <summary>
///     @see: FakeIBackgroundTaskQueue
/// </summary>
public class FakeDiskWatcherUpdateBackgroundTaskQueue : IDiskWatcherBackgroundTaskQueue
{
	private int _count;
	private readonly IServiceScopeFactory? _scopeFactory;


	public FakeDiskWatcherUpdateBackgroundTaskQueue(IServiceScopeFactory? scopeFactory = null,
		int count = 0)
	{
		_scopeFactory = scopeFactory;
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
		await Task.Yield();

		// If a scope factory is provided, attempt to resolve a matching IBackgroundJobHandler and execute immediately
		if ( _scopeFactory != null )
		{
			using var scope = _scopeFactory.CreateScope();
			var handlers = scope.ServiceProvider.GetServices<IBackgroundJobHandler>();
			foreach ( var handler in handlers )
			{
				if ( handler.JobType != job.JobType )
				{
					continue;
				}

				// execute and don't await exceptions here; bubble up if desired
				await handler.ExecuteAsync(job.PayloadJson, CancellationToken.None);
				break;
			}
		}
	}

	public async ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(
		CancellationToken cancellationToken)
	{
		_count--;
		DequeueAsyncCounter++;
		await Task.Yield();
		return new BackgroundTaskQueueJob
		{
			JobType = "Fake.Noop",
			PayloadJson = "{}",
			MetaData = string.Empty,
			TraceParentId = string.Empty
		};
	}
}

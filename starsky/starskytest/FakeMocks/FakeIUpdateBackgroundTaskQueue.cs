using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starskytest.FakeMocks;

public class FakeIUpdateBackgroundTaskQueue : IUpdateBackgroundTaskQueue
{
	private readonly IServiceScopeFactory? _scopeFactory;

	public FakeIUpdateBackgroundTaskQueue()
	{
	}

	public FakeIUpdateBackgroundTaskQueue(IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

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

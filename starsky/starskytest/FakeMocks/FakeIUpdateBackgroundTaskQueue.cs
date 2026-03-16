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

	// Expose the last queued job for inspection in tests
	public BackgroundTaskQueueJob? LastQueuedJob { get; set; }

	// Expose the last background execution task so tests can await completion if desired
	public Task? LastExecutionTask { get; set; }

	public int Count()
	{
		return 0;
	}

	public async ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		await Task.Yield();
		QueueBackgroundWorkItemCalled = true;
		QueueBackgroundWorkItemCalledCounter++;

		// store job for tests
		LastQueuedJob = job;

		// Debug: print payload to help trace JSON issues in tests
		System.Console.WriteLine($"[FakeQueue] Queued PayloadJson: {job.PayloadJson}");

		// If a scope factory is provided, attempt to resolve a matching IBackgroundJobHandler and execute synchronously
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

				// Execute and await so tests can assert immediately after QueueJobAsync completes
				LastExecutionTask = handler.ExecuteAsync(job.PayloadJson, CancellationToken.None);
				await LastExecutionTask;
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

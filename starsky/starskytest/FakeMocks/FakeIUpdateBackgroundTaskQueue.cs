using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starskytest.FakeMocks;

/// <summary>
/// Test double for <see cref="IUpdateBackgroundTaskQueue"/>.
///
/// Important: when constructed with an <see cref="IServiceScopeFactory"/>, this fake
/// resolves the matching <see cref="IBackgroundJobHandler"/> and executes it synchronously
/// (awaits <c>ExecuteAsync</c>) before returning from <c>QueueJobAsync</c>.
///
/// Rationale: many unit tests in this codebase expect background work (DB updates,
/// filesystem actions, websocket notifications) to be completed immediately after the
/// controller or service method returns. Running the handler synchronously avoids
/// race conditions where assertions run before the background handler finished.
///
/// How to change behavior: if you want to simulate true asynchronous/background
/// execution, modify the implementation in <c>QueueJobAsync</c> to call
/// <c>Task.Run(() =&gt; handler.ExecuteAsync(...))</c> (or otherwise start the work
/// without awaiting). To keep deterministic tests, assign that Task to
/// <c>LastExecutionTask</c> and have tests await it explicitly when needed.
/// </summary>
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

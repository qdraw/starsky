using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.worker.Backends;
using starsky.foundation.injection;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Metrics;
using starsky.foundation.worker.Models;

namespace starsky.foundation.worker.Services;

/// <summary>
///     @see: https://learn.microsoft.com/en-us/dotnet/core/extensions/queue-service
/// </summary>
[Service(typeof(IUpdateBackgroundTaskQueue), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class UpdateBackgroundTaskQueue : IUpdateBackgroundTaskQueue
{
	public const string QueueName = QueueNames.Update;

	private readonly IBaseBackgroundTaskQueue _backend;
	private readonly UpdateBackgroundQueuedMetrics _metrics;

	public UpdateBackgroundTaskQueue(IServiceScopeFactory scopeFactory,
		IQueueBackendFactory? queueBackendFactory = null)
	{
		_backend = queueBackendFactory?.Create(QueueName) ?? new InMemoryQueueBackend();

		_metrics = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<UpdateBackgroundQueuedMetrics>();
	}

	public int Count()
	{
		return _backend.Count();
	}

	public ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		return _backend.QueueJobAsync(job);
	}

	public async ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(
		CancellationToken cancellationToken)
	{
		var queueItem = await _backend.DequeueJobAsync(cancellationToken);
		_metrics.Value = Count();
		return queueItem;
	}
}

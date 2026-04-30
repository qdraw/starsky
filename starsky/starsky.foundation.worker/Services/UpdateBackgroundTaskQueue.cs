using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.worker.Backends;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Helpers;
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
	private readonly IServiceScopeFactory _scopeFactory;

	public UpdateBackgroundTaskQueue(IServiceScopeFactory scopeFactory,
		IQueueBackendFactory? queueBackendFactory = null)
	{
		_scopeFactory = scopeFactory;
		_backend = queueBackendFactory?.Create(QueueName) ?? new InMemoryQueueBackend();

		_metrics = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<UpdateBackgroundQueuedMetrics>();
	}

	public int Count()
	{
		return _backend.Count();
	}

	public async ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		ArgumentNullException.ThrowIfNull(job);
		if ( string.IsNullOrWhiteSpace(job.JobType) )
		{
			throw new ArgumentException("JobType is required", nameof(job));
		}

		using var scope = _scopeFactory.CreateScope();
		var logger = scope.ServiceProvider.GetService<IWebLogger>();
		var queuedJobs = await QueueJobTenantEnforcer.ExpandForTenantCoverageAsync(job,
			scope.ServiceProvider, logger, QueueName);
		foreach ( var queuedJob in queuedJobs )
		{
			await _backend.QueueJobAsync(queuedJob);
		}
	}

	public async ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(
		CancellationToken cancellationToken)
	{
		var queueItem = await _backend.DequeueJobAsync(cancellationToken);
		_metrics.Value = Count();
		return queueItem;
	}
}

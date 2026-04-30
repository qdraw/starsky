using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.worker.Backends;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.worker.Helpers;
using starsky.foundation.sync.Metrics;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starsky.foundation.sync.WatcherBackgroundService;

/// <summary>
///     @see: https://learn.microsoft.com/en-us/dotnet/core/extensions/queue-service
/// </summary>
[Service(typeof(IDiskWatcherBackgroundTaskQueue),
	InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class DiskWatcherBackgroundTaskQueue : IDiskWatcherBackgroundTaskQueue
{
	public const string QueueName = QueueNames.DiskWatcher;

	private readonly IBaseBackgroundTaskQueue _backend;
	private readonly DiskWatcherBackgroundTaskQueueMetrics _metrics;
	private readonly IServiceScopeFactory _scopeFactory;

	public DiskWatcherBackgroundTaskQueue(IServiceScopeFactory scopeFactory,
		IQueueBackendFactory? queueBackendFactory = null)
	{
		_scopeFactory = scopeFactory;
		_backend = queueBackendFactory?.Create(QueueName) ?? new InMemoryQueueBackend();
		_metrics = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<DiskWatcherBackgroundTaskQueueMetrics>();
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
		var workItem = await _backend.DequeueJobAsync(cancellationToken);
		_metrics.Value = Count();
		return workItem;
	}
}

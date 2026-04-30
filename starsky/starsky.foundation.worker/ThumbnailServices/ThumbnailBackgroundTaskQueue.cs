using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.worker.Backends;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.CpuEventListener.Interfaces;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Metrics;
using starsky.foundation.worker.Models;
using starsky.foundation.worker.ThumbnailServices.Exceptions;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

namespace starsky.foundation.worker.ThumbnailServices;

/// <summary>
///     @see: https://learn.microsoft.com/en-us/dotnet/core/extensions/queue-service
/// </summary>
[Service(typeof(IThumbnailQueuedHostedService),
	InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class ThumbnailBackgroundTaskQueue : IThumbnailQueuedHostedService
{
	public const string QueueName = QueueNames.Thumbnail;

	private readonly AppSettings _appSettings;
	private readonly IBaseBackgroundTaskQueue _backend;
	private readonly ICpuUsageListener _cpuUsageListenerService;
	private readonly IWebLogger _logger;
	private readonly IServiceScopeFactory _scopeFactory;

	private readonly ThumbnailBackgroundQueuedMetrics _metrics;

	public ThumbnailBackgroundTaskQueue(ICpuUsageListener cpuUsageListenerService,
		IWebLogger logger, AppSettings appSettings, IServiceScopeFactory scopeFactory,
		IQueueBackendFactory? queueBackendFactory = null)
	{
		_cpuUsageListenerService = cpuUsageListenerService;
		_logger = logger;
		_scopeFactory = scopeFactory;
		_appSettings = appSettings;
		_backend = queueBackendFactory?.Create(QueueName) ?? new InMemoryQueueBackend();
		_metrics = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<ThumbnailBackgroundQueuedMetrics>();
	}

	public int Count()
	{
		return _backend.Count();
	}

	public bool ThrowExceptionIfCpuUsageIsToHigh(string? metaData)
	{
		if ( _cpuUsageListenerService.CpuUsageMean <= _appSettings.CpuUsageMaxPercentage )
		{
			return true;
		}

		_logger.LogError("[QueueBackgroundWorkItemAsync]" +
		                 $"Skip {metaData} because of high CPU usage");
		throw new ToManyUsageException($"QueueBackgroundWorkItemAsync: " +
		                               $"Skip {metaData} because of high CPU usage");
	}

	public ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		ArgumentNullException.ThrowIfNull(job);
		if ( string.IsNullOrWhiteSpace(job.JobType) )
		{
			throw new ArgumentException("JobType is required", nameof(job));
		}

		return QueueJobInternalAsync(job);
	}

	private async ValueTask QueueJobInternalAsync(BackgroundTaskQueueJob job)
	{
		using var scope = _scopeFactory.CreateScope();
		var queuedJobs = await QueueJobTenantEnforcer.ExpandForTenantCoverageAsync(job,
			scope.ServiceProvider, _logger, QueueName);
		foreach ( var queuedJob in queuedJobs )
		{
			ThrowExceptionIfCpuUsageIsToHigh(queuedJob.MetaData);
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

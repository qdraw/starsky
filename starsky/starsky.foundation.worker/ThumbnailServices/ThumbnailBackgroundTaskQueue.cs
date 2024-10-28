using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.CpuEventListener.Interfaces;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Metrics;
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
	private readonly AppSettings _appSettings;
	private readonly ICpuUsageListener _cpuUsageListenerService;
	private readonly IWebLogger _logger;

	private readonly ThumbnailBackgroundQueuedMetrics _metrics;

	private readonly Channel<Tuple<Func<CancellationToken, ValueTask>, string?, string?>>
		_queue;

	public ThumbnailBackgroundTaskQueue(ICpuUsageListener cpuUsageListenerService,
		IWebLogger logger, AppSettings appSettings, IServiceScopeFactory scopeFactory)
	{
		_cpuUsageListenerService = cpuUsageListenerService;
		_logger = logger;
		_appSettings = appSettings;
		_queue = Channel
			.CreateBounded<Tuple<Func<CancellationToken, ValueTask>, string?, string?>>(
				ProcessTaskQueue.DefaultBoundedChannelOptions);
		_metrics = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<ThumbnailBackgroundQueuedMetrics>();
	}

	public int Count()
	{
		return _queue.Reader.Count;
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

	[SuppressMessage("ReSharper", "InvertIf")]
	public ValueTask QueueBackgroundWorkItemAsync(
		Func<CancellationToken, ValueTask> workItem, string? metaData = null,
		string? traceParentId = null)
	{
		ThrowExceptionIfCpuUsageIsToHigh(metaData);
		return ProcessTaskQueue.QueueBackgroundWorkItemAsync(_queue,
			workItem, metaData, traceParentId);
	}

	public async ValueTask<Tuple<Func<CancellationToken, ValueTask>, string?, string?>>
		DequeueAsync(
			CancellationToken cancellationToken)
	{
		var workItem =
			await _queue.Reader.ReadAsync(cancellationToken);
		_metrics.Value = Count();
		return workItem;
	}
}

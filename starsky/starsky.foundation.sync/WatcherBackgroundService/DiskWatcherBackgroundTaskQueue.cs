using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.sync.Metrics;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Models;

namespace starsky.foundation.sync.WatcherBackgroundService;

/// <summary>
///     @see: https://learn.microsoft.com/en-us/dotnet/core/extensions/queue-service
/// </summary>
[Service(typeof(IDiskWatcherBackgroundTaskQueue),
	InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class DiskWatcherBackgroundTaskQueue : IDiskWatcherBackgroundTaskQueue
{
	private readonly DiskWatcherBackgroundTaskQueueMetrics _metrics;
	private readonly Channel<BackgroundTaskQueueJob> _queue;

	public DiskWatcherBackgroundTaskQueue(IServiceScopeFactory scopeFactory)
	{
		_queue = Channel.CreateBounded<BackgroundTaskQueueJob>(
			ProcessTaskQueue.DefaultBoundedChannelOptions);
		_metrics = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<DiskWatcherBackgroundTaskQueueMetrics>();
	}

	public int Count()
	{
		return _queue.Reader.Count;
	}

	public ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		ArgumentNullException.ThrowIfNull(job);
		return string.IsNullOrWhiteSpace(job.JobType)
			? throw new ArgumentException("JobType is required", nameof(job))
			: _queue.Writer.WriteAsync(job);
	}

	public async ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(
		CancellationToken cancellationToken)
	{
		var workItem = await _queue.Reader.ReadAsync(cancellationToken);
		_metrics.Value = Count();
		return workItem;
	}
}

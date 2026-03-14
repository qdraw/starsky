using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
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
	private readonly UpdateBackgroundQueuedMetrics _metrics;
	private readonly Channel<BackgroundTaskQueueJob> _queue;

	public UpdateBackgroundTaskQueue(IServiceScopeFactory scopeFactory)
	{
		_queue = Channel.CreateBounded<BackgroundTaskQueueJob>(
			ProcessTaskQueue.DefaultBoundedChannelOptions);

		_metrics = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<UpdateBackgroundQueuedMetrics>();
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
		var queueItem = await _queue.Reader.ReadAsync(cancellationToken);
		_metrics.Value = Count();
		return queueItem;
	}
}

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.injection;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Metrics;

namespace starsky.foundation.worker.Services;

/// <summary>
/// @see: https://learn.microsoft.com/en-us/dotnet/core/extensions/queue-service
/// </summary>
[Service(typeof(IUpdateBackgroundTaskQueue), InjectionLifetime = InjectionLifetime.Singleton)]
public sealed class UpdateBackgroundTaskQueue : IUpdateBackgroundTaskQueue
{
	private readonly Channel<Tuple<Func<CancellationToken, ValueTask>, string>> _queue;
	private readonly UpdateBackgroundQueuedMetrics _metrics;

	public UpdateBackgroundTaskQueue(IServiceScopeFactory scopeFactory)
	{
		_queue = Channel.CreateBounded<Tuple<Func<CancellationToken, ValueTask>,
			string>>(ProcessTaskQueue.DefaultBoundedChannelOptions);
		_metrics = scopeFactory.CreateScope().ServiceProvider
			.GetRequiredService<UpdateBackgroundQueuedMetrics>();
	}

	public int Count()
	{
		return _queue.Reader.Count;
	}

	public ValueTask QueueBackgroundWorkItemAsync(
		Func<CancellationToken, ValueTask> workItem, string metaData)
	{
		return ProcessTaskQueue.QueueBackgroundWorkItemAsync(_queue, workItem, metaData);
	}

	public async ValueTask<Tuple<Func<CancellationToken, ValueTask>, string>> DequeueAsync(
		CancellationToken cancellationToken)
	{
		var queueItem = await _queue.Reader.ReadAsync(cancellationToken);
		_metrics.Value = Count();
		return queueItem;
	}
}

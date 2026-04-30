using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using starsky.foundation.worker.Helpers;
using starsky.foundation.worker.Interfaces;
using starsky.foundation.worker.Models;

namespace starsky.foundation.worker.Backends;

public sealed class InMemoryQueueBackend : IBaseBackgroundTaskQueue
{
	private readonly Channel<BackgroundTaskQueueJob> _queue = Channel.CreateBounded<BackgroundTaskQueueJob>(
		ProcessTaskQueue.DefaultBoundedChannelOptions);

	public int Count()
	{
		return _queue.Reader.Count;
	}

	public ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
	{
		ArgumentNullException.ThrowIfNull(job);
		return string.IsNullOrWhiteSpace(job.JobType)
			? throw new ArgumentException("JobType is required", nameof(job))
			: QueueJobInternal(job);
	}

	private ValueTask QueueJobInternal(BackgroundTaskQueueJob job)
	{
		QueueJobTenantEnforcer.ValidateTenantOrThrow(job, null, "InMemory");
		return _queue.Writer.WriteAsync(job);
	}

	public ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(CancellationToken cancellationToken)
	{
		return _queue.Reader.ReadAsync(cancellationToken);
	}
}


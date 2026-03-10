using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.Models;

namespace starsky.foundation.worker.Helpers;

public static class InMemoryBackgroundJobCallbackRegistry
{
	public const string CallbackJobType = "InMemoryCallback";

	private static readonly ConcurrentDictionary<string, Func<CancellationToken, ValueTask>>
		Callbacks = new();

	public static BackgroundTaskQueueJob Register(
		Func<CancellationToken, ValueTask> callback,
		string? metaData,
		string? traceParentId,
		int priorityLane,
		string queueName)
	{
		ArgumentNullException.ThrowIfNull(callback);
		var id = Guid.NewGuid().ToString("N");
		Callbacks[id] = callback;
		return new BackgroundTaskQueueJob
		{
			MetaData = metaData,
			TraceParentId = traceParentId,
			PriorityLane = priorityLane,
			QueueName = queueName,
			JobType = CallbackJobType,
			PayloadJson = id
		};
	}

	public static async Task<bool> TryExecuteAsync(BackgroundTaskQueueJob job,
		CancellationToken cancellationToken)
	{
		if ( job.JobType != CallbackJobType || string.IsNullOrWhiteSpace(job.PayloadJson) )
		{
			return false;
		}

		if ( !Callbacks.TryRemove(job.PayloadJson, out var callback) )
		{
			return false;
		}

		await callback(cancellationToken);
		return true;
	}
}

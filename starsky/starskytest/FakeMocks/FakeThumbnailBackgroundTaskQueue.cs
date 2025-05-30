using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.ThumbnailServices.Exceptions;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

namespace starskytest.FakeMocks;

public class FakeThumbnailBackgroundTaskQueue : IThumbnailQueuedHostedService
{
	private readonly bool _cpuOverload;

	public FakeThumbnailBackgroundTaskQueue(bool cpuOverload = false)
	{
		_cpuOverload = cpuOverload;
	}

	public int QueueBackgroundWorkItemCalledCounter { get; set; }

	public bool QueueBackgroundWorkItemCalled { get; set; }

	public int Count()
	{
		return QueueBackgroundWorkItemCalledCounter;
	}

	public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem,
		string? metaData = null,
		string? traceParentId = null)
	{
		await workItem.Invoke(CancellationToken.None);
		QueueBackgroundWorkItemCalled = true;
		QueueBackgroundWorkItemCalledCounter++;
	}

	public ValueTask<Tuple<Func<CancellationToken, ValueTask>, string?, string?>> DequeueAsync(
		CancellationToken cancellationToken)
	{
		var sayHello = GetMessage;
		var res =
			new Tuple<Func<CancellationToken, ValueTask>, string?, string?>(
				sayHello, string.Empty, string.Empty);
		return ValueTask.FromResult(res);
	}

	public bool ThrowExceptionIfCpuUsageIsToHigh(string metaData)
	{
		if ( _cpuOverload )
		{
			throw new ToManyUsageException($"CPU is to high, skip thumbnail generation {metaData}");
		}

		return true;
	}

	private static ValueTask GetMessage(CancellationToken arg)
	{
		return ValueTask.CompletedTask;
	}
}

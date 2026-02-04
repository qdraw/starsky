using System;
using System.Threading;
using System.Threading.Tasks;

namespace starsky.foundation.worker.Interfaces
{
	public interface IBaseBackgroundTaskQueue
	{
		public int Count();

		ValueTask QueueBackgroundWorkItemAsync(
			Func<CancellationToken, ValueTask> workItem,
			string? metaData = null, string? traceParentId = null);

		ValueTask<Tuple<Func<CancellationToken, ValueTask>, string?, string?>> DequeueAsync(
			CancellationToken cancellationToken);
	}
}

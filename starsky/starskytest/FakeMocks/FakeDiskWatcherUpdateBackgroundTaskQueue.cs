using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.sync.WatcherBackgroundService;

namespace starskytest.FakeMocks
{
	/// <summary>
	/// @see: FakeIBackgroundTaskQueue
	/// </summary>
	public class FakeDiskWatcherUpdateBackgroundTaskQueue : IDiskWatcherBackgroundTaskQueue
	{
		private int _count;

		public FakeDiskWatcherUpdateBackgroundTaskQueue(int count = 0)
		{
			_count = count;
		}

		public bool QueueBackgroundWorkItemCalled { get; set; }
		public int QueueBackgroundWorkItemCalledCounter { get; set; } = 0;
		public int DequeueAsyncCounter { get; set; } = 0;

		public int Count()
		{
			return _count;
		}

		public async ValueTask QueueBackgroundWorkItemAsync(
			Func<CancellationToken, ValueTask> workItem, string? metaData = null,
			string? traceParentId = null)
		{
			QueueBackgroundWorkItemCalled = true;
			QueueBackgroundWorkItemCalledCounter++;
			await workItem.Invoke(CancellationToken.None);
		}

		public ValueTask<Tuple<Func<CancellationToken, ValueTask>, string?, string?>> DequeueAsync(
			CancellationToken cancellationToken)
		{
			_count--;
			DequeueAsyncCounter++;

			var sayHello = GetMessage;
			var res =
				new Tuple<Func<CancellationToken, ValueTask>, string?, string?>(
					sayHello, "", "");
			return ValueTask.FromResult(res);
		}

		private ValueTask GetMessage(CancellationToken arg)
		{
			return ValueTask.CompletedTask;
		}
	}
}

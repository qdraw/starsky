using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.sync.WatcherBackgroundService;
using starsky.foundation.worker.Interfaces;

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

		public ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem, string metaData)
		{
			QueueBackgroundWorkItemCalled = true;
			QueueBackgroundWorkItemCalledCounter++;
			return ValueTask.CompletedTask;
		}

		public ValueTask<Tuple<Func<CancellationToken, ValueTask>, string>> DequeueAsync(CancellationToken cancellationToken)
		{
			_count--;
			DequeueAsyncCounter++;
			Func<CancellationToken, ValueTask> sayHello = GetMessage;
			var res =
				new Tuple<Func<CancellationToken, ValueTask>, string>(
					sayHello, "");
			return ValueTask.FromResult(res);
		}
		private ValueTask GetMessage(CancellationToken arg)
		{
			return ValueTask.CompletedTask;
		}
	}

}

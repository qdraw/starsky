using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.sync.WatcherBackgroundService;

namespace starskytest.FakeMocks
{
	/// <summary>
	/// @see: FakeIBackgroundTaskQueue
	/// </summary>
	public class FakeDiskWatcherUpdateBackgroundTaskQueue : IDiskWatcherBackgroundTaskQueue
	{
		public bool QueueBackgroundWorkItemCalled { get; set; }
		public int QueueBackgroundWorkItemCalledCounter { get; set; } = 0;
		
		public ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
		{
			QueueBackgroundWorkItemCalled = true;
			QueueBackgroundWorkItemCalledCounter++;
			return ValueTask.CompletedTask;
		}

		public ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

}

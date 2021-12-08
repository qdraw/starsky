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
		public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
		{
			QueueBackgroundWorkItemCalled = true;
		}

		public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

}

using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.sync.WatcherBackgroundService;

namespace starskytest.FakeMocks
{
	/// <summary>
	/// @see: FakeIBackgroundTaskQueue
	/// </summary>
	public class FakeDiskWatcherBackgroundTaskQueue : DiskWatcherBackgroundTaskQueue
	{
		public bool QueueBackgroundWorkItemCalled { get; set; }
		public override void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
		{
			QueueBackgroundWorkItemCalled = true;
			workItem.Invoke(CancellationToken.None);
		}
	}
}

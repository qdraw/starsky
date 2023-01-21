using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.ThumbnailServices.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeThumbnailBackgroundTaskQueue : IThumbnailQueuedHostedService	{

		public int Count()
		{
			return QueueBackgroundWorkItemCalledCounter;
		}

		public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem, string metaData)
		{
			await workItem.Invoke(CancellationToken.None);
			QueueBackgroundWorkItemCalled = true;
			QueueBackgroundWorkItemCalledCounter++;
		}

		public int QueueBackgroundWorkItemCalledCounter { get; set; }

		public bool QueueBackgroundWorkItemCalled { get; set; }

		public ValueTask<Tuple<Func<CancellationToken, ValueTask>, string>> DequeueAsync(CancellationToken cancellationToken)
		{
			Func<CancellationToken, ValueTask> sayHello = GetMessage;
			var res =
				new Tuple<Func<CancellationToken, ValueTask>, string>(
					sayHello, "");
			return ValueTask.FromResult(res);
		}

		private static ValueTask GetMessage(CancellationToken arg)
		{
			return ValueTask.CompletedTask;
		}
	}
}


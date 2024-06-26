using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIUpdateBackgroundTaskQueue : IUpdateBackgroundTaskQueue
	{

		public int Count()
		{
			return 0;
		}

		public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem, string? metaData = null,
			string? traceParentId = null)
		{
			await workItem.Invoke(CancellationToken.None);
			QueueBackgroundWorkItemCalled = true;
			QueueBackgroundWorkItemCalledCounter++;
		}

		public int QueueBackgroundWorkItemCalledCounter { get; set; }

		public bool QueueBackgroundWorkItemCalled { get; set; }

		public ValueTask<Tuple<Func<CancellationToken, ValueTask>, string?, string?>> DequeueAsync(CancellationToken cancellationToken)
		{
			Func<CancellationToken, ValueTask> sayHello = GetMessage;
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

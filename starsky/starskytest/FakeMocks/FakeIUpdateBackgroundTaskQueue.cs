using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.Interfaces;

namespace starskytest.FakeMocks
{
	public class FakeIUpdateBackgroundTaskQueue : IUpdateBackgroundTaskQueue
	{
		
		public ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
		{
			workItem.Invoke(CancellationToken.None);
			return ValueTask.CompletedTask;
		}

		public ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
		{
			Func<CancellationToken, ValueTask> sayHello = GetMessage;
			return ValueTask.FromResult(sayHello);
		}


		private ValueTask GetMessage(CancellationToken arg)
		{
			return ValueTask.CompletedTask;
		}
	}
}

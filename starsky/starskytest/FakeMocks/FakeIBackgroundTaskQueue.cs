using System;
using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.worker.Services;

namespace starskytest.FakeMocks
{
	public class FakeIBackgroundTaskQueue : IBackgroundTaskQueue
	{
		public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
		{
			workItem.Invoke(CancellationToken.None);
		}

		public Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
		{
			Func<CancellationToken, Task> sayHello = GetMessage;
			return Task.FromResult(sayHello);
		}

		private Task GetMessage(CancellationToken arg)
		{
			return Task.CompletedTask;
		}
	}
}

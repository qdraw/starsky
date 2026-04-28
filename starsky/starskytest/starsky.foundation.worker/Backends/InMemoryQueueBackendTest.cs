using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.worker.Backends;
using starsky.foundation.worker.Models;

namespace starskytest.starsky.foundation.worker.Backends;

[TestClass]
public sealed class InMemoryQueueBackendTest
{
	[TestMethod]
	public async Task QueueJobAsync_Null_ThrowsArgumentNullException()
	{
		var backend = new InMemoryQueueBackend();
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
			await backend.QueueJobAsync(null!));
	}

	[TestMethod]
	public async Task QueueJobAsync_WithoutJobType_ThrowsArgumentException()
	{
		var backend = new InMemoryQueueBackend();
		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await backend.QueueJobAsync(new BackgroundTaskQueueJob()));
	}

	[TestMethod]
	public async Task QueueAndDequeue_FifoAndCount_AreCorrect()
	{
		var backend = new InMemoryQueueBackend();
		var first = new BackgroundTaskQueueJob { JobType = "job-1", PayloadJson = "1" };
		var second = new BackgroundTaskQueueJob { JobType = "job-2", PayloadJson = "2" };

		await backend.QueueJobAsync(first);
		await backend.QueueJobAsync(second);

		Assert.AreEqual(2, backend.Count());

		var dequeued1 = await backend.DequeueJobAsync(CancellationToken.None);
		var dequeued2 = await backend.DequeueJobAsync(CancellationToken.None);

		Assert.AreEqual("job-1", dequeued1.JobType);
		Assert.AreEqual("job-2", dequeued2.JobType);
		Assert.AreEqual(0, backend.Count());
	}

	[TestMethod]
	public async Task DequeueJobAsync_CancelledToken_ThrowsOperationCanceledException()
	{
		var backend = new InMemoryQueueBackend();
		using var cancellation = new CancellationTokenSource();
		await cancellation.CancelAsync();
		Exception? exception = null;

		try
		{
			await backend.DequeueJobAsync(cancellation.Token);
		}
		catch ( Exception ex )
		{
			exception = ex;
		}

		Assert.IsNotNull(exception);
		Assert.IsInstanceOfType<OperationCanceledException>(exception);
	}
}






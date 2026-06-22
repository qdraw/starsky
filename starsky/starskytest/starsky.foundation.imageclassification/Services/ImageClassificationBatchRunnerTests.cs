using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.imageclassification.Interfaces;
using starsky.foundation.imageclassification.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.worker.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.imageclassification.Services;

[TestClass]
public sealed class ImageClassificationBatchRunnerTests
{
	public TestContext TestContext { get; set; }

	private sealed class FakeImageClassificationBackgroundTaskQueue :
		IImageClassificationBackgroundTaskQueue
	{
		public List<BackgroundTaskQueueJob> Jobs { get; } = [];

		public int Count()
		{
			return Jobs.Count;
		}

		public ValueTask QueueJobAsync(BackgroundTaskQueueJob job)
		{
			Jobs.Add(job);
			return ValueTask.CompletedTask;
		}

		public ValueTask<BackgroundTaskQueueJob> DequeueJobAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	[TestMethod]
	public async Task EnqueueBatchAsync_ShouldOrderByDateTime_ThenFallbackLastEdited()
	{
		var queue = new FakeImageClassificationBackgroundTaskQueue();
		var query = new FakeIQuery(
		[
			new FileIndexItem
			{
				FilePath = "/a.jpg", FileName = "a.jpg", DateTime = new DateTime(2022, 1, 1),
				LastEdited = new DateTime(2020, 1, 1)
			},
			new FileIndexItem
			{
				FilePath = "/b.jpg", FileName = "b.jpg", DateTime = default,
				LastEdited = new DateTime(2024, 1, 1)
			},
			new FileIndexItem
			{
				FilePath = "/c.jpg", FileName = "c.jpg", DateTime = new DateTime(2023, 1, 1),
				LastEdited = new DateTime(2019, 1, 1)
			}
		]);

		var appSettings = new AppSettings { ImageClassificationBatchSize = 2 };
		var sut = new ImageClassificationBatchRunner(query, queue, appSettings,
			new FakeIWebLogger());

		var queued = await sut.EnqueueBatchAsync(TestContext.CancellationToken);

		Assert.AreEqual(2, queued);
		Assert.AreEqual(2, queue.Jobs.Count);
		Assert.AreEqual("/b.jpg", queue.Jobs[0].MetaData);
		Assert.AreEqual("/c.jpg", queue.Jobs[1].MetaData);
	}

	[TestMethod]
	public void GetOrderingDate_ShouldFallbackToLastEdited_WhenDateTimeIsDefault()
	{
		var item = new FileIndexItem
		{
			DateTime = default,
			LastEdited = new DateTime(2025, 2, 1)
		};

		var result = ImageClassificationBatchRunner.GetOrderingDate(item);
		Assert.AreEqual(new DateTime(2025, 2, 1), result);
	}
}



using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.worker.ThumbnailServices.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public class SmallThumbnailBackgroundJobServiceTests
{
	[TestMethod]
	public async Task CreateJob_ShouldReturnFalse_WhenFileDoesNotExist()
	{
		// Arrange
		var fakeStorage = new FakeIStorage();
		var service = CreateService(fakeStorage);

		// Act
		var result = await service.CreateJob(true, "/non-existent-file.jpg");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task CreateJob_ShouldReturnFalse_WhenNotAuthenticated()
	{
		// Arrange
		var fakeStorage = new FakeIStorage();
		await fakeStorage.WriteStreamAsync(new MemoryStream([1, 2, 3]), "/test.jpg");
		var service = CreateService(fakeStorage);

		// Act
		var result = await service.CreateJob(false, "/test.jpg");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task CreateJob_ShouldReturnFalse_WhenQueueExceedsLimit()
	{
		// Arrange
		var fakeQueue =
			new FakeThumbnailBackgroundTaskQueue { QueueBackgroundWorkItemCalledCounter = 5000 };
		var fakeStorage = new FakeIStorage();
		await fakeStorage.WriteStreamAsync(new MemoryStream([1, 2, 3]), "/test.jpg");
		var service = CreateService(fakeStorage, fakeQueue);

		// Act
		var result = await service.CreateJob(true, "/test.jpg");

		// Assert
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task CreateJob_ShouldReturnTrue_WhenConditionsAreValid()
	{
		// Arrange
		var fakeQueue = new FakeThumbnailBackgroundTaskQueue();
		var fakeStorage = new FakeIStorage();
		await fakeStorage.WriteStreamAsync(new MemoryStream([1, 2, 3]), "/test.jpg");
		var service = CreateService(fakeStorage, fakeQueue);

		// Act
		var result = await service.CreateJob(true, "/test.jpg");

		// Assert
		Assert.IsTrue(result);
		Assert.AreEqual(1, fakeQueue.QueueBackgroundWorkItemCalledCounter);
	}

	private static SmallThumbnailBackgroundJobService CreateService(
		IStorage storage,
		IThumbnailQueuedHostedService? queue = null)
	{
		return new SmallThumbnailBackgroundJobService(
			queue ?? new FakeThumbnailBackgroundTaskQueue(),
			new FakeIThumbnailService(new FakeSelectorStorage(storage)),
			new FakeSelectorStorage(storage),
			new FakeIThumbnailSocketService(),
			new FakeIWebLogger()
		);
	}
}

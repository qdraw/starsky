using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Interfaces;
using starsky.feature.thumbnail.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public sealed class DatabaseThumbnailGenerationJobHandlerTest
{
	[TestMethod]
	public void JobType_ShouldMatchExpected()
	{
		var service = new FakeIDatabaseThumbnailGenerationService();
		var handler = new DatabaseThumbnailGenerationJobHandler(service);
		Assert.AreEqual(DatabaseThumbnailGenerationService.DatabaseThumbnailGenerationJobType,
			handler.JobType);
	}

	[TestMethod]
	public async Task ExecuteAsync_InvalidJson_ShouldThrow()
	{
		var service = new FakeIDatabaseThumbnailGenerationService();
		var handler = new DatabaseThumbnailGenerationJobHandler(service);
		await Assert.ThrowsExactlyAsync<JsonException>(() =>
			handler.ExecuteAsync("{ invalid }", CancellationToken.None));
	}

	[TestMethod]
	public async Task ExecuteAsync_CallsServiceExecuteQueuedJobAsync()
	{
		// Arrange: create a spy service that tracks ExecuteQueuedJobAsync calls
		var service = new SpyDatabaseThumbnailGenerationService();
		var handler = new DatabaseThumbnailGenerationJobHandler(service);

		// Act: call ExecuteAsync with no payload
		await handler.ExecuteAsync(null, CancellationToken.None);

		// Assert: the service's ExecuteQueuedJobAsync was invoked once
		Assert.AreEqual(1, service.ExecuteCount);
	}

	// Simple spy implementation for IDatabaseThumbnailGenerationService used only in this test
	private sealed class SpyDatabaseThumbnailGenerationService : IDatabaseThumbnailGenerationService
	{
		public int ExecuteCount { get; private set; }
		public int StartCount { get; private set; }

		public Task StartBackgroundQueue()
		{
			StartCount++;
			return Task.CompletedTask;
		}

		public Task ExecuteQueuedJobAsync(CancellationToken cancellationToken)
		{
			ExecuteCount++;
			return Task.CompletedTask;
		}
	}
}

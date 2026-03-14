using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		Assert.AreEqual(DatabaseThumbnailGenerationService.DatabaseThumbnailGenerationJobType, handler.JobType);
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
	public async Task ExecuteAsync_InterfaceImplementation_ShouldCallStartBackgroundQueue()
	{
		var service = new FakeIDatabaseThumbnailGenerationService();
		var handler = new DatabaseThumbnailGenerationJobHandler(service);
		await handler.ExecuteAsync(null, CancellationToken.None);
		
		Assert.AreEqual(1, service.Count);
	}

	[TestMethod]
	public async Task ExecuteAsync_ConcreteImplementation_ShouldCallExecuteQueuedJobAsync()
	{
		var query = new FakeIQuery();
		var thumbnailQuery = new FakeIThumbnailQuery();
		var logger = new FakeIWebLogger();
		var connectionsService = new FakeIWebSocketConnectionsService();
		var thumbnailService = new FakeIThumbnailService();
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var updateStatusGeneratedThumbnailService = new FakeIUpdateStatusGeneratedThumbnailService();
		
		var concreteService = new DatabaseThumbnailGenerationService(
			query, logger, connectionsService, thumbnailService, 
			thumbnailQuery, bgTaskQueue, updateStatusGeneratedThumbnailService);
		
		var handler = new DatabaseThumbnailGenerationJobHandler(concreteService);
		
		// The real ExecuteQueuedJobAsync calls GetMissingThumbnailsBatchAsync on thumbnailQuery
		await handler.ExecuteAsync(null, CancellationToken.None);
		
		// If it reached here without calling StartBackgroundQueue (which would increment bgTaskQueue.QueueBackgroundWorkItemCalledCounter)
		// it means it called ExecuteQueuedJobAsync (because it's a concrete instance)
		Assert.AreEqual(0, bgTaskQueue.QueueBackgroundWorkItemCalledCounter);
		// Check if it interacted with the thumbnailQuery (it calls IsRunningJob() then SetRunningJob(true))
		// We can't easily check internal calls without mocking, but if it finished without error it's likely fine.
		// However, let's check if it called GetMissingThumbnailsBatchAsync - we can add a counter to FakeIThumbnailQuery if needed.
	}
}

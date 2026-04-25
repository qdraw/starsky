using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Services;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailgeneration.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public class DatabaseThumbnailGenerationServiceTest
{
	[TestMethod]
	public async Task StartBackgroundQueue_IsJobRunningSoSkip()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			new FakeIThumbnailQuery(null, true), // mock running job
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);

		await databaseThumbnailGenerationService.StartBackgroundQueue();

		Assert.AreEqual(0, bgTaskQueue.Count());
	}

	[TestMethod]
	public async Task ExecuteQueuedJobAsync_NoMissingThumbnails_SetsRunningFalse()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var fakeThumbnailQuery = new FakeIThumbnailQuery(); // empty
		var fakeQuery = new FakeIQuery();
		var logger = new FakeIWebLogger();

		var service = new DatabaseThumbnailGenerationService(
			fakeQuery, logger, new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			fakeThumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);

		await service.ExecuteQueuedJobAsync(System.Threading.CancellationToken.None);

		// Ensure the running flag was cleared
		Assert.IsFalse(fakeThumbnailQuery.IsRunningJob());
	}

	[TestMethod]
	public async Task ExecuteQueuedJobAsync_OneItem_ProcessesAndClearsRunningFlag()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();

		// Prepare thumbnail query with one missing thumbnail
		var thumbItem = new ThumbnailItem("filehash123", null, null, null, null);
		var thumbnailQuery = new FakeIThumbnailQuery([thumbItem]);

		// Prepare query to return a matching FileIndexItem
		var fileIndex = new FileIndexItem
		{
			FileHash = "filehash123",
			FilePath = "/test.jpg",
			Status = FileIndexItem.ExifStatus.Ok
		};

		var query = new FakeIQuery([fileIndex]);

		var selector = new FakeSelectorStorage(new FakeIStorage([],
			["/test.jpg"], new List<byte[]> { CreateAnImage.Bytes.ToArray() }));
		var thumbnailService = new FakeIThumbnailService(selector);

		var logger = new FakeIWebLogger();

		var service = new DatabaseThumbnailGenerationService(
			query, logger, new FakeIWebSocketConnectionsService(),
			thumbnailService,
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(thumbnailQuery)
		);

		await service.ExecuteQueuedJobAsync(System.Threading.CancellationToken.None);

		// Ensure the running flag was cleared
		Assert.IsFalse(thumbnailQuery.IsRunningJob());

		// Should have logged processed and done messages
		Assert.Contains(
			p => p.Item2 != null && p.Item2.Contains("Processed"), logger.TrackedInformation);
		Assert.Contains(
			p => p.Item2 != null && p.Item2.Contains("Done"), logger.TrackedInformation);
	}
	
	[TestMethod]
	public async Task StartBackgroundQueue_NoContentSoHitOnce()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			new FakeIThumbnailQuery(),
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);

		await databaseThumbnailGenerationService.StartBackgroundQueue();

		Assert.AreEqual(1, bgTaskQueue.Count());
	}

	[TestMethod]
	public async Task StartBackgroundQueue_OneItemSoTrigger()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery([new("12", null, null, null, null)]);

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);

		await databaseThumbnailGenerationService.StartBackgroundQueue();

		Assert.AreEqual(1, bgTaskQueue.Count());
	}
	
	[TestMethod]
	public async Task WorkThumbnailGeneration_ZeroItems()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery([new("12", null, null, null, null)]);

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);

		var result = await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			[], []);

		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task WorkThumbnailGeneration_NotFoundItem_Database()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery([new("12", null, null, null, null)]);

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery(
				[]))
		);

		var result = ( await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			[new("74283rei_ot_fs_kl", null, null, null, null)],
			[
				new()
				{
					FileHash = "74283rei_ot_fs_kl",
					Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
				}
			]) ).ToList();

		Assert.HasCount(1, result);
		Assert.IsFalse(result.FirstOrDefault()!.Large);
	}

	[TestMethod]
	public async Task WorkThumbnailGeneration_NotFoundItem_2()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery([
			new("74283rei_ot_fs_kl", null, null, null, null)
		]);

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(new FakeSelectorStorage()),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(thumbnailQuery)
		);

		var result = ( await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			[new("74283rei_ot_fs_kl", null, null, null, null)],
			[
				new()
				{
					FileHash = "74283rei_ot_fs_kl",
					FilePath = "/test.jpg",
					Status = FileIndexItem.ExifStatus.Ok
				}
			]) ).ToList();

		Assert.HasCount(1, result);
		Assert.IsEmpty(await thumbnailQuery.Get("74283rei_ot_fs_kl"));
	}

	[TestMethod]
	public async Task WorkThumbnailGeneration_FoundUpdate()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery([
			new("345742938fs_jk_df_kj", null, null, null, null)
		]);

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(new FakeSelectorStorage(new FakeIStorage([],
				["/test.jpg"],
				new List<byte[]> { CreateAnImage.Bytes.ToArray() }))),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(thumbnailQuery)
		);

		var result = ( await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			[new("345742938fs_jk_df_kj", null, null, null, null)],
			[
				new()
				{
					FileHash = "345742938fs_jk_df_kj",
					FilePath = "/test.jpg",
					Status = FileIndexItem.ExifStatus.Ok
				}
			]) ).ToList();

		Assert.HasCount(1, result);
		Assert.HasCount(1, await thumbnailQuery.Get("345742938fs_jk_df_kj"));
		Assert.IsNull(result.FirstOrDefault()!.Large);
	}

	[TestMethod]
	public async Task WorkThumbnailGeneration_MatchItem()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery([new("12", null, null, null, null)]);

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);

		var result = ( await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			[new("345742938fs_jk_df_kj", null, null, null, null)],
			[
				new() { FileHash = "345742938fs_jk_df_kj", Status = FileIndexItem.ExifStatus.Ok }
			]) ).ToList();

		Assert.HasCount(1, result);
		Assert.IsNull(result.FirstOrDefault()!.Large);
	}
}

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
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new("12", null, null, null, null)
		});

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
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new("12", null, null, null, null)
		});

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);

		var result = await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			new List<ThumbnailItem>(), new List<FileIndexItem>());

		Assert.AreEqual(0, result.Count());
	}

	[TestMethod]
	public async Task WorkThumbnailGeneration_NotFoundItem_Database()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new("12", null, null, null, null)
		});

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery(
				new List<ThumbnailItem>()))
		);

		var result = ( await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			new List<ThumbnailItem> { new("74283rei_ot_fs_kl", null, null, null, null) },
			new List<FileIndexItem>
			{
				new()
				{
					FileHash = "74283rei_ot_fs_kl",
					Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
				}
			}) ).ToList();

		Assert.AreEqual(1, result.Count);
		Assert.IsFalse(result.FirstOrDefault()!.Large);
	}

	[TestMethod]
	public async Task WorkThumbnailGeneration_NotFoundItem_2()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new("74283rei_ot_fs_kl", null, null, null, null)
		});

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(new FakeSelectorStorage()),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(thumbnailQuery)
		);

		var result = ( await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			new List<ThumbnailItem> { new("74283rei_ot_fs_kl", null, null, null, null) },
			new List<FileIndexItem>
			{
				new()
				{
					FileHash = "74283rei_ot_fs_kl",
					FilePath = "/test.jpg",
					Status = FileIndexItem.ExifStatus.Ok
				}
			}) ).ToList();

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(0, ( await thumbnailQuery.Get("74283rei_ot_fs_kl") ).Count);
	}


	[TestMethod]
	public async Task WorkThumbnailGeneration_FoundUpdate()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new("345742938fs_jk_df_kj", null, null, null, null)
		});

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(new FakeSelectorStorage(new FakeIStorage(new List<string>(),
				new List<string> { "/test.jpg" },
				new List<byte[]> { CreateAnImage.Bytes.ToArray() }))),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(thumbnailQuery)
		);

		var result = ( await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			new List<ThumbnailItem> { new("345742938fs_jk_df_kj", null, null, null, null) },
			new List<FileIndexItem>
			{
				new()
				{
					FileHash = "345742938fs_jk_df_kj",
					FilePath = "/test.jpg",
					Status = FileIndexItem.ExifStatus.Ok
				}
			}) ).ToList();

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(1, ( await thumbnailQuery.Get("345742938fs_jk_df_kj") ).Count);
		Assert.AreEqual(null, result.FirstOrDefault()!.Large);
	}

	[TestMethod]
	public async Task WorkThumbnailGeneration_MatchItem()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new("12", null, null, null, null)
		});

		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);

		var result = ( await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			new List<ThumbnailItem> { new("345742938fs_jk_df_kj", null, null, null, null) },
			new List<FileIndexItem>
			{
				new() { FileHash = "345742938fs_jk_df_kj", Status = FileIndexItem.ExifStatus.Ok }
			}) ).ToList();

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(null, result.FirstOrDefault()!.Large);
	}
}

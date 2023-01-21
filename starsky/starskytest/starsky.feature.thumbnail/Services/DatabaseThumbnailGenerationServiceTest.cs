using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Services;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailgeneration.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public class DatabaseThumbnailGenerationServiceTest
{
	
	[TestMethod]
	public async Task StartBackgroundQueue_NoContentSoNotFired()
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
		
		Assert.AreEqual(0,bgTaskQueue.Count());
	}
	
	[TestMethod]
	public async Task StartBackgroundQueue_OneItemSoTrigger()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new ThumbnailItem("12",null,null,null,null)
		});
		
		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);
		
		await databaseThumbnailGenerationService.StartBackgroundQueue();
		
		Assert.AreEqual(1,bgTaskQueue.Count());
	}
	
	[TestMethod]
	public async Task WorkThumbnailGeneration_ZeroItems()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new ThumbnailItem("12",null,null,null,null)
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
		
		Assert.AreEqual(0,result.Count());
	}
	
	
	[TestMethod]
	public async Task WorkThumbnailGeneration_NotFoundItem()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new ThumbnailItem("12",null,null,null,null)
		});
		
		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);
		
		var result = (await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			new List<ThumbnailItem>
			{
				new ThumbnailItem("74283reiotfskl",null,null,null,null)
			}, new List<FileIndexItem>
			{
				new FileIndexItem()
				{
					FileHash = "74283reiotfskl",
					Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
				}
			})).ToList();
		
		Assert.AreEqual(1,result.Count);
		Assert.AreEqual(false,result.FirstOrDefault()!.Large);
	}
		
	[TestMethod]
	public async Task WorkThumbnailGeneration_MatchItem()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new ThumbnailItem("12",null,null,null,null)
		});
		
		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery())
		);
		
		var result = (await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			new List<ThumbnailItem>
			{
				new ThumbnailItem("74283reiotfskl",null,null,null,null)
			}, new List<FileIndexItem>
			{
				new FileIndexItem()
				{
					FileHash = "74283reiotfskl",
					Status = FileIndexItem.ExifStatus.Ok
				}
			})).ToList();
		
		Assert.AreEqual(1,result.Count);
		Assert.AreEqual(null,result.FirstOrDefault()!.Large);
	}
}

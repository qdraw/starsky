using System;
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

		await databaseThumbnailGenerationService.StartBackgroundQueue(
			DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
		
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
		
		await databaseThumbnailGenerationService.StartBackgroundQueue(
			DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)));
		
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
	public async Task WorkThumbnailGeneration_NotFoundItem_Database()
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
			new UpdateStatusGeneratedThumbnailService(new FakeIThumbnailQuery(
				new List<ThumbnailItem>()))
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
		Assert.IsFalse(result.FirstOrDefault()!.Large);
	}
	
	[TestMethod]
	public async Task WorkThumbnailGeneration_NotFoundItem_2()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new ThumbnailItem("74283reiotfskl",null,null,null,null)
		});
		
		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(new FakeSelectorStorage()),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(thumbnailQuery)
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
					FilePath = "/test.jpg",
					Status = FileIndexItem.ExifStatus.Ok
				}
			})).ToList();
		
		Assert.AreEqual(1,result.Count);
		Assert.AreEqual(0,(await thumbnailQuery.Get( "74283reiotfskl")).Count);
	}
	
		
	[TestMethod]
	public async Task WorkThumbnailGeneration_FoundUpdate()
	{
		var bgTaskQueue = new FakeThumbnailBackgroundTaskQueue();
		var thumbnailQuery = new FakeIThumbnailQuery(new List<ThumbnailItem>
		{
			new ThumbnailItem("345742938fsdjkdfkj",null,null,null,null)
		});
		
		var databaseThumbnailGenerationService = new DatabaseThumbnailGenerationService(
			new FakeIQuery(), new FakeIWebLogger(), new FakeIWebSocketConnectionsService(),
			new FakeIThumbnailService(new FakeSelectorStorage(new FakeIStorage(new List<string>(), new List<string>
			{
				"/test.jpg"
			}, new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes.ToArray()}))),
			thumbnailQuery,
			bgTaskQueue,
			new UpdateStatusGeneratedThumbnailService(thumbnailQuery)
		);
		
		var result = (await databaseThumbnailGenerationService.WorkThumbnailGeneration(
			new List<ThumbnailItem>
			{
				new ThumbnailItem("345742938fsdjkdfkj",null,null,null,null)
			}, new List<FileIndexItem>
			{
				new FileIndexItem()
				{
					FileHash = "345742938fsdjkdfkj",
					FilePath = "/test.jpg",
					Status = FileIndexItem.ExifStatus.Ok
				}
			})).ToList();
		
		Assert.AreEqual(1,result.Count);
		Assert.AreEqual(1,(await thumbnailQuery.Get( "345742938fsdjkdfkj")).Count);
		Assert.AreEqual(null,result.FirstOrDefault()!.Large);
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
	
	[TestMethod]
	public async Task FilterWorkThumbnailGeneration_Timeout()
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

		var result = (await databaseThumbnailGenerationService.FilterAndWorkThumbnailGeneration(
			new DateTime(2000,01,01,01,01,01, kind: DateTimeKind.Local),  
			new List<ThumbnailItem>
			{
				new ThumbnailItem("23478928939438234",null,null,null,null)
			}, new List<FileIndexItem>
			{
				new FileIndexItem()
				{
					FileHash = "23478928939438234",
					Status = FileIndexItem.ExifStatus.Ok
				}
			})).ToList();
		
		Assert.AreEqual(0,result.Count);
	}

	[TestMethod]
	public async Task FilterWorkThumbnailGeneration_MatchItem()
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
		
		var result = (await databaseThumbnailGenerationService.FilterAndWorkThumbnailGeneration(
			new DateTime(3000,01,01,01,01,01, kind: DateTimeKind.Local), 
			new List<ThumbnailItem>
			{
				new ThumbnailItem("2437998234",null,null,null,null)
			}, new List<FileIndexItem>
			{
				new FileIndexItem()
				{
					FileHash = "2437998234",
					Status = FileIndexItem.ExifStatus.Ok
				}
			})).ToList();
		
		Assert.AreEqual(1,result.Count);
		Assert.AreEqual(null,result.FirstOrDefault()!.Large);
	}
}

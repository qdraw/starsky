using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.thumbnail.Services;
using starsky.foundation.database.Models;
using starsky.foundation.thumbnailgeneration.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.thumbnail.Services;

[TestClass]
public class ThumbnailSocketServiceTests
{
	private static ThumbnailSocketService CreateSut(FakeIQuery fakeQuery,
		FakeIWebSocketConnectionsService fakeConnectionsService,
		FakeINotificationQuery fakeNotificationQuery)
	{
		var logger = new FakeIWebLogger();

		var service = new ThumbnailSocketService(
			fakeQuery, fakeConnectionsService, logger, fakeNotificationQuery);
		return service;
	}

	[TestMethod]
	public async Task NotificationSocketUpdate_ShouldSendResults_WhenFilesNeedUpdate()
	{
		// Arrange
		var fakeQuery = new FakeIQuery(new List<FileIndexItem>
		{
			new("/test.jpg") { Tags = "tag1" }
		});

		var fakeConnectionsService = new FakeIWebSocketConnectionsService();
		var fakeNotificationQuery = new FakeINotificationQuery();

		var service = CreateSut(fakeQuery, fakeConnectionsService, fakeNotificationQuery);

		var generateThumbnailResults = new List<GenerationResultModel>
		{
			new() { SubPath = "/test.jpg", Success = true }
		};

		// Act
		await service.NotificationSocketUpdate("/test.jpg", generateThumbnailResults);

		// Assert
		Assert.AreEqual(1, fakeConnectionsService.FakeSendToAllAsync.Count);
		Assert.AreEqual(1, fakeNotificationQuery.FakeContent.Count);
	}

	[TestMethod]
	public async Task NotificationSocketUpdate_Folder_ShouldSendResults_WhenFilesNeedUpdate()
	{
		// Arrange
		var fakeQuery = new FakeIQuery(new List<FileIndexItem>
		{
			new("/") { IsDirectory = true }, new("/test.jpg") { Tags = "tag1" }
		});

		var fakeConnectionsService = new FakeIWebSocketConnectionsService();
		var fakeNotificationQuery = new FakeINotificationQuery();

		var service = CreateSut(fakeQuery, fakeConnectionsService, fakeNotificationQuery);

		var generateThumbnailResults = new List<GenerationResultModel>
		{
			new() { SubPath = "/test.jpg", Success = true }
		};

		// Act
		await service.NotificationSocketUpdate("/", generateThumbnailResults);

		// Assert
		Assert.AreEqual(1, fakeConnectionsService.FakeSendToAllAsync.Count);
		Assert.AreEqual(1, fakeNotificationQuery.FakeContent.Count);
	}

	[TestMethod]
	public async Task NotificationSocketUpdate_NoResults()
	{
		var fakeQuery = new FakeIQuery();
		var fakeConnectionsService = new FakeIWebSocketConnectionsService();
		var fakeNotificationQuery = new FakeINotificationQuery();

		var service = CreateSut(fakeQuery, fakeConnectionsService, fakeNotificationQuery);

		await service.NotificationSocketUpdate(null!, []);
		Assert.AreEqual(0, fakeConnectionsService.FakeSendToAllAsync.Count);
	}

	[TestMethod]
	public async Task NotificationSocketUpdate_ShouldNotSendResults_WhenNoFilesNeedUpdate()
	{
		// Arrange
		var fakeQuery = new FakeIQuery(new List<FileIndexItem>
		{
			new("/test.jpg") { Tags = TrashKeyword.TrashKeywordString }
		});

		var fakeConnectionsService = new FakeIWebSocketConnectionsService();
		var fakeNotificationQuery = new FakeINotificationQuery();

		var service = CreateSut(fakeQuery, fakeConnectionsService, fakeNotificationQuery);

		var generateThumbnailResults = new List<GenerationResultModel>
		{
			new() { SubPath = "/test.jpg", Success = true }
		};

		// Act
		await service.NotificationSocketUpdate("/test.jpg", generateThumbnailResults);

		// Assert
		Assert.AreEqual(0, fakeConnectionsService.FakeSendToAllAsync.Count);
		Assert.AreEqual(0, fakeNotificationQuery.FakeContent.Count);
	}

	[TestMethod]
	public void WhichFilesNeedToBePushedForUpdate_NothingToUpdate()
	{
		var result = ThumbnailSocketService.WhichFilesNeedToBePushedForUpdates(
			new List<GenerationResultModel>(), new List<FileIndexItem>());
		Assert.AreEqual(0, result.Count);
	}


	[TestMethod]
	public void WhichFilesNeedToBePushedForUpdate_DoesNotExistInFilesList()
	{
		var result = ThumbnailSocketService.WhichFilesNeedToBePushedForUpdates(
			[new GenerationResultModel { SubPath = "/test.jpg", Success = true }],
			new List<FileIndexItem>());

		Assert.AreEqual(0, result.Count);
	}

	[TestMethod]
	public void WhichFilesNeedToBePushedForUpdate_DeletedSoIgnored()
	{
		var result = ThumbnailSocketService.WhichFilesNeedToBePushedForUpdates(
			[new GenerationResultModel { SubPath = "/test.jpg", Success = true }],
			new List<FileIndexItem>
			{
				new("/test.jpg")
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = TrashKeyword.TrashKeywordString
				}
			});

		Assert.AreEqual(0, result.Count);
	}


	[TestMethod]
	public void WhichFilesNeedToBePushedForUpdate_ShouldMap()
	{
		var result = ThumbnailSocketService.WhichFilesNeedToBePushedForUpdates(
			new List<GenerationResultModel> { new() { SubPath = "/test.jpg", Success = true } },
			new List<FileIndexItem> { new("/test.jpg") });

		Assert.AreEqual(1, result.Count);
	}
}

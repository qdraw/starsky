using System.Collections.Generic;
using System.Linq;
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
			new() { SubPath = "/test.jpg", Success = true, FileHash = "test" }
		};

		// Act
		await service.NotificationSocketUpdate("/test.jpg", generateThumbnailResults);

		// Assert
		Assert.HasCount(1, fakeConnectionsService.FakeSendToAllAsync);
		Assert.HasCount(1, fakeNotificationQuery.FakeContent);
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
			new() { SubPath = "/test.jpg", Success = true, FileHash = "test" }
		};

		// Act
		await service.NotificationSocketUpdate("/", generateThumbnailResults);

		// Assert
		Assert.HasCount(1, fakeConnectionsService.FakeSendToAllAsync);
		Assert.HasCount(1, fakeNotificationQuery.FakeContent);
	}

	[TestMethod]
	public async Task NotificationSocketUpdate_NoResults()
	{
		var fakeQuery = new FakeIQuery();
		var fakeConnectionsService = new FakeIWebSocketConnectionsService();
		var fakeNotificationQuery = new FakeINotificationQuery();

		var service = CreateSut(fakeQuery, fakeConnectionsService, fakeNotificationQuery);

		await service.NotificationSocketUpdate(null!, []);
		Assert.IsEmpty(fakeConnectionsService.FakeSendToAllAsync);
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
			new() { SubPath = "/test.jpg", Success = true, FileHash = "test" }
		};

		// Act
		await service.NotificationSocketUpdate("/test.jpg", generateThumbnailResults);

		// Assert
		Assert.IsEmpty(fakeConnectionsService.FakeSendToAllAsync);
		Assert.IsEmpty(fakeNotificationQuery.FakeContent);
	}

	[TestMethod]
	public void WhichFilesNeedToBePushedForUpdate_NothingToUpdate()
	{
		var result = ThumbnailSocketService.WhichFilesNeedToBePushedForUpdates(
			new List<GenerationResultModel>(), new List<FileIndexItem>());
		Assert.IsEmpty(result);
	}


	[TestMethod]
	public void WhichFilesNeedToBePushedForUpdate_DoesNotExistInFilesList()
	{
		var result = ThumbnailSocketService.WhichFilesNeedToBePushedForUpdates(
			[
				new GenerationResultModel
				{
					SubPath = "/test.jpg", Success = true, FileHash = "test"
				}
			],
			new List<FileIndexItem>());

		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void WhichFilesNeedToBePushedForUpdate_DeletedSoIgnored()
	{
		var result = ThumbnailSocketService.WhichFilesNeedToBePushedForUpdates(
			[
				new GenerationResultModel
				{
					SubPath = "/test.jpg", Success = true, FileHash = "test"
				}
			],
			new List<FileIndexItem>
			{
				new("/test.jpg")
				{
					Status = FileIndexItem.ExifStatus.Ok,
					Tags = TrashKeyword.TrashKeywordString
				}
			});

		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void WhichFilesNeedToBePushedForUpdate_ShouldMap()
	{
		var result = ThumbnailSocketService.WhichFilesNeedToBePushedForUpdates(
			new List<GenerationResultModel>
			{
				new() { SubPath = "/test.jpg", Success = true, FileHash = "test" }
			},
			new List<FileIndexItem> { new("/test.jpg") });

		Assert.HasCount(1, result);
	}

	[TestMethod]
	public void WhichFilesNeedToBePushedForUpdates_ShouldAssignCorrectFileHash()
	{
		// Arrange
		var thumbs = new List<GenerationResultModel>
		{
			new() { SubPath = "/test/file1.jpg", FileHash = "hash1", Success = true },
			new() { SubPath = "/test/file2.jpg", FileHash = "hash2", Success = true }
		};

		var fileIndexItems = new List<FileIndexItem>
		{
			new() { FilePath = "/test/file1.jpg", Tags = "tag1" },
			new() { FilePath = "/test/file2.jpg", Tags = "tag2" }
		};

		// Act
		var result =
			ThumbnailSocketService.WhichFilesNeedToBePushedForUpdates(thumbs, fileIndexItems);

		// Assert
		Assert.AreEqual("hash1",
			result.FirstOrDefault(x => x.FilePath == "/test/file1.jpg")?.FileHash);
		Assert.AreEqual("hash2",
			result.FirstOrDefault(x => x.FilePath == "/test/file2.jpg")?.FileHash);
	}
}

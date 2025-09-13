using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.trash.Services;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.trash.Services;

[TestClass]
public class TrashConnectionServiceTest
{
	[TestMethod]
	public async Task ConnectionServiceAsync_NotFound()
	{
		var webSocketService = new FakeIWebSocketConnectionsService();
		var notificationQuery = new FakeINotificationQuery();
		var trashConnectionService =
			new TrashConnectionService(webSocketService, notificationQuery);
		var moveToTrash = new List<FileIndexItem> { new("/test.jpg") };
		var result = await trashConnectionService.ConnectionServiceAsync(moveToTrash, true);

		Assert.IsNotNull(result);
		Assert.HasCount(1, result);

		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
			result.FirstOrDefault()?.Status);
		Assert.AreEqual("/test.jpg", result.FirstOrDefault()?.FilePath);
	}

	[TestMethod]
	public async Task ConnectionServiceAsync_Deleted()
	{
		var webSocketService = new FakeIWebSocketConnectionsService();
		var notificationQuery = new FakeINotificationQuery();
		var trashConnectionService =
			new TrashConnectionService(webSocketService, notificationQuery);
		var moveToTrash = new List<FileIndexItem> { new("/test.jpg") };
		var result = await trashConnectionService.ConnectionServiceAsync(moveToTrash, false);

		Assert.IsNotNull(result);
		Assert.HasCount(1, result);

		Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, result.FirstOrDefault()?.Status);
		Assert.AreEqual("/test.jpg", result.FirstOrDefault()?.FilePath);
	}

	[TestMethod]
	public async Task ConnectionServiceAsync_NotFound_Socket()
	{
		var webSocketService = new FakeIWebSocketConnectionsService();
		var notificationQuery = new FakeINotificationQuery();
		var trashConnectionService =
			new TrashConnectionService(webSocketService, notificationQuery);
		var moveToTrash = new List<FileIndexItem> { new("/test.jpg") };
		await trashConnectionService.ConnectionServiceAsync(moveToTrash, true);

		Assert.HasCount(1, webSocketService.FakeSendToAllAsync);
		Assert.HasCount(1, notificationQuery.FakeContent);
	}
}

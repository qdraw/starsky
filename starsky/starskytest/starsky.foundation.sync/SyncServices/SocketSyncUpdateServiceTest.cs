using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices;

[TestClass]
public class SocketSyncUpdateServiceTest
{
	[TestMethod]
	public async Task PushToSockets_ContainsValue()
	{
		var socket = new FakeIWebSocketConnectionsService();

		var service =
			new SocketSyncUpdateService(socket, new FakeINotificationQuery(), new FakeIWebLogger());

		await service.PushToSockets(new List<FileIndexItem> { new("/test.jpg") });

		Assert.Contains("/test.jpg", socket.FakeSendToAllAsync[0]);
	}

	[TestMethod]
	public async Task PushToSockets_CatchException()
	{
		var socket = new FakeIWebSocketConnectionsService(new WebSocketException("test"));

		var service =
			new SocketSyncUpdateService(socket, new FakeINotificationQuery(), new FakeIWebLogger());

		await service.PushToSockets(new List<FileIndexItem> { new("/test.jpg") });

		Assert.IsEmpty(socket.FakeSendToAllAsync);
	}

	[TestMethod]
	public void FilterBefore_OkShouldPass()
	{
		var result = SocketSyncUpdateService.FilterBefore(
			new List<FileIndexItem> { new("/test.jpg") { Status = FileIndexItem.ExifStatus.Ok } });

		Assert.HasCount(1, result);
		Assert.AreEqual("/test.jpg", result[0].FilePath);
	}

	[TestMethod]
	public void FilterBefore_NotFoundShouldPass()
	{
		var result = SocketSyncUpdateService.FilterBefore(
			new List<FileIndexItem>
			{
				new("/test.jpg") { Status = FileIndexItem.ExifStatus.NotFoundSourceMissing }
			});

		Assert.HasCount(1, result);
		Assert.AreEqual("/test.jpg", result[0].FilePath);
	}

	[TestMethod]
	public void FilterBefore_ShouldIgnoreHome()
	{
		var result = SocketSyncUpdateService.FilterBefore(
			new List<FileIndexItem> { new("/") { Status = FileIndexItem.ExifStatus.Ok } });

		Assert.IsEmpty(result);
	}
}

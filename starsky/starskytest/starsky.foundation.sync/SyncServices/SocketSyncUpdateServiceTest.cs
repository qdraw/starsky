using System.Collections.Generic;
using System.Linq;
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
                
		var service = new SocketSyncUpdateService(socket, new FakeINotificationQuery(), new FakeIWebLogger());

		await service.PushToSockets(new List<FileIndexItem>{new FileIndexItem("/test.jpg")});
    
		Assert.IsTrue(socket.FakeSendToAllAsync[0].Contains("/test.jpg"));
	}
	
	[TestMethod]
	public async Task PushToSockets_CatchException()
	{
		var socket = new FakeIWebSocketConnectionsService(new WebSocketException("test"));
                
		var service = new SocketSyncUpdateService(socket, new FakeINotificationQuery(), new FakeIWebLogger());

		await service.PushToSockets(new List<FileIndexItem>{new FileIndexItem("/test.jpg")});
    
		Assert.IsFalse(socket.FakeSendToAllAsync.Any());
	}

	[TestMethod]
	public void FilterBefore_OkShouldPass()
	{
		var result=  SocketSyncUpdateService.FilterBefore(
			new List<FileIndexItem>{new FileIndexItem("/test.jpg")
			{
				Status = FileIndexItem.ExifStatus.Ok
			}});
    
		Assert.AreEqual(1,result.Count);
		Assert.AreEqual("/test.jpg",result[0].FilePath);
	}
    		
	[TestMethod]
	public void FilterBefore_NotFoundShouldPass()
	{
		var result=  SocketSyncUpdateService.FilterBefore(
			new List<FileIndexItem>{new FileIndexItem("/test.jpg")
			{
				Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
			}});
    
		Assert.AreEqual(1,result.Count);
		Assert.AreEqual("/test.jpg",result[0].FilePath);
	}

	[TestMethod]
	public void FilterBefore_ShouldIgnoreHome()
	{
		var result=  SocketSyncUpdateService.FilterBefore(
			new List<FileIndexItem>{new FileIndexItem("/")
			{
				Status = FileIndexItem.ExifStatus.Ok
			}});
    
		Assert.AreEqual(0,result.Count);
	}
}

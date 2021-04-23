using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public class ManualBackgroundSyncServiceTest
	{
		[TestMethod]
		public async Task NotFound()
		{
			var result = await new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(new Dictionary<string, object>()))
				.ManualSync("/test");
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result);
		}
		
		[TestMethod]
		public async Task ObjectStarted()
		{
			var result = await new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(new List<FileIndexItem>{new FileIndexItem("/test")}),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(new Dictionary<string, object>()))
				.ManualSync("/test");
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result);
		}
		
		[TestMethod]
		public async Task IgnoreWhenCacheValue()
		{
			var result = await new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(new List<FileIndexItem>{new FileIndexItem("/test")}),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(new Dictionary<string, object>{{ManualBackgroundSyncService.QueryCacheName + "/test", string.Empty}}))
				.ManualSync("/test");
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result);
		}

		[TestMethod]
		public async Task BackgroundTask_expect_socket_output()
		{
			var fakeSocket = new FakeIWebSocketConnectionsService();
			var items = new List<FileIndexItem> {new FileIndexItem("/test")};
			await new ManualBackgroundSyncService(
					new FakeISynchronize(items),
					new FakeIQuery(items),
					fakeSocket,
					new FakeMemoryCache(new Dictionary<string, object>{{ManualBackgroundSyncService.QueryCacheName + "/test", string.Empty}}))
				.BackgroundTask("/test");

			Assert.AreEqual(JsonSerializer.Serialize(items, 
				DefaultJsonSerializer.CamelCase), fakeSocket.FakeSendToAllAsync[0]);
		}
	}
}

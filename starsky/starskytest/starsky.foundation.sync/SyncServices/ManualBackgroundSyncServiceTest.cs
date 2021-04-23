using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
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
	}
}

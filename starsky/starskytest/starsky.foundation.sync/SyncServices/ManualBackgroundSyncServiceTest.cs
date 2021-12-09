using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public class ManualBackgroundSyncServiceTest
	{
		private IServiceScopeFactory GetScope()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task NotFound()
		{
			var result = await new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(new Dictionary<string, object>()),
					new FakeIWebLogger(), 
					new FakeIUpdateBackgroundTaskQueue(), 
					GetScope()
					)
				.ManualSync("/test", string.Empty);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result);
		}
		
		[TestMethod]
		public async Task HomeIsAlwaysFound()
		{
			var result = await new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(new Dictionary<string, object>()), 
					new FakeIWebLogger(), new FakeIUpdateBackgroundTaskQueue(),GetScope())
				.ManualSync("/",string.Empty);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result);
		}
		
		[TestMethod]
		public async Task ObjectStarted()
		{
			var result = await new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(new List<FileIndexItem>{new FileIndexItem("/test")}),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(new Dictionary<string, object>()),
					new FakeIWebLogger(), new FakeIUpdateBackgroundTaskQueue(), GetScope())
				.ManualSync("/test", string.Empty);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result);
		}
		
		[TestMethod]
		public async Task IgnoreWhenCacheValue()
		{
			var result = await new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(new List<FileIndexItem>{new FileIndexItem("/test")}),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(new Dictionary<string, object>
					{
						{ManualBackgroundSyncService.ManualSyncCacheName + "/test", string.Empty}
					}), new FakeIWebLogger(), new FakeIUpdateBackgroundTaskQueue(),GetScope())
				.ManualSync("/test", string.Empty);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result);
		}

		[TestMethod]
		public async Task PushToSockets_ContainsValue()
		{
			var socket = new FakeIWebSocketConnectionsService();
			await new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(),
					socket,
					new FakeMemoryCache(
						new Dictionary<string, object>()), 
					new FakeIWebLogger(), new FakeIUpdateBackgroundTaskQueue(),GetScope())
				.PushToSockets(new List<FileIndexItem>{new FileIndexItem("/test.jpg")});

			Assert.IsTrue(socket.FakeSendToAllAsync[0].Contains("/test.jpg"));
		}
		
		
		[TestMethod]
		public void FilterBefore_OkShouldPass()
		{
			var result=  new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(
						new Dictionary<string, object>()), 
					new FakeIWebLogger(), new FakeIUpdateBackgroundTaskQueue(),
					GetScope())
				.FilterBefore(new List<FileIndexItem>{new FileIndexItem("/test.jpg")
				{
					Status = FileIndexItem.ExifStatus.Ok
				}});

			Assert.AreEqual(1,result.Count);
			Assert.AreEqual("/test.jpg",result[0].FilePath);
		}
		
		[TestMethod]
		public void FilterBefore_NotFoundShouldPass()
		{
			var result=  new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(new Dictionary<string, object>()), 
					new FakeIWebLogger(), 
					new FakeIUpdateBackgroundTaskQueue(),
					GetScope())
				.FilterBefore(new List<FileIndexItem>{new FileIndexItem("/test.jpg")
				{
					Status = FileIndexItem.ExifStatus.NotFoundSourceMissing
				}});

			Assert.AreEqual(1,result.Count);
			Assert.AreEqual("/test.jpg",result[0].FilePath);
		}
		
		[TestMethod]
		public void FilterBefore_OperationNotSupportedShouldIgnore()
		{
			var result=  new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(new Dictionary<string, object>()), 
					new FakeIWebLogger(),
					new FakeIUpdateBackgroundTaskQueue(),GetScope())
				.FilterBefore(new List<FileIndexItem>{new FileIndexItem("/test.jpg")
				{
					Status = FileIndexItem.ExifStatus.OperationNotSupported
				}});

			Assert.AreEqual(0,result.Count);
		}
		
		[TestMethod]
		public void FilterBefore_ShouldIgnoreHome()
		{
			var result=  new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(),
					new FakeIWebSocketConnectionsService(),
					new FakeMemoryCache(new Dictionary<string, object>()), 
					new FakeIWebLogger(), 
					new FakeIUpdateBackgroundTaskQueue(),GetScope())
				.FilterBefore(new List<FileIndexItem>{new FileIndexItem("/")
				{
					Status = FileIndexItem.ExifStatus.Ok
				}});

			Assert.AreEqual(0,result.Count);
		}
	}
}

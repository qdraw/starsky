using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public sealed class ManualBackgroundSyncServiceTest
	{
		private static IServiceScopeFactory GetScope()
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
					new SocketSyncUpdateService(new FakeIWebSocketConnectionsService(),
						new FakeINotificationQuery(), new FakeIWebLogger()),
					new FakeMemoryCache(new Dictionary<string, object>()),
					new FakeIWebLogger(),
					new FakeIUpdateBackgroundTaskQueue()
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
					new SocketSyncUpdateService(new FakeIWebSocketConnectionsService(),
						new FakeINotificationQuery(), new FakeIWebLogger()),
					new FakeMemoryCache(new Dictionary<string, object>()),
					new FakeIWebLogger(), new FakeIUpdateBackgroundTaskQueue())
				.ManualSync("/", string.Empty);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result);
		}

		[TestMethod]
		public async Task ManualSync_test()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache();

			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
				Verbose = false,
				DatabaseConnection = "ManualSync_test"
			};

			new SetupDatabaseTypes(appSettings, provider).BuilderDb();
			provider.AddSingleton(appSettings);
			provider.AddScoped<IQuery, Query>();
			provider.AddScoped<IWebLogger, FakeIWebLogger>();

			var buildServiceProvider = provider.BuildServiceProvider();
			var memoryCache = buildServiceProvider.GetService<IMemoryCache>();
			var query = buildServiceProvider.GetService<IQuery>();

			var cacheDbName = new Query(null!, null!,
				null, null!).CachingDbName(
				nameof(FileIndexItem) + "manual_sync", "/");
			memoryCache!.Remove(cacheDbName);

			var cachedContent = new List<FileIndexItem>
			{
				new FileIndexItem("/999_not_found_1.jpg")
			};
			memoryCache.Set(cacheDbName, cachedContent);
			await query!.AddItemAsync(new FileIndexItem("/999_not_found_1.jpg"));

			var item = new FakeSelectorStorage(
				new FakeIStorage(new List<string> { "/" },
					new List<string> { "/test2__1234.jpg", "/test3__1234.jpg" },
					new List<byte[]>
					{
						FakeCreateAn.CreateAnImageNoExif.Bytes.ToArray(),
						FakeCreateAn.CreateAnImageNoExif.Bytes.ToArray()
					}));

			await new ManualBackgroundSyncService(
					new Synchronize(appSettings, query, item, new FakeIWebLogger(),
						new FakeISyncAddThumbnailTable(), null, memoryCache),
					query,
					new SocketSyncUpdateService(new FakeIWebSocketConnectionsService(),
						new FakeINotificationQuery(), new FakeIWebLogger()),
					memoryCache,
					new FakeIWebLogger(),
					new FakeIUpdateBackgroundTaskQueue())
				.BackgroundTask("/");

			var content = query.DisplayFileFolders().Where(p => p.FilePath != "/").ToList();
			foreach ( var itemContent in content )
			{
				Console.WriteLine("Manual sync " + itemContent.FilePath);
			}

			Assert.AreEqual(1,
				content.Count(p => p.FilePath == "/test2__1234.jpg"));
			Assert.AreEqual(1,
				content.Count(p => p.FilePath == "/test3__1234.jpg"));
			Assert.AreEqual(2,
				content.Count(p => p.FilePath is "/test2__1234.jpg" or "/test3__1234.jpg"));
			// should not contain the cached item
			Assert.AreEqual(0,
				content.Count(p => p.FilePath is "/999_not_found_1.jpg"));
		}

		[TestMethod]
		public async Task ObjectStarted()
		{
			var result = await new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(new List<FileIndexItem> { new FileIndexItem("/test") }),
					new SocketSyncUpdateService(new FakeIWebSocketConnectionsService(),
						new FakeINotificationQuery(), new FakeIWebLogger()),
					new FakeMemoryCache(new Dictionary<string, object>()),
					new FakeIWebLogger(), new FakeIUpdateBackgroundTaskQueue())
				.ManualSync("/test", string.Empty);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result);
		}

		[TestMethod]
		public async Task IgnoreWhenCacheValue()
		{
			var result = await new ManualBackgroundSyncService(
					new FakeISynchronize(new List<FileIndexItem>()),
					new FakeIQuery(new List<FileIndexItem> { new FileIndexItem("/test") }),
					new SocketSyncUpdateService(new FakeIWebSocketConnectionsService(),
						new FakeINotificationQuery(), new FakeIWebLogger()),
					new FakeMemoryCache(new Dictionary<string, object>
					{
						{
							ManualBackgroundSyncService.ManualSyncCacheName + "/test", string.Empty
						}
					}), new FakeIWebLogger(), new FakeIUpdateBackgroundTaskQueue())
				.ManualSync("/test", string.Empty);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result);
		}

		[TestMethod]
		public async Task BackgroundTaskExceptionWrapper()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache();

			var buildServiceProvider = provider.BuildServiceProvider();
			var memoryCache = buildServiceProvider.GetService<IMemoryCache>();
			Assert.IsNotNull(memoryCache);

			var service = new ManualBackgroundSyncService(
				new FakeISynchronize(new List<FileIndexItem>()),
				null!,
				new SocketSyncUpdateService(new FakeIWebSocketConnectionsService(),
					new FakeINotificationQuery(), new FakeIWebLogger()),
				memoryCache,
				new FakeIWebLogger(),
				new FakeIUpdateBackgroundTaskQueue());

			service.CreateSyncLock("test");
			var hasCache1 = memoryCache.Get(
				ManualBackgroundSyncService.ManualSyncCacheName + "test");
			Assert.IsNotNull(hasCache1);

			var isException = false;
			try
			{
				// Should crash on null reference exception on query
				await service.BackgroundTaskExceptionWrapper("test", "1");
			}
			catch ( NullReferenceException )
			{
				isException = true;

				var hasCache = memoryCache.Get(
					ManualBackgroundSyncService.ManualSyncCacheName + "test");

				Assert.IsNull(hasCache);
			}

			Assert.IsTrue(isException);
		}
	}
}

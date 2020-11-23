using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starsky.foundation.sync.WatcherHelpers;
using starskytest.Controllers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherHelpers
{
	[TestClass]
	public class SyncWatcherConnectorTest
	{
		[TestMethod]
		public void Sync_CheckInput()
		{
			var sync = new FakeISynchronize();
			var appSettings = new AppSettings();
			var syncWatcherPreflight = new SyncWatcherConnector(new AppSettings(), sync, 
				new FakeIWebSocketConnectionsService(), new FakeIQuery());
			syncWatcherPreflight.Sync(
				new Tuple<string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), WatcherChangeTypes.Changed));

			Assert.AreEqual("/test", sync.Inputs[0].Item1);
		}

		[TestMethod]
		public void Sync_CheckInput_Socket()
		{
			var sync = new FakeISynchronize(new List<FileIndexItem>
			{
				new FileIndexItem("/test"){Status = FileIndexItem.ExifStatus.Ok}
			});
			var websockets = new FakeIWebSocketConnectionsService();
			var appSettings = new AppSettings();
			var syncWatcherPreflight = new SyncWatcherConnector(new AppSettings(), sync, websockets, new FakeIQuery());
			syncWatcherPreflight.Sync(
				new Tuple<string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), WatcherChangeTypes.Changed));

			Assert.AreEqual(1, websockets.FakeSendToAllAsync.Count);
			Assert.IsTrue(websockets.FakeSendToAllAsync[0].Contains("filePath\":\"/test\""));
			Assert.AreEqual("/test", sync.Inputs[0].Item1);
		}

		[TestMethod]
		public void Sync_CheckInput_Socket_Ignore()
		{
			var sync = new FakeISynchronize(new List<FileIndexItem>
			{
				new FileIndexItem("/test"){Status = FileIndexItem.ExifStatus.OperationNotSupported}
			});
			var websockets = new FakeIWebSocketConnectionsService();
			var appSettings = new AppSettings();
			var syncWatcherConnector = new SyncWatcherConnector(appSettings, sync, websockets, new FakeIQuery());
			syncWatcherConnector.Sync(
				new Tuple<string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), WatcherChangeTypes.Changed));

			Assert.AreEqual(0, websockets.FakeSendToAllAsync.Count);
			Assert.AreEqual("/test", sync.Inputs[0].Item1);
		}

		[TestMethod]
		public void Sync_CheckInput_CheckIfCacheIsUpdated()
		{
			var sync = new FakeISynchronize(new List<FileIndexItem>
			{
				new FileIndexItem("/test.jpg"){Status = FileIndexItem.ExifStatus.Ok}
			});
			var websockets = new FakeIWebSocketConnectionsService();
			var appSettings = new AppSettings();
			
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase(nameof(DownloadPhotoControllerTest));
			var options = builderDb.Options;
			var context = new ApplicationDbContext(options);

			var query = new Query(context, memoryCache);
				
			query.AddCacheParentItem("/", 
				new List<FileIndexItem>{new FileIndexItem("/test.jpg")
			{
				IsDirectory = false, 
				Tags = "This should not be the tags",
				ParentDirectory = "/"
			}});
			
			var syncWatcherConnector = new SyncWatcherConnector(appSettings, sync, websockets, query);
			syncWatcherConnector.Sync(
				new Tuple<string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test.jpg"), WatcherChangeTypes.Changed));

			Assert.AreEqual(string.Empty,query.SingleItem("/test.jpg").FileIndexItem.Tags);
		}
	}
}

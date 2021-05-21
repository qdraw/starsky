using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
		public async Task Sync_CheckInput()
		{
			var sync = new FakeISynchronize();
			var appSettings = new AppSettings();
			var syncWatcherPreflight = new SyncWatcherConnector(new AppSettings(), sync, 
				new FakeIWebSocketConnectionsService(), new FakeIQuery());
			await syncWatcherPreflight.Sync(
				new Tuple<string, string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), null, WatcherChangeTypes.Changed));

			Assert.AreEqual("/test", sync.Inputs[0].Item1);
		}
		
		[TestMethod]
		public async Task Sync_Rename()
		{
			var sync = new FakeISynchronize();
			var appSettings = new AppSettings();
			var syncWatcherPreflight = new SyncWatcherConnector(new AppSettings(), sync, 
				new FakeIWebSocketConnectionsService(), new FakeIQuery());
			var result = await syncWatcherPreflight.Sync(
				new Tuple<string, string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), Path.Combine(appSettings.StorageFolder, "test2"), WatcherChangeTypes.Renamed));

			Assert.AreEqual("/test", sync.Inputs[0].Item1);
			Assert.AreEqual("/test2", sync.Inputs[1].Item1);
			// result
			Assert.AreEqual("/test", result[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result[0].Status);
		}
		
				
		[TestMethod]
		public async Task Sync_Rename_skipNull()
		{
			var sync = new FakeISynchronize();
			var appSettings = new AppSettings();
			var syncWatcherPreflight = new SyncWatcherConnector(new AppSettings(), sync, 
				new FakeIWebSocketConnectionsService(), new FakeIQuery());
			var result = await syncWatcherPreflight.Sync(
				new Tuple<string, string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), null, WatcherChangeTypes.Renamed));

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
				new Tuple<string, string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), null, WatcherChangeTypes.Changed));

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
				new Tuple<string, string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), null, WatcherChangeTypes.Changed));

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
				new Tuple<string, string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test.jpg"), null, WatcherChangeTypes.Changed));

			Assert.AreEqual(string.Empty,query.SingleItem("/test.jpg").FileIndexItem.Tags);
		}
		
		[TestMethod]
		public void Sync_CheckInput_CheckIfCacheIsUpdated_ButIgnoreNotInIndexFile()
		{
			var sync = new FakeISynchronize(new List<FileIndexItem>
			{
				//    = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = > source is missing
				new FileIndexItem("/test.jpg"){Status = FileIndexItem.ExifStatus.NotFoundSourceMissing}
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
				new Tuple<string, string,  WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test.jpg"), null, WatcherChangeTypes.Changed));
			
			Assert.AreEqual(0, query.DisplayFileFolders().Count());
		}

		[TestMethod]
		public void FilterBefore_AllowedStatus()
		{
			var fileIndexItems = new List<FileIndexItem>
			{
				new FileIndexItem() {Status = FileIndexItem.ExifStatus.Deleted},
				new FileIndexItem() {Status = FileIndexItem.ExifStatus.Ok},
				new FileIndexItem() {Status = FileIndexItem.ExifStatus.NotFoundNotInIndex},
				new FileIndexItem() {Status = FileIndexItem.ExifStatus.NotFoundSourceMissing}
			};

			var result = new SyncWatcherConnector(null,null,
				null,null).FilterBefore(fileIndexItems);
			Assert.AreEqual(4,result.Count);
		}
	}
}

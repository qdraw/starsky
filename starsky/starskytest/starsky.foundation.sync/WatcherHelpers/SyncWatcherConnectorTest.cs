using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.consoletelemetry.Initializers;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.realtime.Interfaces;
using starsky.foundation.sync.SyncInterfaces;
using starsky.foundation.sync.WatcherHelpers;
using starsky.foundation.webtelemetry.Models;
using starskytest.Controllers;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.WatcherHelpers
{
	[TestClass]
	public class SyncWatcherConnectorTest
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task Sync_ArgumentException()
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			var syncWatcherPreflight = new SyncWatcherConnector(null,null!,null!,null!,null!,null);
			await syncWatcherPreflight.Sync(
				new Tuple<string, string, WatcherChangeTypes>("test", null, WatcherChangeTypes.Changed));
		}
		
		[TestMethod]
		public async Task Sync_CheckInput()
		{
			var sync = new FakeISynchronize();
			var appSettings = new AppSettings();
			var syncWatcherPreflight = new SyncWatcherConnector(new AppSettings(), sync, 
				new FakeIWebSocketConnectionsService(), 
				new FakeIQuery(), new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));
			await syncWatcherPreflight.Sync(
				new Tuple<string, string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), null, WatcherChangeTypes.Changed));

			Assert.AreEqual("/test", sync.Inputs[0].Item1);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public async Task Sync_InjectScopes_NullReferenceException()
		{
			var appSettings = new AppSettings();
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();
			var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var syncWatcherPreflight = new SyncWatcherConnector(scope);
			await syncWatcherPreflight.Sync(
				new Tuple<string, string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), null, WatcherChangeTypes.Changed));
		}

		[TestMethod]
		public async Task Sync_Rename()
		{
			var sync = new FakeISynchronize();
			var appSettings = new AppSettings();
			var syncWatcherPreflight = new SyncWatcherConnector(new AppSettings(), sync, 
				new FakeIWebSocketConnectionsService(), 
				new FakeIQuery(), new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));
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
				new FakeIWebSocketConnectionsService(), new FakeIQuery(), 
				new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));
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
			var syncWatcherPreflight = new SyncWatcherConnector(new AppSettings(),
				sync, websockets, new FakeIQuery(), new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));
			syncWatcherPreflight.Sync(
				new Tuple<string, string, WatcherChangeTypes>(
					Path.Combine(appSettings.StorageFolder, "test"), null, WatcherChangeTypes.Changed));

			Assert.AreEqual(1, websockets.FakeSendToAllAsync.Count(p => !p.StartsWith("[system]")));
			var value = websockets.FakeSendToAllAsync.FirstOrDefault(p =>
					!p.StartsWith("[system]"));
			Assert.IsTrue(value.Contains("filePath\":\"/test\""));
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
			var syncWatcherConnector = new SyncWatcherConnector(appSettings, 
				sync, websockets, new FakeIQuery(), new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));
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

			var query = new Query(context,null,null,null, memoryCache);
				
			query.AddCacheParentItem("/", 
				new List<FileIndexItem>{new FileIndexItem("/test.jpg")
				{
					IsDirectory = false, 
					Tags = "This should not be the tags",
					ParentDirectory = "/"
				}});
			
			var syncWatcherConnector = new SyncWatcherConnector(appSettings,
				sync, websockets, query, new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));
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

			var query = new Query(context,null,null,null, memoryCache);
				
			query.AddCacheParentItem("/", 
				new List<FileIndexItem>{new FileIndexItem("/test.jpg")
				{
					IsDirectory = false, 
					Tags = "This should not be the tags",
					ParentDirectory = "/"
				}});
			
			var syncWatcherConnector = new SyncWatcherConnector(appSettings,
				sync, websockets, query, new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));
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
				new FileIndexItem{ FilePath = "/1.jpg", Status = FileIndexItem.ExifStatus.Deleted},
				new FileIndexItem() { FilePath = "/2.jpg",Status = FileIndexItem.ExifStatus.Ok},
				new FileIndexItem() { FilePath = "/3.jpg",Status = FileIndexItem.ExifStatus.NotFoundNotInIndex},
				new FileIndexItem() { FilePath = "/4.jpg", Status = FileIndexItem.ExifStatus.NotFoundSourceMissing}
			};

			var result = new SyncWatcherConnector(null,null,
				null,null, new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration())).FilterBefore(fileIndexItems);
			Assert.AreEqual(4,result.Count);
		}
		
		[TestMethod]
		public void FilterBefore_AllowedStatus_removeDuplicates()
		{
			var fileIndexItems = new List<FileIndexItem>
			{
				new FileIndexItem{ FilePath = "/1.jpg", Status = FileIndexItem.ExifStatus.Ok},
				new FileIndexItem() { FilePath = "/1.jpg",Status = FileIndexItem.ExifStatus.Ok},

			};
			var result = new SyncWatcherConnector(null,null,
				null,null, new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration())).FilterBefore(fileIndexItems);
			Assert.AreEqual(1,result.Count);
		}
		
		[TestMethod]
		public void Sync_InjectScopes()
		{
			var services = new ServiceCollection();

			services.AddSingleton<ISynchronize, FakeISynchronize>();
			services.AddSingleton<AppSettings>();
			services.AddSingleton<IWebSocketConnectionsService, FakeIWebSocketConnectionsService>();
			services.AddSingleton<IQuery, FakeIQuery>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();
			services.AddMemoryCache();
				
			var serviceProvider = services.BuildServiceProvider();
			
			var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			
			var syncWatcherPreflight = new SyncWatcherConnector(scope);
			var result = syncWatcherPreflight.InjectScopes();
			
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void CreateNewRequestTelemetry_NoKey()
		{
			var connector = new SyncWatcherConnector(new AppSettings(), new FakeISynchronize(),
				new FakeIWebSocketConnectionsService(), new FakeIQuery(),
				new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));

			var operationHolder = connector.CreateNewRequestTelemetry();
			var operationHolder2 = operationHolder as EmptyOperationHolder<RequestTelemetry>;
			Assert.IsTrue(operationHolder2.Empty);
		}
		
		[TestMethod]
		public void CreateNewRequestTelemetry_Key()
		{
			var connector = new SyncWatcherConnector(new AppSettings{ ApplicationInsightsInstrumentationKey = "1"}, new FakeISynchronize(),
				new FakeIWebSocketConnectionsService(), new FakeIQuery(),
				new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));

			var operationHolder = connector.CreateNewRequestTelemetry();
			Assert.AreEqual("FSW SyncWatcherConnector", operationHolder.Telemetry.Name);
			var expected = new RequestTelemetry();
			new CloudRoleNameInitializer($"{new AppSettings().ApplicationType}").Initialize(expected);
			Assert.AreEqual(expected.Context.Cloud.RoleName, operationHolder.Telemetry.Context.Cloud.RoleName);
			Assert.AreEqual(expected.Context.Cloud.RoleInstance, operationHolder.Telemetry.Context.Cloud.RoleInstance);
			connector.EndRequestOperation(operationHolder);
		}
		
		[TestMethod]
		public void CreateNewRequestTelemetry_Key_NoTelemetryClient()
		{
			var connector = new SyncWatcherConnector(new AppSettings{ ApplicationInsightsInstrumentationKey = "1"}, new FakeISynchronize(),
				new FakeIWebSocketConnectionsService(), new FakeIQuery(),
				new FakeIWebLogger(), null); // <-- no tel client

			var operationHolder = connector.CreateNewRequestTelemetry();
			
			var operationHolder2 = operationHolder as EmptyOperationHolder<RequestTelemetry>;
			Assert.IsTrue(operationHolder2.Empty);
		}
		
		[TestMethod]
		public void EndRequestOperation_NoKey()
		{
			var connector = new SyncWatcherConnector(new AppSettings(), new FakeISynchronize(),
				new FakeIWebSocketConnectionsService(), new FakeIQuery(),
				new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));

			var result = connector.EndRequestOperation(new EmptyOperationHolder<RequestTelemetry>());
			Assert.IsFalse(result);
		}
		
		[TestMethod]
		public void EndRequestOperation_Key()
		{
			var connector = new SyncWatcherConnector(new AppSettings{ ApplicationInsightsInstrumentationKey = "1"}, new FakeISynchronize(),
				new FakeIWebSocketConnectionsService(), new FakeIQuery(),
				new FakeIWebLogger(), new TelemetryClient(new TelemetryConfiguration()));

			var operationHolder = connector.CreateNewRequestTelemetry();

			var result = connector.EndRequestOperation(operationHolder);

			Assert.IsTrue(result);
		}
		
				
		[TestMethod]
		public void EndRequestOperation_Key_NoTelemetryClient()
		{
			var connector = new SyncWatcherConnector(new AppSettings{ ApplicationInsightsInstrumentationKey = "1"}, new FakeISynchronize(),
				new FakeIWebSocketConnectionsService(), new FakeIQuery(),
				new FakeIWebLogger(), null); // <-- no tel client

			var operationHolder = connector.CreateNewRequestTelemetry();

			var result = connector.EndRequestOperation(operationHolder);

			Assert.IsFalse(result);
		}
	}
}

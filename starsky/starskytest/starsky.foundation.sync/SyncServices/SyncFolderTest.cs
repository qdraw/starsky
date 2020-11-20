using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public class SyncFolderTest
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly AppSettings _appSettings;
		private readonly FakeIQuery _query;

		public SyncFolderTest()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();
			_appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			services.AddScoped(p =>_appSettings);
			_query = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/folder_no_content/") {IsDirectory = true},
				new FileIndexItem("/folder_content") {IsDirectory = true},
				new FileIndexItem("/folder_content/test.jpg"),
				new FileIndexItem("/folder_content/test2.jpg"),
				new FileIndexItem("/") {IsDirectory = true}
			});
			services.AddScoped<IQuery, FakeIQuery>(p => _query);
			_serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		private IStorage GetStorage()
		{
			return new FakeIStorage(
				new List<string> {"/", "/test"}, 
				new List<string>
				{
					"/test1.jpg",
					"/test2.jpg",
					"/test3.jpg",
					"/test/test4.jpg"
				},
				new List<byte[]>
				{
					FakeCreateAn.CreateAnImage.Bytes,
					FakeCreateAn.CreateAnImageColorClass.Bytes,
					FakeCreateAn.CreateAnImageNoExif.Bytes,
					FakeCreateAn.CreateAnImage.Bytes
				});
		}
		
		[TestMethod]
		public async Task Dir_NotFound()
		{
			var result = await new SyncFolder(_appSettings, 
				_serviceScopeFactory,_query, new FakeSelectorStorage(GetStorage()), 
				new ConsoleWrapper()).Folder("/not_found");

			Assert.AreEqual("/not_found",result[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result[0].Status);
		}

		[TestMethod]
		public async Task FilesOnDiskButNotInTheDb()
		{
			var result = await new SyncFolder(_appSettings, 
				_serviceScopeFactory,_query, new FakeSelectorStorage(GetStorage()),
				new ConsoleWrapper()).Folder("/");
			
			Assert.AreEqual("/test1.jpg",result[0].FilePath);
			Assert.AreEqual("/test2.jpg",result[1].FilePath);
			Assert.AreEqual("/test3.jpg",result[2].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result[1].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result[2].Status);
			
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 
				_query.SingleItem("/test1.jpg").FileIndexItem.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 
				_query.SingleItem("/test2.jpg").FileIndexItem.Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, 
				_query.SingleItem("/test3.jpg").FileIndexItem.Status);
		}
		
		[TestMethod]
		public async Task InDbButNotOnDisk()
		{
			var query = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/folder_content") {IsDirectory = true},
				new FileIndexItem("/folder_content/test.jpg"),
				new FileIndexItem("/folder_content/test2.jpg"),
				new FileIndexItem("/") {IsDirectory = true}
			});
			
			var result = await new SyncFolder(_appSettings, 
				_serviceScopeFactory, query, new FakeSelectorStorage(GetStorage()),
				new ConsoleWrapper()).Folder("/folder_content");

			Assert.AreEqual("/folder_content/test.jpg",result[0].FilePath);
			Assert.AreEqual("/folder_content/test2.jpg",result[1].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result[0].Status);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result[1].Status);
			
			Assert.AreEqual(null, 
				query.SingleItem("/folder_content/test.jpg"));
			Assert.AreEqual(null, 
				query.SingleItem("/folder_content/test2.jpg"));
		}

		[TestMethod]
		public async Task FileSizeIsChanged()
		{
			var subPath = "/change/test_change.jpg";
			await _query.AddItemAsync(new FileIndexItem(subPath)
			{
				Size = 123456
			});
			
			var storage = GetStorage();
			await storage.WriteStreamAsync(new MemoryStream(FakeCreateAn.CreateAnImage.Bytes),
				subPath);
			
			var result = await new SyncFolder(_appSettings, 
				_serviceScopeFactory,_query, new FakeSelectorStorage(storage),
				new ConsoleWrapper()).Folder("/change");

			Assert.AreEqual(subPath,result[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result[0].Status);
			Assert.IsTrue(result[0].Size != 123456);
			Assert.IsFalse(string.IsNullOrWhiteSpace(result[0].Tags));
		}

		[TestMethod]
		public async Task ShouldAddFolderItSelfAndParentFolders()
		{
			var storage = GetStorage();
			var folderPath = "/should_add_root";
			storage.CreateDirectory(folderPath);

			var query = new FakeIQuery();
			var results = await new SyncFolder(_appSettings, 
				_serviceScopeFactory,query, new FakeSelectorStorage(storage),
				new ConsoleWrapper()).Folder(folderPath);

			Assert.IsNotNull(query.GetObjectByFilePathAsync("/"));
			Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual(folderPath, results[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,results[0].Status);
		}

		[TestMethod]
		public async Task AddParentFolder_NewFolders()
		{
			var storage = GetStorage();
			var folderPath = "/should_add_root2";
			storage.CreateDirectory(folderPath);

			var query = new FakeIQuery();
			var result = await new SyncFolder(_appSettings, 
				_serviceScopeFactory,query, new FakeSelectorStorage(storage),
				new ConsoleWrapper()).AddParentFolder(folderPath);

			Assert.IsNotNull(query.GetObjectByFilePathAsync("/"));
			Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
			Assert.AreEqual(folderPath, result.FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result.Status);
		}
		
		[TestMethod]
		public async Task AddParentFolder_ExistingFolder()
		{
			var storage = GetStorage();
			var folderPath = "/exist2";
			
			var query = new FakeIQuery(new List<FileIndexItem>{new FileIndexItem("/exist2")
			{
				IsDirectory = true
			}});
			
			var result = await new SyncFolder(_appSettings, 
				_serviceScopeFactory,query, new FakeSelectorStorage(storage),
				new ConsoleWrapper()).AddParentFolder(folderPath);

			Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
			Assert.AreEqual(folderPath, result.FilePath);

			// should not add duplicate content
			var allItems = await query.GetAllRecursiveAsync("/");
			
			Assert.AreEqual(1, allItems.Count);
			Assert.AreEqual(folderPath, allItems[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,allItems[0].Status);
		}

		[TestMethod]
		public async Task AddParentFolder_NotFound()
		{
			var storage = GetStorage();
			var folderPath = "/not-found";
			
			var query = new FakeIQuery();
			
			var result = await new SyncFolder(_appSettings, 
				_serviceScopeFactory,query, new FakeSelectorStorage(storage),
				new ConsoleWrapper()).AddParentFolder(folderPath);

			Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
			Assert.AreEqual(folderPath, result.FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result.Status);

			// should not add content
			var allItems = await query.GetAllRecursiveAsync("/");
			Assert.AreEqual(0, allItems.Count);
		}

		[TestMethod]
		public void PathsToUpdateInDatabase_FilesOnDiskButNotInTheDb()
		{
			var results = SyncFolder.PathsToUpdateInDatabase(
				new List<FileIndexItem>(), new List<string>
				{
					"/test.jpg",
				});

			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("/test.jpg", results[0]);
		}
		
		[TestMethod]
		public void PathsToUpdateInDatabase_InDbButNotOnDisk()
		{
			var results = SyncFolder.PathsToUpdateInDatabase(
				new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg")
				}, new string[0]);

			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("/test.jpg", results[0]);
		}
		
		[TestMethod]
		public void PathsToUpdateInDatabase_ExistBoth()
		{
			var results = SyncFolder.PathsToUpdateInDatabase(
				new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg")
				}, new List<string>
				{
					"/test.jpg",
				});

			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("/test.jpg", results[0]);
		}
	}
}

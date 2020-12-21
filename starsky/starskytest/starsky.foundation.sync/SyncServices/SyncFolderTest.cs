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
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices
{
	[TestClass]
	public class SyncFolderTest
	{
		private readonly AppSettings _appSettings;
		private readonly IQuery _query;

		public SyncFolderTest()
		{
			_appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			(_query, _) = CreateNewExampleData();
		}
		
		private Tuple<IQuery, IServiceScopeFactory> CreateNewExampleData()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			services.AddScoped(p =>_appSettings);
			var query = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/folder_no_content/") {IsDirectory = true},
				new FileIndexItem("/folder_content") {IsDirectory = true},
				new FileIndexItem("/folder_content/test.jpg"),
				new FileIndexItem("/folder_content/test2.jpg")
			});
			services.AddScoped<IQuery, FakeIQuery>(p => query);
			var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
			return new Tuple<IQuery, IServiceScopeFactory>(query, serviceScopeFactory);
		}
		
		private IStorage GetStorage()
		{
			return new FakeIStorage(
				new List<string>
				{
					"/", 
					"/test",
					"/folder_no_content"
				}, 
				new List<string>
				{
					"/test1.jpg",
					"/test2.jpg",
					"/test3.jpg",
					"/test/test4.jpg",
				},
				new List<byte[]>
				{
					CreateAnImage.Bytes,
					CreateAnImageColorClass.Bytes,
					CreateAnImageNoExif.Bytes,
					CreateAnImage.Bytes
				});
		}
		
		[TestMethod]
		public async Task Folder_Dir_NotFound()
		{
			var storage = new FakeIStorage();
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.Folder("/not_found");
			
			Assert.AreEqual("/not_found",result[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result[0].Status);
		}

		[TestMethod]
		public async Task Folder_FolderWithNoContent()
		{
			var storage = GetStorage();
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.Folder("/folder_no_content");

			Assert.AreEqual("/folder_no_content",result[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result[0].Status);
		}

		[TestMethod]
		public async Task Folder_FileSizeIsChanged()
		{
			var subPath = "/change/test_change.jpg";
			await _query.AddItemAsync(new FileIndexItem(subPath)
			{
				Size = 123456
			});
			
			var storage = GetStorage();
			await storage.WriteStreamAsync(new MemoryStream(FakeCreateAn.CreateAnImage.Bytes),
				subPath);
			
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.Folder("/change");

			Assert.AreEqual(subPath,result[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result[0].Status);
			Assert.IsTrue(result[0].Size != 123456);
			Assert.IsFalse(string.IsNullOrWhiteSpace(result[0].Tags));
		}
		
		[TestMethod]
		public async Task Folder_DuplicateChildItems()
		{
			var storage =  new FakeIStorage(
				new List<string>
				{
					"/", 
					"/Folder_Duplicate"
				}, 
				new List<string>
				{
					"/Folder_Duplicate/test.jpg",
				},
				new List<byte[]>
				{
					CreateAnImage.Bytes,
				});
			
			// yes this is duplicate!
			await _query.AddItemAsync(new FileIndexItem("/Folder_Duplicate/test.jpg"));
			await _query.AddItemAsync(new FileIndexItem("/Folder_Duplicate/test.jpg")); // yes this is duplicate!
			
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.Folder("/Folder_Duplicate");

			Assert.AreEqual(2, result.Count);
			var queryResult = await _query.GetAllFilesAsync("/Folder_Duplicate");
			Assert.AreEqual(1, queryResult.Count);

			await _query.RemoveItemAsync(queryResult[0]);
		}

		[TestMethod]
		public async Task Folder_ShouldAddFolderItSelfAndParentFolders()
		{
			var storage = GetStorage();
			var folderPath = "/should_add_root";
			storage.CreateDirectory(folderPath);

			var query = new FakeIQuery();

			var syncFolder = new SyncFolder(_appSettings, query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.Folder(folderPath);

			Assert.IsNotNull(query.GetObjectByFilePathAsync("/"));
			Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(folderPath, result[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok,result[0].Status);
		}

		[TestMethod]
		public async Task AddParentFolder_NewFolders()
		{
			var storage = GetStorage();
			var folderPath = "/should_add_root2";
			storage.CreateDirectory(folderPath);

			var query = new FakeIQuery();
			var syncFolder = new SyncFolder(_appSettings, query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.AddParentFolder(folderPath);

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
			
			var syncFolder = new SyncFolder(_appSettings, query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.AddParentFolder(folderPath);

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
			
			var syncFolder = new SyncFolder(_appSettings, query, new FakeSelectorStorage(storage),
				new ConsoleWrapper());
			var result = await syncFolder.AddParentFolder(folderPath);

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

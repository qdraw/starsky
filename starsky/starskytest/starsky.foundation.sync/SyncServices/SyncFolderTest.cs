using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
				SyncIgnore = new List<string>{"/.git"}
			};
			
			(_query, _) = CreateNewExampleData();
		}
		
		[SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeEvident")]
		private Tuple<IQuery, IServiceScopeFactory> CreateNewExampleData()
		{
			var services = new ServiceCollection();
			var serviceProvider = services.BuildServiceProvider();

			services.AddScoped(_ =>_appSettings);
			var query = new FakeIQuery(new List<FileIndexItem>
			{
				new FileIndexItem("/folder_no_content/") {IsDirectory = true},
				new FileIndexItem("/folder_content") {IsDirectory = true},
				new FileIndexItem("/folder_content/test.jpg"),
				new FileIndexItem("/folder_content/test2.jpg")
			});
			services.AddScoped<IQuery, FakeIQuery>(_ => query);
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
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			var result = await syncFolder.Folder("/not_found");
			
			Assert.AreEqual("/not_found",result[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result[0].Status);
		}

		[TestMethod]
		public async Task Folder_FolderWithNoContent()
		{
			var storage = GetStorage();
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
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
			await storage.WriteStreamAsync(new MemoryStream(CreateAnImage.Bytes),
				subPath);
			
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
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
			
			var queryResultBefore = await _query.GetAllFilesAsync("/Folder_Duplicate");
			Assert.AreEqual(2, queryResultBefore.Count);

			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			
			var result = (await syncFolder.Folder(
				"/Folder_Duplicate")).Where(p => p.FilePath != "/").ToList();

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
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
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
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			var result = await syncFolder.AddParentFolder(folderPath, null);

			Assert.IsNotNull(query.GetObjectByFilePathAsync("/"));
			Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
			Assert.AreEqual(folderPath, result?.FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result?.Status);
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
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			var result = await syncFolder.AddParentFolder(folderPath,null);

			Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
			Assert.AreEqual(folderPath, result?.FilePath);

			// should not add duplicate content
			var allItems = await query.GetAllRecursiveAsync();
			
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
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			var result = await syncFolder.AddParentFolder(folderPath, null);

			Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
			Assert.AreEqual(folderPath, result!.FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result.Status);

			// should not add content
			var allItems = await query.GetAllRecursiveAsync();
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
			Assert.AreEqual("/test.jpg", results[0].FilePath);
		}
		
		[TestMethod]
		public void PathsToUpdateInDatabase_InDbButNotOnDisk()
		{
			var results = SyncFolder.PathsToUpdateInDatabase(
				new List<FileIndexItem>
				{
					new FileIndexItem("/test.jpg")
				}, Array.Empty<string>());

			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("/test.jpg", results[0].FilePath);
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
			Assert.AreEqual("/test.jpg", results[0].FilePath);
		}
		
		[TestMethod]
		public void PathsToUpdateInDatabase_Duplicates()
		{
			var results = SyncFolder.PathsToUpdateInDatabase(
				new List<FileIndexItem>(), new List<string>
				{
					"/test.jpg",
					"/test.jpg"
				});

			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("/test.jpg", results[0].FilePath);
		}

		
				
		[TestMethod]
		public async Task Folder_DuplicateFolders_Implicit()
		{
			await _query.AddItemAsync(new FileIndexItem("/DuplicateFolder"){IsDirectory = true});
			// yes this is duplicate
			await _query.AddItemAsync(new FileIndexItem("/DuplicateFolder"){IsDirectory = true});

			var storage =
				new FakeIStorage(new List<string> {"/", "/DuplicateFolder"});
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			
			await syncFolder.Folder("/");

			var allFolders = _query.GetAllFolders();
			if ( allFolders == null )
			{
				throw new NullReferenceException(
					"all folder should not be null");
			}
			
			Assert.AreEqual("/", allFolders.FirstOrDefault(p => p.FilePath == "/")?.FilePath);
			Assert.AreEqual(1, allFolders.Count(p => p.FilePath == "/DuplicateFolder"));
		}
		
		[TestMethod]
		public async Task Folder_DuplicateFolders_Direct()
		{
			await _query.AddItemAsync(new FileIndexItem("/DuplicateFolder"){IsDirectory = true});
			// yes this is duplicate
			await _query.AddItemAsync(new FileIndexItem("/DuplicateFolder"){IsDirectory = true});

			var storage =
				new FakeIStorage(new List<string> {"/", "/DuplicateFolder"});
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			
			await syncFolder.Folder("/DuplicateFolder");

			var allFolders = _query.GetAllFolders().Where(p => p.FilePath == "/DuplicateFolder").ToList();
			if ( allFolders == null )
			{
				throw new NullReferenceException(
					"all folder should not be null");
			}
			
			Assert.AreEqual("/DuplicateFolder", allFolders.FirstOrDefault(p => p.FilePath == "/DuplicateFolder")?.FilePath);
			Assert.AreEqual(1, allFolders.Count(p => p.FilePath == "/DuplicateFolder"));

		}
		
		[TestMethod]
		public async Task Folder_ShouldIgnore()
		{
			var storage =  new FakeIStorage(
				new List<string>
				{
					"/", 
					"/test_ignore",
					"/test_ignore/ignore"
				}, 
				new List<string>
				{
					"/test_ignore/ignore/test1.jpg"
				},
				new List<byte[]>
				{
					CreateAnImage.Bytes,
					CreateAnImageColorClass.Bytes,
					CreateAnImageNoExif.Bytes,
				});
			
			var appSettings = new AppSettings{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase, 
				Verbose = true,
				SyncIgnore = new List<string>{"/test_ignore/ignore"}
			};
			
			var syncFolder = new SyncFolder(appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache());
			var result = await syncFolder.Folder("/test_ignore");
			
			Assert.AreEqual("/test_ignore/ignore/test1.jpg",result[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,result[0].Status);

			var files = await _query.GetAllFilesAsync("/test_ignore");

			Assert.AreEqual(0,files.Count);
		}
		
				
		[TestMethod]
		public async Task RemoveChildItems_Floating_items()
		{
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk3"){IsDirectory = true});
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk3/test.jpg"));
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk3/test_dir"){IsDirectory = true});
			await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk3/test_dir/test.jpg"));

			var storage = new FakeIStorage();
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(new Dictionary<string, object>()));

			var rootItem = await _query.GetObjectByFilePathAsync("/Folder_InDbButNotOnDisk3");
			var result = await syncFolder.RemoveChildItems(_query, rootItem);
			
			Assert.AreEqual("/Folder_InDbButNotOnDisk3", result.FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,result.Status);

			var data = await _query.GetAllRecursiveAsync("/Folder_InDbButNotOnDisk3");
			Assert.AreEqual(0, data.Count);
		}
		
		[TestMethod]
		public async Task CompareFolderListAndFixMissingFoldersTest_Ok()
		{
			var storage = new FakeIStorage(new List<string>{"/", "/2018", "/2018/02", "/2018/02/2018_02_01"});
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(new Dictionary<string, object>()));
			
			await syncFolder.CompareFolderListAndFixMissingFolders(
				new List<string>{"/", "/2018", "/2018/02", "/2018/02/2018_02_01"},
				new List<FileIndexItem>{new FileIndexItem("/2018")});
			
			Assert.AreEqual(null,(await _query.GetObjectByFilePathAsync("/2018")));
			Assert.AreEqual("/2018/02",(await _query.GetObjectByFilePathAsync("/2018/02")).FilePath);
			Assert.AreEqual("/2018/02/2018_02_01",(await _query.GetObjectByFilePathAsync("/2018/02/2018_02_01")).FilePath);
		}
		
		[TestMethod]
		public async Task CompareFolderListAndFixMissingFoldersTest_Ignored()
		{
			var storage = new FakeIStorage(new List<string>{"/", "/.git","/test"});
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(new Dictionary<string, object>()));
			
			await syncFolder.CompareFolderListAndFixMissingFolders(
				new List<string>{"/", "/.git"},
				new List<FileIndexItem>{new FileIndexItem("/")});
			
			Assert.AreEqual(null,(await _query.GetObjectByFilePathAsync("/.git")));
		}
		
		[TestMethod]
		public async Task CompareFolderListAndFixMissingFoldersTest_Ok_SameCount()
		{
			var storage = new FakeIStorage(new List<string>{"/", "/2018", "/2018/02", "/2018/02/2018_02_01"});
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(new Dictionary<string, object>()));
			
			await syncFolder.CompareFolderListAndFixMissingFolders(
				new List<string>{"/", "/2018", "/2018/02", "/2018/02/2018_02_01"},
				new List<FileIndexItem>{
					new FileIndexItem("/"),
					new FileIndexItem("/2018"), 
					new FileIndexItem("/2018/02"), 
					new FileIndexItem("/2018/02/2018_02_01")}
				);
			
			Assert.AreEqual(null,(await _query.GetObjectByFilePathAsync("/2018")));
			Assert.AreEqual(null,(await _query.GetObjectByFilePathAsync("/2018/02")));
			Assert.AreEqual(null,(await _query.GetObjectByFilePathAsync("/2018/02/2018_02_01")));
		}
		
		[TestMethod]
		public async Task CompareFolderListAndFixMissingFoldersTest_NotFound()
		{
			var storage = new FakeIStorage(new List<string>{"/"});
			var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
				new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(new Dictionary<string, object>()));
			
			await syncFolder.CompareFolderListAndFixMissingFolders(
				new List<string>{"/", "/2018", "/2018/02", "/2018/02/2018_02_01"},
				new List<FileIndexItem>{new FileIndexItem("/2018")});
			
			Assert.AreEqual(null,(await _query.GetObjectByFilePathAsync("/2018")));
			Assert.AreEqual(null,(await _query.GetObjectByFilePathAsync("/2018/02")));
			Assert.AreEqual(null,(await _query.GetObjectByFilePathAsync("/2018/02/2018_02_01")));
		}
	}
}

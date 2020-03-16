using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starskycore.Attributes;
using starskycore.Helpers;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using Query = starsky.foundation.query.Services.Query;
using SyncService = starskycore.Services.SyncService;

namespace starskytest.Services
{
    [TestClass]
    public class SyncServiceTest
    {
        
        public SyncServiceTest()
        {
            // Inject MemCache
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();
            // Activate dependency injection            
            var services = new ServiceCollection();
            // Add IConfig to DI
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            // Make example config in memory
            var newImage = new CreateAnImage();
            var dict = new Dictionary<string, string>
            {
                { "App:StorageFolder", newImage.BasePath },
                { "App:Verbose", "true" },
	            { "App:DatabaseType", "InMemoryDatabase"}
            };
            // Build Fake database
            var dbBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            dbBuilder.UseInMemoryDatabase(nameof(SyncService));
            var options = dbBuilder.Options;
            var context = new ApplicationDbContext(options);
            // Build Configuration
            var builder = new ConfigurationBuilder();        
            // Add example config to build
            builder.AddInMemoryCollection(dict);
            var configuration = builder.Build();
            // Inject as Poco Plain old cl class
            services.ConfigurePoco<AppSettings>(configuration.GetSection("App"));
            // build the config service
            var serviceProvider = services.BuildServiceProvider();
            // copy config to AppSettings as service to inject
            _appSettings = serviceProvider.GetRequiredService<AppSettings>();
            // Activate Query
            _query = new Query(context,memoryCache);
            
	        _iStorage = new StorageSubPathFilesystem(_appSettings);
            var readmeta = new ReadMeta(_iStorage,_appSettings);
            // Activate SyncService
	        var iStorage = new StorageSubPathFilesystem(_appSettings);
	        var storageSelector = new FakeSelectorStorage(iStorage);
            _syncService = new SyncService(_query,_appSettings,storageSelector);
        }

        private readonly Query _query;
        private readonly SyncService _syncService;
        private readonly AppSettings _appSettings;
	    private StorageSubPathFilesystem _iStorage;

	    [ExcludeFromCoverage]
        [TestMethod]
        public void SyncServiceAddFoldersToDatabaseTest()
        {
            var folder1 = new FileIndexItem
            {
                FileName = "test",
                ParentDirectory = "/folder99",
                IsDirectory = true
            };
            
            var folder1List = new List<string> {"/folder99/test"};
            _syncService.AddFoldersToDatabase(folder1List,new List<FileIndexItem>());
            //  Run twice to check if there are no duplicates
             _syncService.AddFoldersToDatabase(folder1List,new List<FileIndexItem> {folder1});
            
            var allItems = _query.GetAllRecursive("/folder99");
			var allItemsString = allItems.Select(p => p.FilePath).ToList();
	        
	        Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, allItems.FirstOrDefault().ImageFormat );
	        Assert.AreEqual("test", allItems.FirstOrDefault().FileName);


            CollectionAssert.AreEqual(allItemsString, folder1List);
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceRemoveOldFilePathItemsFromDatabaseTest()
        {
            var folder1 =  _query.AddItem(new FileIndexItem
            {
                FileName = "folder1",
                ParentDirectory = "/test/",
                IsDirectory = true
            });
            
            // Old folder does not exist anymore
            var folder2 = _query.AddItem(new FileIndexItem
            {
                FileName = "folder2",
                ParentDirectory = "/test/",
                IsDirectory = true
            });
            
            var databaseSubFolderList = new List<FileIndexItem> {folder1,folder2};
            var localSubFolderDbStyle = new List<string>{"/test/folder1"};
            _syncService.RemoveOldFilePathItemsFromDatabase(localSubFolderDbStyle, databaseSubFolderList, "/test");

            var output = new List<FileIndexItem> {folder1};
            var input = _query.GetAllRecursive("/test");
            
            CollectionAssert.AreEqual(input,output);
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceAddSubPathFolderTest()
        {
            // For the parent folders
            _syncService.AddSubPathFolder("/temp/dir/dir2/");
            var output = new List<string> {"/temp/dir"};
            var input = _query.DisplayFileFolders("/temp").Select(item => item.FilePath).ToList();
            
            CollectionAssert.AreEqual(input,output);

        }

	    [TestMethod]
	    public void SyncServiceCheckMd5Hash_change()
	    {
		    
		    var fakeStorage = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg","/toChange.jpg"},
			    new List<byte[]>{CreateAnImage.Bytes,CreateAnImage.Bytes});
		    
		    var fakeSelectorStorage = new FakeSelectorStorage(fakeStorage);
		    
		    var readmeta = new ReadMeta(fakeStorage);
		    // Set Initial database for this folder
		    new SyncService(_query,_appSettings,fakeSelectorStorage).SyncFiles("/",false);

		    var initalItem = _query.GetObjectByFilePath("/toChange.jpg");

			// update item with different bytes	 (CreateAnImageNoExif)  
		    fakeStorage = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg","/toChange.jpg"},
			    new List<byte[]>{CreateAnImage.Bytes,CreateAnImageNoExif.Bytes});
		    fakeSelectorStorage = new FakeSelectorStorage(fakeStorage);

		    // Run sync again
		    new SyncService(_query,_appSettings,fakeSelectorStorage).SyncFiles("/",false);

		    var updatedItem = _query.GetObjectByFilePath("/toChange.jpg");
		    
		    // Are not the same due change in file
		    Assert.AreNotEqual(initalItem.FileHash,updatedItem.FileHash);

		    var updatedTestItem = _query.GetObjectByFilePath("/test.jpg");

		    // are the same:
		    Assert.AreEqual(initalItem.FileHash,updatedTestItem.FileHash);
		    
	    }

	    [TestMethod]
	    public void SyncServiceSingleFileTest_FakeStorage_AddItem()
	    {
		    var fakeStorage = new FakeIStorage(new List<string>{"/"},new List<string>{"/test.jpg"},
			    new List<byte[]>{CreateAnImageNoExif.Bytes});
    
		    var fakeSelectorStorage = new FakeSelectorStorage(fakeStorage);

		    new SyncService(_query,_appSettings,fakeSelectorStorage).SyncFiles("/test.jpg",false);

		    var updatedItem = _query.GetObjectByFilePath("/test.jpg");
		    
		    Assert.AreEqual("/test.jpg",updatedItem.FilePath);
			Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg,updatedItem.ImageFormat);
		    Assert.AreEqual(false,updatedItem.IsDirectory);
		    Assert.AreEqual(updatedItem.FileHash.Length >= 5,true);

	    }

	    [TestMethod]
        public void SyncServiceSingleFileTest()
        {
            // Test to do a sync with one single file
            // used in importer or web api.
            var newImage = new CreateAnImage();

	        Console.WriteLine(_appSettings.StorageFolder);
            
            _appSettings.StorageFolder = newImage.BasePath;

            _syncService.SingleFile(newImage.DbPath);

            // Run twice >= result is one image in database
            var t = _syncService.SingleFile(newImage.DbPath);

            // todo: Need to check if there is only one image with the same name

            var all = _query.GetAllRecursive("/");

	        Console.WriteLine(">>>>>>itemInAll");
	        foreach ( var itemInAll in all )
	        {
		        Console.WriteLine(itemInAll.FilePath);
	        }
	        Console.WriteLine("<<<<<itemInAll");
	        

            var item = _query.SingleItem(newImage.DbPath).FileIndexItem;
            
	        Assert.AreEqual(item.ImageFormat,ExtensionRolesHelper.ImageFormat.jpg);
            Assert.AreEqual(item.FileHash.Length >= 5,true);
            _query.RemoveItem(item);
            
            // The Base Directory will be ignored
            Assert.AreEqual(_syncService.SingleFile(),SyncService.SingleFileSuccess.Ignore);

        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceDeletedSingleFileTest()
        {
            var newImage = new CreateAnImage();
            _appSettings.StorageFolder = newImage.BasePath;

            _query.AddItem(new FileIndexItem
            {
                FileName = "non-existing.jpg",
                //FilePath = "/non-existing.jpg",
                ParentDirectory = "/",
                IsDirectory = false
            });

            Assert.AreEqual(_syncService.Deleted("/non-existing.jpg"),true);
            
            // If file exist => ignore this one 
            Assert.AreEqual(_syncService.Deleted(newImage.DbPath),false);

        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceDeletedFolderTest()
        {
            var newImage = new CreateAnImage();
            _appSettings.StorageFolder = newImage.BasePath;

            _query.AddItem(new FileIndexItem
            {
                FileName = "non-existing-folder",
                //FilePath = "/non-existing-folder",
                ParentDirectory = "/",
                IsDirectory = true
            });
            
            // Test if deleted file
            _query.AddItem(new FileIndexItem
            {
                FileName = "non-existing.jpg",
               // FilePath = "non-existing-folder/non-existing.jpg",
                ParentDirectory = "/non-existing-folder",
                FileHash = "4444",
                IsDirectory =  false
            });
            
            _syncService.Deleted("/non-existing-folder");

            var nonExisting = _query.GetSubPathByHash("4444");
            Assert.AreEqual(nonExisting,null);
            
        }

        [TestMethod]
        public void SyncServiceFirstItemDirectoryTest() // Scans childfolders and add thumbnails
        {
            
            var createAnImage = new CreateAnImage();

            var existFullDir = createAnImage.BasePath + Path.DirectorySeparatorChar + "exist";
            if (!Directory.Exists(existFullDir))
            {
                Directory.CreateDirectory(existFullDir);
            }
            
            var testFileFullPath = existFullDir + Path.DirectorySeparatorChar +
                    createAnImage.DbPath.Replace("/", string.Empty);


            if (!File.Exists(testFileFullPath))
            {
                File.Copy(createAnImage.FullFilePath, testFileFullPath);                
            }

            _appSettings.StorageFolder = createAnImage.BasePath;
            
            // Add base folder
            _query.AddItem(new FileIndexItem
            {
                FileName = "exist",
                ParentDirectory = "/",
                IsDirectory = true
            });
            
            // Add Image
            _query.AddItem(new FileIndexItem
            {
                FileName = createAnImage.DbPath.Replace("/",string.Empty),
                ParentDirectory = "/" + "exist",
                IsDirectory = false
            });
	        
	        var fileHashCode = new FileHash(_iStorage).GetHashCode(createAnImage.DbPath);

            _syncService.FirstItemDirectory();

	        Console.WriteLine(createAnImage.BasePath);
            var queryItem = _query.GetObjectByFilePath("/exist");
            Assert.AreEqual(fileHashCode, queryItem.FileHash);
            
            File.Delete(testFileFullPath);
            _query.RemoveItem(queryItem);
            
        }

        [TestMethod]
        public void SyncServiceOrphanFolderTest()
        {
            var newImage = new CreateAnImage();
            _appSettings.StorageFolder = _query.SubPathSlashRemove(newImage.BasePath);
            
            // Add Image
            _query.AddItem(new FileIndexItem
            {
	            Id = 2023,
                FileName = "test.jpg",
                ParentDirectory = "/deletedFolder",
                FileHash = "SyncServiceOrphanFolderTestDeletedFile",
                IsDirectory = false
            });

	        Assert.AreEqual("/deletedFolder/test.jpg", _query.GetSubPathByHash("SyncServiceOrphanFolderTestDeletedFile"));

	        // Reset the hashed cache list 
	        _query.ResetItemByHash("SyncServiceOrphanFolderTestDeletedFile");

            _syncService.OrphanFolder("/");
            
            Assert.AreEqual(null, _query.GetSubPathByHash("SyncServiceOrphanFolderTestDeletedFile"));
   
	        //all
	        // Cleanup the database
	        var all = _query.GetAllRecursive("/");
	        foreach ( var itemInAll in all )
	        {
		        Console.WriteLine($"...itemInAll: {itemInAll.FilePath} {itemInAll.Id}");
		        _query.RemoveItem(itemInAll);
	        }
        }
        
        [TestMethod]
        [ExpectedException(typeof(ConstraintException))]
        public void SyncServiceOrphanFolder_ToLarge_Test()
        {
            _syncService.OrphanFolder("/",-1); // always fail
        }



        [TestMethod]
        public void SyncService_DuplicateContentInDatabase_Test()
        {
	        // Cleanup the database
	        var all = _query.GetAllRecursive("/");
	        foreach ( var itemInAll in all )
	        {
		        Console.WriteLine($"itemInAll: {itemInAll.FilePath} {itemInAll.Id}");
		        _query.RemoveItem(itemInAll);
	        }
	        
            var createAnImage = new CreateAnImage();
            var testjpg = new FileIndexItem
            {
                Id = 905,
                FileName = createAnImage.DbPath.Replace("/",string.Empty),
                ParentDirectory = "/",
                IsDirectory = false
            };

            _query.AddItem(testjpg);


			var testjpg2 = new FileIndexItem
			{
				Id = 906,
				FileName = createAnImage.DbPath.Replace("/", string.Empty),
				ParentDirectory = "/",
				IsDirectory = false
			};
	        _query.AddItem(testjpg2);


            // this query is before syncing the api
            var inputWithoutSync = _query.GetAllFiles("/");
            Assert.AreEqual(2,inputWithoutSync.Count(p => p.FilePath == createAnImage.DbPath));

            // do a sync
            _syncService.SyncFiles("/");
            var outputWithSync = _query.GetAllFiles("/");
	        
            // test if the sync is working
            Assert.AreEqual(1,outputWithSync.Count(p => p.FilePath == createAnImage.DbPath));
	        
        }
        
        [TestMethod]
        public void SyncService_Duplicate_Folders_Directories_InDatabase_Test()
        {
            _appSettings.Verbose = true;
            
            var createAnImage = new CreateAnImage();
            _appSettings.StorageFolder = createAnImage.BasePath; // needs to have an / or \ at the end

            var existFullDir = createAnImage.BasePath + Path.DirectorySeparatorChar + "exist";
            if (!Directory.Exists(existFullDir))
            {
                Directory.CreateDirectory(existFullDir);
            }
            
            var existFolder = new FileIndexItem
            {
                Id = 500,
                FileName = "exist",
                ParentDirectory = "/",
                IsDirectory = true
            };
            _query.AddItem(existFolder);
            
            existFolder.Id++;
            _query.AddItem(existFolder);
            
            existFolder.Id++;
            _query.AddItem(existFolder);
            
            // this query is before syncing the api
            var inputWithoutSync = _query.GetAllRecursive();
            Assert.AreEqual(true,inputWithoutSync.Count(p => p.FilePath == "/exist") >= 2);

            var inputWithoutSync1 = _query.GetAllRecursive().Where(
                p => p.FilePath == "/exist" 
                     && !p.FilePath.Contains("/exist/")
            ).ToList();
            
            // do a sync
            _syncService.SyncFiles("/");

            var outputWithSync = _query.GetAllRecursive();

            // test if the sync is working
            Assert.AreEqual(1,outputWithSync.Count(
                p => p.FilePath == "/exist" 
                && !p.FilePath.Contains("/exist/")
            ));
            
            // remove item
            _query.RemoveItem(outputWithSync.FirstOrDefault(p => p.FilePath == "/exist"));
        }

    }
}

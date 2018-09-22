using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Data;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;

namespace starskytests
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
                { "App:Verbose", "true" }
            };
            // Build Fake database
            var dbBuilder = new     DbContextOptionsBuilder<ApplicationDbContext>();
            dbBuilder.UseInMemoryDatabase("test");
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
            
            var readmeta = new ReadMeta(_appSettings);
            // Activate SyncService
            _syncservice = new SyncService(context, _query,_appSettings,readmeta);
        }

        private readonly Query _query;
        private readonly SyncService _syncservice;
        private readonly AppSettings _appSettings;

        [ExcludeFromCoverage]
        [TestMethod]
        public void SyncServiceAddFoldersToDatabaseTest()
        {
            var folder1 = new FileIndexItem
            {
                FileName = "test",
                //FilePath = "/folder99/test",
                ParentDirectory = "/folder99",
                IsDirectory = true
            };
            
            var folder1List = new List<string> {"/folder99/test"};
            _syncservice.AddFoldersToDatabase(folder1List,new List<FileIndexItem>());
            //  Run twice to check if there are no duplicates
             _syncservice.AddFoldersToDatabase(folder1List,new List<FileIndexItem> {folder1});
            
            var allItems = _query.GetAllRecursive("/folder99").Select(p => p.FilePath).ToList();

            CollectionAssert.AreEqual(allItems, folder1List);
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceRemoveOldFilePathItemsFromDatabaseTest()
        {
            var folder1 =  _query.AddItem(new FileIndexItem
            {
                FileName = "folder1",
                //FilePath = "/test/folder1",
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
            _syncservice.RemoveOldFilePathItemsFromDatabase(localSubFolderDbStyle, databaseSubFolderList, "/test");

            var output = new List<FileIndexItem> {folder1};
            var input = _query.GetAllRecursive("/test");
            
            CollectionAssert.AreEqual(input,output);
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceAddSubPathFolderTest()
        {
            // For the parent folders
            _syncservice.AddSubPathFolder("/temp/dir/dir2/");
            var output = new List<string> {"/temp/dir"};
            var input = _query.DisplayFileFolders("/temp").Select(item => item.FilePath).ToList();
            
            CollectionAssert.AreEqual(input,output);

        }
        
//        [TestMethod]
//        [ExcludeFromCoverage]
//        public void SyncServiceCheckMd5HashTest()
//        {
//            string path = "hashing-file-test.tmp";
//
//            var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;
//
//            AppSettingsProvider.BasePath = basePath;
//            
//            Thumbnail.CreateErrorLogItem(path);
//            
//            var input = new List<string> {"/_hashing-file-test.tmp"};
//            
//            var folder2 = _query.AddItem(new FileIndexItem
//            {
//                FileName = "_hashing-file-test.tmp",
//                //FilePath = "/_hashing-file-test.tmp",
//                ParentDirectory = "/",
//                Tags = "!delete!",
//                IsDirectory = false
//            });
//
//            FileIndexItem.DatabasePathToFilePath("_hashing-file-test.tmp");
//            
//            var localHash = FileHash.GetHashCode(FileIndexItem.DatabasePathToFilePath("_hashing-file-test.tmp"));
//            var localHashInList = new List<string> {FileHash.GetHashCode(FileIndexItem.DatabasePathToFilePath("_hashing-file-test.tmp"))}.FirstOrDefault();
//            
//            Assert.AreEqual(localHash,localHashInList);
//
//            
//            var databaseList = new List<FileIndexItem> {folder2};
//            _syncservice.CheckMd5Hash(input,databaseList);
//
//            var outputFileIndex = _query.SingleItem("/_hashing-file-test.tmp").FileIndexItem;
//            var output = new List<FileIndexItem> {outputFileIndex}.Select(p => p.FilePath).ToList();
//           
//            CollectionAssert.AreEqual(output,input);
//
//            // Clean // add underscore
//            var fullPath = basePath + "_" + path;
//            if (File.Exists(fullPath))
//            {
//                File.Delete(fullPath);
//            }  
//            
//        }

        [TestMethod]
        public void SyncServiceSingleFileTest()
        {
            // Test to do a sync with one single file
            // used in importer or web api.
            var newImage = new CreateAnImage();
            
            _appSettings.StorageFolder = newImage.BasePath;

            _syncservice.SingleFile(newImage.DbPath);

            // Run twice >= result is one image in database
            var t = _syncservice.SingleFile(newImage.DbPath);

            // todo: Need to check if there is only one image with the same name

            var all = _query.GetAllRecursive("/");
            
            var item = _query.SingleItem(newImage.DbPath).FileIndexItem;
            
            Assert.AreEqual(item.FileHash.Length >= 5,true);
            _query.RemoveItem(item);
            
            // The Base Directory will be ignored
            Assert.AreEqual(_syncservice.SingleFile(),SyncService.SingleFileSuccess.Ignore);

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

            Assert.AreEqual(_syncservice.Deleted("/non-existing.jpg"),true);
            
            // If file exist => ignore this one 
            Assert.AreEqual(_syncservice.Deleted(newImage.DbPath),false);

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
            
            _syncservice.Deleted("/non-existing-folder");

            var nonExisting = _query.GetItemByHash("4444");
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
            var expectThisHashCode = FileHash.GetHashCode(createAnImage.FullFilePath);
            
            _syncservice.FirstItemDirectory();
            

            var queryItem = _query.GetObjectByFilePath("/exist");
            Assert.AreEqual(expectThisHashCode, queryItem.FileHash);
            
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
                FileName = "test.jpg",
                ParentDirectory = "/deletedFolder",
                FileHash = "deletedFile",
                IsDirectory = false
            });

            Assert.AreNotEqual(_query.GetItemByHash("deletedFile"),null);
       
            _syncservice.OrphanFolder("/");
            
            Assert.AreEqual(_query.GetItemByHash("deletedFile"),null);
   
        }
        
        [TestMethod]
        [ExpectedException(typeof(ConstraintException))]
        public void SyncServiceOrphanFolder_ToLarge_Test()
        {
            _syncservice.OrphanFolder("/",-1); // always fail
        }

        [TestMethod]
        public void SyncServiceRenameListItemsToDbStyleTest()
        {
            var newImage = new CreateAnImage();
            _appSettings.StorageFolder = newImage.BasePath; // needs to have an / or \ at the end
            var inputList = new List<string>{ Path.DirectorySeparatorChar.ToString() };
            var expectedOutputList = new List<string>{ "/"};
            var output = _syncservice.RenameListItemsToDbStyle(inputList);
            // list of files names that are starting with a filename (and not an / or \ )

            CollectionAssert.AreEqual(expectedOutputList,output);
        }

        [TestMethod]
        public void SyncService_DuplicateContentInDatabase_Test()
        {
            var createAnImage = new CreateAnImage();
            var testjpg = new FileIndexItem
            {
                Id = 300,
                FileName = createAnImage.DbPath.Replace("/",string.Empty),
                ParentDirectory = "/",
                IsDirectory = false
            };

            _query.AddItem(testjpg);
            testjpg.Id++;
            _query.AddItem(testjpg);

            // this query is before syncing the api
            var inputWithoutSync = _query.GetAllFiles();
            Assert.AreEqual(2,inputWithoutSync.Count(p => p.FilePath == createAnImage.DbPath));

            // do a sync
            _syncservice.SyncFiles("/");
            var outputWithSync = _query.GetAllFiles();

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
            _syncservice.SyncFiles("/");

            var outputWithSync = _query.GetAllRecursive();

            var outputWithSync1 = _query.GetAllRecursive().Where(
                p => p.FilePath == "/exist" 
                     && !p.FilePath.Contains("/exist/")
            ).ToList();

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

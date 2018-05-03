using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Data;
using starsky.Models;
using starsky.Services;

namespace starskytests
{
    [TestClass]
    public class SyncServiceTest
    {
        public SyncServiceTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context);
            _syncservice = new SyncService(context, _query);
        }

        private readonly Query _query;
        private readonly SyncService _syncservice;

        [ExcludeFromCoverage]
        [TestMethod]
        public void SyncServiceAddFoldersToDatabaseTest()
        {
            var folder1 = new FileIndexItem
            {
                FileName = "test",
                FilePath = "/folder99/test",
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
                FilePath = "/test/folder1",
                ParentDirectory = "/test/",
                IsDirectory = true
            });
            
            // Old folder does not exist anymore
            var folder2 = _query.AddItem(new FileIndexItem
            {
                FileName = "folder2",
                FilePath = "/test/folder2",
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
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceCheckMd5HashTest()
        {
            string path = "hashing-file-test.tmp";

            var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;

            AppSettingsProvider.BasePath = basePath;
            
            Thumbnail.CreateErrorLogItem(path);
            
            var input = new List<string> {"/_hashing-file-test.tmp"};
            
            var folder2 = _query.AddItem(new FileIndexItem
            {
                FileName = "_hashing-file-test.tmp",
                FilePath = "/_hashing-file-test.tmp",
                ParentDirectory = "/",
                Tags = "!delete!",
                IsDirectory = false
            });

            FileIndexItem.DatabasePathToFilePath("_hashing-file-test.tmp");
            
            var localHash = FileHash.GetHashCode(FileIndexItem.DatabasePathToFilePath("_hashing-file-test.tmp"));
            var localHashInList = new List<string> {FileHash.GetHashCode(FileIndexItem.DatabasePathToFilePath("_hashing-file-test.tmp"))}.FirstOrDefault();
            
            Assert.AreEqual(localHash,localHashInList);

            
            var databaseList = new List<FileIndexItem> {folder2};
            _syncservice.CheckMd5Hash(input,databaseList);

            var outputFileIndex = _query.SingleItem("/_hashing-file-test.tmp").FileIndexItem;
            var output = new List<FileIndexItem> {outputFileIndex}.Select(p => p.FilePath).ToList();
           
            CollectionAssert.AreEqual(output,input);

            
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceSingleFileTest()
        {
            var newImage = new CreateAnImage();
            AppSettingsProvider.BasePath = newImage.BasePath;

            _syncservice.SingleFile(newImage.DbPath);

            // Run twice >= result is one image in database
            _syncservice.SingleFile(newImage.DbPath);

            // todo: Need to check if there is only one image with the same name
                
            var item = _query.SingleItem(newImage.DbPath).FileIndexItem;
            Assert.AreEqual(item.FileHash.Length >= 5,true);
            _query.RemoveItem(item);
            
            // The Base Directory will be ignored
            Assert.AreEqual(_syncservice.SingleFile(),false);

        }
        
        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceDeletedSingleFileTest()
        {
            var newImage = new CreateAnImage();
            AppSettingsProvider.BasePath = newImage.BasePath;

            _query.AddItem(new FileIndexItem
            {
                FileName = "non-existing.jpg",
                FilePath = "/non-existing.jpg",
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
            AppSettingsProvider.BasePath = newImage.BasePath;

            _query.AddItem(new FileIndexItem
            {
                FileName = "non-existing-folder",
                FilePath = "/non-existing-folder",
                ParentDirectory = "/",
                IsDirectory = true
            });
            
            // Test if deleted file
            _query.AddItem(new FileIndexItem
            {
                FileName = "non-existing.jpg",
                FilePath = "non-existing-folder/non-existing.jpg",
                ParentDirectory = "/non-existing-folder",
                FileHash = "4444",
                IsDirectory =  false
            });
            
            _syncservice.Deleted("/non-existing-folder");

            var nonExisting = _query.GetItemByHash("4444");
            Assert.AreEqual(nonExisting,null);
            
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceFirstItemDirectoryTest() // Scans childfolders and add thumbnails
        {
            // Give some errors in vsts
            
            var newImage = new CreateAnImage();
            
            var split = newImage.BasePath.Split(Path.DirectorySeparatorChar);
            AppSettingsProvider.BasePath = string.Empty;
            var testfolder = string.Empty;

            var expectThisHashCode = FileHash.GetHashCode(newImage.FullFilePath);

            for (var i = 0; i < split.Length; i++)
            {
                if (i != split.Length - 2)
                {
                    AppSettingsProvider.BasePath += split[i];
                    if (i != split.Length - 1)
                    {
                        AppSettingsProvider.BasePath += Path.DirectorySeparatorChar;
                    }
                    continue;
                }
                // save the last folder to test
                testfolder = split[i];
            }

            Console.WriteLine("AppSettingsProvider.BasePath");
            Console.WriteLine(AppSettingsProvider.BasePath);

            // Add base folder
            _query.AddItem(new FileIndexItem
            {
                FileName = testfolder,
                FilePath = "/"+ testfolder,
                ParentDirectory = "/",
                IsDirectory = true
            });
            
            // Add Image
            _query.AddItem(new FileIndexItem
            {
                FileName = "test.jpg",
                FilePath = "/"+ testfolder + newImage.DbPath,
                ParentDirectory = "/" + testfolder,
                IsDirectory = false
            });

            Console.WriteLine("\"/\"+ testfolder + newImage.DbPath");
            Console.WriteLine("/"+ testfolder + newImage.DbPath);
            
            _syncservice.FirstItemDirectory();

            var queryItem = _query.GetObjectByFilePath("/" + testfolder);
            Assert.AreEqual(expectThisHashCode, queryItem.FileHash);
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceOrphanFolderTest()
        {
            var newImage = new CreateAnImage();
            AppSettingsProvider.BasePath = _query.SubPathSlashRemove(newImage.BasePath);
            
            // Add Image
            _query.AddItem(new FileIndexItem
            {
                FileName = "test.jpg",
                FilePath = "/deletedFolder/test.jpg",
                ParentDirectory = "/deletedFolder",
                FileHash = "deletedFile",
                IsDirectory = false
            });

            Assert.AreNotEqual(_query.GetItemByHash("deletedFile"),null);
       
            _syncservice.OrphanFolder("/");
            
            Assert.AreEqual(_query.GetItemByHash("deletedFile"),null);
   
        }

        [TestMethod]
        [ExcludeFromCoverage]
        public void SyncServiceRenameListItemsToDbStyleTest()
        {
            var newImage = new CreateAnImage();
            AppSettingsProvider.BasePath = newImage.BasePath; // needs to have an / or \ at the end
            var inputList = new List<string>{newImage.FullFilePath};
            var expectedOutputList = new List<string>{newImage.DbPath.Replace(Path.DirectorySeparatorChar.ToString(),"")};
            var output = _syncservice.RenameListItemsToDbStyle(inputList);
            // list of files names that are starting with a filename (and not an / or \ )

            CollectionAssert.AreEqual(expectedOutputList,output);
        }
        
    }
}

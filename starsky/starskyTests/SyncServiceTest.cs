using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                Tags = "!delete!"
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

        
        
        

    }
}

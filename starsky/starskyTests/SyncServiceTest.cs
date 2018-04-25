using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        [TestMethod]
        public void SyncServiceAddFoldersToDatabaseTest()
        {
            var folder1 = new FileIndexItem
            {
                FileName = "folder1",
                FilePath = "/folder1",
                ParentDirectory = "/",
                IsDirectory = true
            };
            
            var folder1List = new List<string> {"/folder1"};
            _syncservice.AddFoldersToDatabase(folder1List,new List<FileIndexItem>());
            // Run twice to check if there are no duplicates
            _syncservice.AddFoldersToDatabase(folder1List,new List<FileIndexItem> {folder1});
            
            var allItems = _query.GetAllRecursive().Select(p => p.FilePath).ToList();

            CollectionAssert.AreEqual(allItems, folder1List);
        }

        [TestMethod]
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
        public void SyncServiceAddSubPathFolderTest()
        {
            var tempFolder = _query.AddItem(new FileIndexItem
            {
                FileName = "temp",
                FilePath = "/temp",
                ParentDirectory = "/",
                IsDirectory = true
            });
            
            _syncservice.AddSubPathFolder("/temp/dir/dir2");
            
            var output = new List<string> {"/temp/dir","/temp/dir/dir2"};
            var input = _query.GetAllRecursive();
            // .Select(item => item.FilePath)
            
            CollectionAssert.AreEqual(input,output);

        }
        

        
        

    }
}

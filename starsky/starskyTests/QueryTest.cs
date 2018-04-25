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
    public class QueryTest
    {
        public QueryTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _query = new Query(context);
//            _syncservice = new SyncService(context, _query);
        }

        private readonly Query _query;
//        private readonly SyncService _syncservice;

        [TestMethod]
        public void QueryAddSingleItemReadReadAllBasicTest()
        {

            var hiJpgInput = _query.AddItem(new FileIndexItem
            {
                FileName = "hi.jpg",
                FilePath = "/hi.jpg",
                ParentDirectory = "/",
                FileHash = "09876543456789"
            });

            var hiJpgOutput = _query.SingleItem(hiJpgInput.FilePath).FileIndexItem;
            
            Assert.AreEqual(hiJpgInput,hiJpgOutput);
            
            var hi2JpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi2.jpg",
                FilePath = "/hi2.jpg",
                Tags = "!delete!",
                ParentDirectory = "/",
            });
            
            var hi2SubfolderJpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi2.jpg",
                FilePath = "/subfolder/hi2.jpg",
                ParentDirectory = "/subfolder",
                FileHash = "234567876543"
            });
            
            // Test root folder ("/)
            var getAllFilesExpectedResult = new List<FileIndexItem> {hiJpgInput, hi2JpgInput};

            var getAllResult = _query.GetAllFiles();

            CollectionAssert.AreEqual(getAllFilesExpectedResult,getAllResult);
            
            // Test subfolder
            var getAllFilesSubFolderExpectedResult = new List<FileIndexItem> {hi2SubfolderJpgInput};

            var getAllResultSubfolder = _query.GetAllFiles("/subfolder");
            CollectionAssert.AreEqual(getAllFilesSubFolderExpectedResult,getAllResultSubfolder);
            
            // GetAllRecursive
            var getAllRecursiveExpectedResult = new List<FileIndexItem> {hiJpgInput, hi2JpgInput, hi2SubfolderJpgInput};
            var getAllRecursive = _query.GetAllRecursive();
            CollectionAssert.AreEqual(getAllRecursive,getAllRecursiveExpectedResult);
            
            
            // GetItemByHash
            // See above for objects
            Assert.AreEqual(_query.GetItemByHash("09876543456789"), "/hi.jpg");

            // SubPathSlashRemove
            Assert.AreEqual(_query.SubPathSlashRemove("/test/"), "/test");

        }

        [TestMethod]
        public void DisplayFileFoldersTest()
        {
            var hiJpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi.jpg",
                FilePath = "/display/hi.jpg",
                ParentDirectory = "/display", // without slash
                FileHash = "123458465522",
                ColorClass = FileIndexItem.Color.Winner // 1
            });
            
            var hi3JpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi3.jpg",
                FilePath = "/display/hi3.jpg",
                ParentDirectory = "/display", // without slash
                FileHash = "78539048765",
                ColorClass = FileIndexItem.Color.Extras
            });
            
            var hi2JpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi2.jpg",
                FilePath = "/display/hi2.jpg",
                ParentDirectory = "/display",
                FileHash = "98765432123456",
                Tags = "!delete!"
            });
            
            // All Color Classes
            var getDisplayExpectedResult = new List<FileIndexItem> {hiJpgInput,hi3JpgInput};
            var getDisplay = _query.DisplayFileFolders("/display").ToList();
            
            CollectionAssert.AreEqual(getDisplayExpectedResult,getDisplay);
         
            // Compare filter
            var getDisplayExpectedResultSuperior = new List<FileIndexItem> {hiJpgInput};
            var colorClassFilterList = new FileIndexItem().GetColorClassList("1");
                
            var getDisplaySuperior = _query.DisplayFileFolders("/display",colorClassFilterList).ToList();
           
            CollectionAssert.AreEqual(getDisplayExpectedResultSuperior,getDisplaySuperior);

        }
        
    }
}

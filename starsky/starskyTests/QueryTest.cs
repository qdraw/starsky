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
        }

        private readonly Query _query;

        [TestMethod]
        public void QueryAddSingleItemReadReadAllBasicTest()
        {

            var hiJpgInput = _query.AddItem(new FileIndexItem
            {
                FileName = "hi.jpg",
                FilePath = "/hi.jpg",
                ParentDirectory = "/",
                FileHash = "09876543456789",
                ColorClass = FileIndexItem.Color.Winner // 1
            });

            var hiJpgOutput = _query.SingleItem(hiJpgInput.FilePath).FileIndexItem;
            
            Assert.AreEqual(hiJpgInput,hiJpgOutput);
            
            var hi2JpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi2.jpg",
                FilePath = "/hi2.jpg",
                Tags = "!delete!",
                ParentDirectory = "/"
            });
            
            var hi3JpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi3.jpg",
                FilePath = "/hi3.jpg",
                ParentDirectory = "/",
                ColorClass = FileIndexItem.Color.Trash // 9
            });
            
            var hi4JpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi4.jpg",
                FilePath = "/hi4.jpg",
                ParentDirectory = "/",
                ColorClass = FileIndexItem.Color.Winner // 1
            });
            
            var hi2SubfolderJpgInput =  _query.AddItem(new FileIndexItem
            {
                FileName = "hi2.jpg",
                FilePath = "/subfolder/hi2.jpg",
                ParentDirectory = "/subfolder",
                FileHash = "234567876543"
            });
            
            // Test root folder ("/)
            var getAllFilesExpectedResult = new List<FileIndexItem> {hiJpgInput, hi2JpgInput,hi3JpgInput,hi4JpgInput};

            var getAllResult = _query.GetAllFiles();

            CollectionAssert.AreEqual(getAllFilesExpectedResult,getAllResult);
            
            // Test subfolder
            var getAllFilesSubFolderExpectedResult = new List<FileIndexItem> {hi2SubfolderJpgInput};

            var getAllResultSubfolder = _query.GetAllFiles("/subfolder");
            CollectionAssert.AreEqual(getAllFilesSubFolderExpectedResult,getAllResultSubfolder);
            
            // GetAllRecursive
            var getAllRecursiveExpectedResult123 = new List<FileIndexItem> {
                hiJpgInput, hi2JpgInput, hi2SubfolderJpgInput, hi3JpgInput, hi4JpgInput };
            var getAllRecursive123 = _query.GetAllRecursive();
            CollectionAssert.AreEqual(getAllRecursive123,getAllRecursiveExpectedResult123);
            
            
            // GetItemByHash
            // See above for objects
            Assert.AreEqual(_query.GetItemByHash("09876543456789"), "/hi.jpg");

            // SubPathSlashRemove
            Assert.AreEqual(_query.SubPathSlashRemove("/test/"), "/test");
            
            // Next Winner
            var colorClassFilterList = new FileIndexItem().GetColorClassList("1");
            var next = _query.SingleItem("/hi.jpg", colorClassFilterList);
            Assert.AreEqual(next.RelativeObjects.NextFilePath, "/hi4.jpg");
            
            // Prev Winner
            var prev = _query.SingleItem("/hi4.jpg", colorClassFilterList).RelativeObjects.PrevFilePath;
            Assert.AreEqual(prev, "/hi.jpg");

        }

        [TestMethod]
        public void QueryFolder_DisplayFileFoldersTest()
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

            // This feature is normal used for folders, for now it is done on files
            // Hi3.jpg Previous -- all mode
            var releative = _query.GetNextPrevInFolder("/display/hi3.jpg");
            
            // Folders ignore deleted items
            Assert.AreEqual(releative.PrevFilePath,"/display/hi2.jpg");
            Assert.AreEqual(releative.NextFilePath,null);

            // Next  Relative -- all mode
            var releative2 = _query.GetNextPrevInFolder("/display/hi.jpg");
            
            Assert.AreEqual(releative2.NextFilePath,"/display/hi2.jpg");
            Assert.AreEqual(releative2.PrevFilePath,null);
            
            // 
            


        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
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

        private static FileIndexItem _insertSearchDatahiJpgInput;
        private static FileIndexItem _insertSearchDatahi2JpgInput;
        private static FileIndexItem _insertSearchDatahi3JpgInput;
        private static FileIndexItem _insertSearchDatahi4JpgInput;
        private static FileIndexItem _insertSearchDatahi2SubfolderJpgInput;

        private void InsertSearchData()
        {
            if (string.IsNullOrEmpty(_query.GetItemByHash("09876543456789")))
            {
                _insertSearchDatahiJpgInput = _query.AddItem(new FileIndexItem
                {
                    FileName = "hi.jpg",
                    FilePath = "/basic/hi.jpg",
                    ParentDirectory = "/basic",
                    FileHash = "09876543456789",
                    ColorClass = FileIndexItem.Color.Winner, // 1
                    Tags = "",
                    Title = ""
                });
                
                _insertSearchDatahi2JpgInput =  _query.AddItem(new FileIndexItem
                {
                    FileName = "hi2.jpg",
                    FilePath = "/basic/hi2.jpg",
                    Tags = "!delete!",
                    ParentDirectory = "/basic"
                });
            
                _insertSearchDatahi3JpgInput =  _query.AddItem(new FileIndexItem
                {
                    FileName = "hi3.jpg",
                    FilePath = "/basic/hi3.jpg",
                    ParentDirectory = "/basic",
                    ColorClass = FileIndexItem.Color.Trash // 9
                });
            
                _insertSearchDatahi4JpgInput =  _query.AddItem(new FileIndexItem
                {
                    FileName = "hi4.jpg",
                    FilePath = "/basic/hi4.jpg",
                    ParentDirectory = "/basic",
                    ColorClass = FileIndexItem.Color.Winner // 1
                });
            
                _insertSearchDatahi2SubfolderJpgInput =  _query.AddItem(new FileIndexItem
                {
                    FileName = "hi2.jpg",
                    FilePath = "/basic/subfolder/hi2.jpg",
                    ParentDirectory = "/basic/subfolder",
                    FileHash = "234567876543"
                });
            }
        }

        [TestMethod]
        public void QueryAddSingleItemhiJpgOutputTest()
        {
            InsertSearchData();
            var hiJpgOutput = _query.SingleItem(_insertSearchDatahiJpgInput.FilePath).FileIndexItem;

            Console.WriteLine(_insertSearchDatahiJpgInput.FileHash);
            Console.WriteLine(hiJpgOutput.FileHash);

            Assert.AreEqual(_insertSearchDatahiJpgInput.FileHash, hiJpgOutput.FileHash);
            
            // other api Get Object By FilePath
            hiJpgOutput = _query.GetObjectByFilePath(_insertSearchDatahiJpgInput.FilePath);
            Assert.AreEqual(_insertSearchDatahiJpgInput.FilePath, hiJpgOutput.FilePath);
        }

        [TestMethod]
        public void QueryAddSingleItemRootFolderTest()
        {

            InsertSearchData();
            // Test root folder ("/)
            var getAllFilesExpectedResult = new List<FileIndexItem>
            {
                _insertSearchDatahiJpgInput,
                _insertSearchDatahi2JpgInput,
                _insertSearchDatahi3JpgInput,
                _insertSearchDatahi4JpgInput
            };

            var getAllResult = _query.GetAllFiles("/basic");

            CollectionAssert.AreEqual(getAllFilesExpectedResult.Select(p => p.FilePath).ToList(), 
                getAllResult.Select(p => p.FilePath).ToList());
        }

        [TestMethod]
        public void QueryAddSingleItemSubFolderTest()
        {
            InsertSearchData();

            // Test subfolder
            var getAllFilesSubFolderExpectedResult = new List<FileIndexItem> {_insertSearchDatahi2SubfolderJpgInput};

            var getAllResultSubfolder = _query.GetAllFiles("/basic/subfolder");
            
            CollectionAssert.AreEqual(getAllFilesSubFolderExpectedResult.Select(p => p.FilePath).ToList(), 
                getAllResultSubfolder.Select(p => p.FilePath).ToList());
        }



        [TestMethod]
        public void QueryAddSingleItemGetAllRecursiveTest()
        {
            InsertSearchData();

            // GetAllRecursive
            var getAllRecursiveExpectedResult123 = new List<FileIndexItem>
            {
                _insertSearchDatahiJpgInput,
                _insertSearchDatahi2JpgInput,
                _insertSearchDatahi2SubfolderJpgInput,
                _insertSearchDatahi3JpgInput,
                _insertSearchDatahi4JpgInput
            }.OrderBy(p => p.FileName).ToList();
            
            var getAllRecursive123 = _query.GetAllRecursive()
                .Where(p => p.FilePath.Contains("/basic"))
                .OrderBy(p => p.FileName).ToList();

            Assert.AreEqual(getAllRecursive123.Count,getAllRecursiveExpectedResult123.Count);
            
            CollectionAssert.AreEqual(getAllRecursive123.Select(p => p.FileHash).ToList(), 
                getAllRecursiveExpectedResult123.Select(p => p.FileHash).ToList());
        }

        [TestMethod]
        public void QueryAddSingleItemGetItemByHashTest()
        {
            InsertSearchData();
            // GetItemByHash
            // See above for objects
            Assert.AreEqual(_query.GetItemByHash("09876543456789"), "/basic/hi.jpg");
        }

        [TestMethod]
        public void QueryAddSingleItemSubPathSlashRemoveTest()
        {
            InsertSearchData();
            // SubPathSlashRemove
            Assert.AreEqual(_query.SubPathSlashRemove("/test/"), "/test");
        }

        [TestMethod]
        public void QueryAddSingleItemNextWinnerTest()
        {
            InsertSearchData();
            // Next Winner
            var colorClassFilterList = new FileIndexItem().GetColorClassList("1");
            var next = _query.SingleItem("/basic/hi.jpg", colorClassFilterList);
            Assert.AreEqual(next.RelativeObjects.NextFilePath, "/basic/hi4.jpg");
        }

        [TestMethod]
        public void QueryAddSingleItemPrevWinnerTest()
        {       
            InsertSearchData();
            // Prev Winner
            var colorClassFilterList = new FileIndexItem().GetColorClassList("1");
            var prev = _query.SingleItem("/basic/hi4.jpg", colorClassFilterList).RelativeObjects.PrevFilePath;
            Assert.AreEqual(prev, "/basic/hi.jpg");
        }

        [TestMethod]
        [ExcludeFromCoverage]
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
            
            _query.AddItem(new FileIndexItem
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
            
        }

        [TestMethod]
        public void QueryFolder_DisplayFileFoldersNoResultTest()
        {
            var getDisplay = _query.DisplayFileFolders("/12345678987654").ToList();
            Assert.AreEqual(0, getDisplay.Count);
        }

        [ExcludeFromCoverage]
        [TestMethod]
        public void BreadcrumbDetailViewTest()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "hi3.jpg",
                FilePath = "/bread/hi3.jpg",
                ParentDirectory = "/bread", // without slash
                FileHash = "234565432",
                ColorClass = FileIndexItem.Color.Extras,
                IsDirectory = false
            });
            
            var exptectedOutput = new List<string>{"/","/bread"};
            var output = _query.SingleItem("/bread/hi3.jpg").Breadcrumb;
            CollectionAssert.AreEqual(exptectedOutput,output);
        }

        [ExcludeFromCoverage]
        [TestMethod]
        public void BreadcrumbDetailViewPagViewTypeTest()
        {
            _query.AddItem(new FileIndexItem
            {
                FileName = "hi4.jpg",
                FilePath = "/bread/hi4.jpg",
                ParentDirectory = "/bread", // without slash
                FileHash = "23456543",
                ColorClass = FileIndexItem.Color.Extras,
                IsDirectory = false
            });
            
            // Used for react to get the context
            var pageTypeReact = _query.SingleItem("/bread/hi4.jpg").PageType;
            Assert.AreEqual("DetailView",pageTypeReact);
        }

    }
}

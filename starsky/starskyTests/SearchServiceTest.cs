using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;
using starsky.Models;
using starsky.Services;
using starsky.ViewModels;

namespace starskytests
{
    [TestClass]
    public class SearchServiceTest
    {
        private SearchService _search;
        private Query _query;

        public SearchServiceTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("search");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _search = new SearchService(context);
            _query = new Query(context);
        }

        public void InsertSearchData()
        {
            if (string.IsNullOrEmpty(_query.GetItemByHash("schipholairplane")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "schipholairplane.jpg",
                    FilePath = "/stations/schipholairplane.jpg",
                    ParentDirectory = "/stations",
                    FileHash = "schipholairplane",
                    Tags = "schiphol, airplane, station",
                    Description = "schiphol",
                    Title = "Schiphol"
                });
            }

            if (string.IsNullOrEmpty(_query.GetItemByHash("lelystadcentrum")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "lelystadcentrum.jpg",
                    FilePath = "/stations/lelystadcentrum.jpg",
                    ParentDirectory = "/stations",
                    FileHash = "lelystadcentrum",
                    Tags = "station, train, lelystad, de trein"
                });
            }
            
            if (string.IsNullOrEmpty(_query.GetItemByHash("lelystadcentrum2")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "lelystadcentrum2.jpg",
                    FilePath = "/stations2/lelystadcentrum.jpg",
                    ParentDirectory = "/stations2",
                    FileHash = "lelystadcentrum2",
                    Tags = "lelystadcentrum2"
                });
            }
            
            if (string.IsNullOrEmpty(_query.GetItemByHash("stationdeletedfile")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "deletedfile.jpg",
                    FilePath = "/stations/deletedfile.jpg",
                    ParentDirectory = "/stations",
                    FileHash = "stationdeletedfile",
                    Tags = "!delete!"
                });
            }
            

            if (string.IsNullOrEmpty(_query.GetItemByHash("cityloop9")))
            {
                for (var i = 0; i < 61; i++)
                {
                    _query.AddItem(new FileIndexItem
                    {
                        FileName = "cityloop" + i + ".jpg",
                        FilePath = "/cities/cityloop" + i + ".jpg",
                        ParentDirectory = "/cities",
                        FileHash = "cityloop" + i,
                        Tags = "cityloop"
                    });
                }
            }

        }

        [TestMethod]
        public void SearchNull()
        {
            InsertSearchData();
            Assert.AreEqual(0, _search.Search(null).SearchCount);
        }
        
        [TestMethod]
        public void SearchCountStationTest()
        {
            InsertSearchData();
            // With deleted files is it 3
            // todo: check the value of this one
            Assert.AreEqual(2, _search.Search("station").SearchCount);
        }

        [TestMethod]
        public void SearchLastPageCityloopTest()
        {
            InsertSearchData();
            Assert.AreEqual(3, _search.Search("cityloop").LastPageNumber);
        }

        [TestMethod]
        public void SearchSchipholDescriptionTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("-Description:Schiphol").SearchCount);
        }
   
        [TestMethod]
        public void SearchSchipholFilenameTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("-filename:'schipholairplane.jpg'").SearchCount);
        }
        
        [TestMethod]
        public void SearchSchipholTitleTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("-title:Schiphol").SearchCount);
        }
            
        [TestMethod]
        public void SearchCityloopTest()
        {
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("cityloop").SearchCount);
        }
        
        [TestMethod]
        public void SearchStationLelystadTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("station lelystad").SearchCount);
        }

        [TestMethod]
        public void SearchParenthesisTreinTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("\"de trein\"").SearchCount);
        }

        [TestMethod]
        public void SearchCityloopCaseSensitiveTest()
        {
             InsertSearchData();
             //    Check case sensitive!
             Assert.AreEqual(61, _search.Search("CityLoop").SearchCount);
        }

        [TestMethod]
        public void SearchCityloopTrimTest()
        {
            // Test TRIM
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("   cityloop    ").SearchCount);
        }
        
        [TestMethod]
        public void SearchCityloopFilePathTest()
        {
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("-FilePath:cityloop").SearchCount);
        }
        
        [TestMethod]
        public void SearchCityloopFileNameTest()
        {
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("-FilePath:cityloop").SearchCount);
        }
        
        [TestMethod]
        public void SearchCityloopParentDirectoryTest()
        {
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("-ParentDirectory:/cities").SearchCount);
        }
        
        [TestMethod]
        public void SearchInUrlTest()
        {
            InsertSearchData();
            // Not 3, because one file is marked as deleted!
            // todo: check the value of this one
            Assert.AreEqual(4, _search.Search("-inurl:/stations").SearchCount);
            Assert.AreEqual(4, _search.Search("-inurl:\"/stations\"").SearchCount);
        }

        [TestMethod]
        public void SearchNarrowFileNameTags()
        {
            InsertSearchData();
            // Not 2 > but needs to be narrow
            // todo: check the value of this one
            Assert.AreEqual(1, _search.Search("lelystad -ParentDirectory:/stations2").SearchCount);
        }

        

        [TestMethod]
        public void SearchSetSearchInStringTypeTest()
        {
            var model = new SearchViewModel();
            model.SetAddSearchInStringType("Tags");
            Assert.AreEqual("Tags", model.SearchIn.FirstOrDefault());

            // Case insensitive!
            model.SetAddSearchInStringType("tAgs");
            Assert.AreEqual("Tags", model.SearchIn.FirstOrDefault());
        }

        [TestMethod]
        public void MatchSearchTwoKeywordsTest()
        {
            var model = new SearchViewModel
            {
                SearchQuery = "-Tags:dion -Filename:'dion.jpg'"
            };
            _search.MatchSearch(model);

            Assert.AreEqual(model.SearchIn.Contains("Tags"), true);
            Assert.AreEqual(model.SearchFor.Contains("dion.jpg"), true);
        }

        [TestMethod]
        public void MatchSearchOneKeywordsTest()
        {
            // Single keyword
            var model = new SearchViewModel {SearchQuery = "-Tags:dion"};
            _search.MatchSearch(model);
            Assert.AreEqual(model.SearchIn.Contains("Tags"), true);
        }

        [TestMethod]
        public void SearchPageTypeTest()
        {
            var model = _search.Search();
            Assert.AreEqual("Search",model.PageType);
        }
        
        [TestMethod]
        public void SearchElapsedSecondsIsNotZeroSecondsTest()
        {
            // todo:    at starskytests.SearchServiceTest.SearchElapsedSecondsIsNotZeroSecondsTest() in :line 266
//            InsertSearchData();
//            var model = _search.Search("dion");
//            Assert.AreNotEqual(0f,model.ElapsedSeconds);
        }

        [TestMethod]
        public void MatchSearchFileNameAndDefaultOptionTest()
        {
            // Single keyword
            var model = new SearchViewModel {SearchQuery = "-Filename:dion test"};
            _search.MatchSearch(model);
            Assert.AreEqual(model.SearchIn.Contains("FileName"), true);
            Assert.AreEqual(model.SearchIn.Contains("Tags"), true);
        }

        [TestMethod]
        public void QuerySafeTest()
        {
            var query = _search.QuerySafe("   d   ");
            Assert.AreEqual("d",query);
        }

        [TestMethod]
        public void QueryShortcutsInurlTest()
        {
            var query = _search.QueryShortcuts("-inurl");
            Assert.AreEqual("-FilePath",query);
        }

        [TestMethod]
        public void MatchSearchDefaultOptionTest()
        {
            // Single keyword
            var model = new SearchViewModel {SearchQuery = "test"};
            _search.MatchSearch(model);
            Assert.AreEqual(model.SearchIn.Contains("Tags"), true);
        }

        [TestMethod]
        public void SearchForDeletedFiles()
        {
            InsertSearchData();
            var del = _search.Search("!delete!");
            Assert.AreEqual(del.FileIndexItems.Count(),1);
            Assert.AreEqual(del.FileIndexItems.FirstOrDefault().FileHash, "stationdeletedfile");
        }

        [TestMethod]
        public void RoundDownTest()
        {
            Assert.AreEqual(_search.RoundDown(12),10);
        }
        
        [TestMethod]
        public void RoundUpTest()
        {
            Assert.AreEqual(_search.RoundUp(8),20); // NumberOfResultsInView
        }
        
    }
}
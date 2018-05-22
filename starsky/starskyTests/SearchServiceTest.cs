using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
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
                    Tags = "schiphol, airplane, station"
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
                    Tags = "station, train"
                });
            }
            
            if (string.IsNullOrEmpty(_query.GetItemByHash("deletedfile")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "deletedfile.jpg",
                    FilePath = "/stations/deletedfile.jpg",
                    ParentDirectory = "/stations",
                    FileHash = "deletedfile",
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
        public void SearchCountStationTest()
        {
            InsertSearchData();
            Assert.AreEqual(2, _search.Search("station").SearchCount);
        }

        [TestMethod]
        public void SearchLastPageCityloopTest()
        {
            InsertSearchData();
            Assert.AreEqual(3, _search.Search("cityloop").LastPageNumber);
        }

        [TestMethod]
        public void SearchCityloopTest()
        {
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("cityloop").SearchCount);
            // Check case sensitive!
            Assert.AreEqual(61, _search.Search("CityLoop").SearchCount);

            // Test TRIM
            Assert.AreEqual(61, _search.Search("   CityLoop    ").SearchCount);
        }

        [TestMethod]
        public void SearchInUrlTest()
        {
            InsertSearchData();
            // Not 3, because one file is marked as deleted!
            Assert.AreEqual(2, _search.Search("-inurl:/stations").SearchCount);
            Assert.AreEqual(2, _search.Search("-inurl:\"/stations\"").SearchCount);
        }

        [TestMethod]
        public void SearchSetSearchInStringTypeTest()
        {
            var model = new SearchViewModel() {AddSearchInStringType = "Tags"};
            Assert.AreEqual("Tags", model.SearchIn.FirstOrDefault());

            // Case insensitive!
            model = new SearchViewModel() {AddSearchInStringType = "tAgs"};
            Assert.AreEqual("Tags", model.SearchIn.FirstOrDefault());
        }

        [TestMethod]
        public void MatchSearchTwoKeywordsTest()
        {
            var model = new SearchViewModel();
            model.SearchQuery = "-Tags:dion -Filename:'dion.jpg'";
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
        public void MatchSearchFileNameAndDefaultOptionTest()
        {
            // Single keyword
            var model = new SearchViewModel {SearchQuery = "-Filename:dion test"};
            _search.MatchSearch(model);
            Assert.AreEqual(model.SearchIn.Contains("FileName"), true);
            Assert.AreEqual(model.SearchIn.Contains("Tags"), true);
        }

        [TestMethod]
        public void ComparePropValueTest()
        {
            var test = new FileIndexItem() {Tags = "test"}.GetPropValue("Tags");
            Assert.AreEqual(test,"test");
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
            Assert.AreEqual(del.FileIndexItems.FirstOrDefault().FileHash, "deletedfile");
        }

    }
}
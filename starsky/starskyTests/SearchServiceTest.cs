using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Attributes;
using starsky.Data;
using starsky.Models;
using starsky.Services;

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
            if (string.IsNullOrEmpty(_query.GetItemByHash("cityloop9")))
            {
                for (var i = 0; i < 61; i++)
                {
                    _query.AddItem(new FileIndexItem
                    {
                        FileName = "cityloop"+ i +".jpg",
                        FilePath = "/cities/cityloop"+ i +".jpg",
                        ParentDirectory = "/cities",
                        FileHash = "cityloop"+ i,
                        Tags = "cityloop"
                    });
                }

//                var q = _query.GetAllFiles("/cities").Count;
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
            Assert.AreEqual(2, _search.Search("-inurl:/stations").SearchCount);
            Assert.AreEqual(2, _search.Search("-inurl:\"/stations\"").SearchCount);

        }



    }
}
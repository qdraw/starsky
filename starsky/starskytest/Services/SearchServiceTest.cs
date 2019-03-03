using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Data;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskycore.ViewModels;
using starskytest.FakeMocks;
using Query = starskycore.Services.Query;

namespace starskytest.Services
{
    [TestClass]
    public class SearchServiceTest
    {
        private SearchService _search;
        private Query _query;
	    private ApplicationDbContext _dbContext;

	    public SearchServiceTest()
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            var memoryCache = provider.GetService<IMemoryCache>();
            
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("searchService");
            var options = builder.Options;
            _dbContext = new ApplicationDbContext(options);
            _search = new SearchService(_dbContext);
            _query = new Query(_dbContext,memoryCache);
        }

        public void InsertSearchData()
        {
            if (string.IsNullOrEmpty(_query.GetSubPathByHash("schipholairplane")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "schipholairplane.jpg",
                    ParentDirectory = "/stations",
                    FileHash = "schipholairplane",
                    Tags = "schiphol, airplane, station",
                    Description = "schiphol",
                    Title = "Schiphol",
					ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
                });
            }

            if (string.IsNullOrEmpty(_query.GetSubPathByHash("lelystadcentrum")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "lelystadcentrum.jpg",
                    ParentDirectory = "/stations",
                    FileHash = "lelystadcentrum",
                    Tags = "station, train, lelystad, de trein",
	                DateTime = DateTime.Now
                });
            }
            
            if (string.IsNullOrEmpty(_query.GetSubPathByHash("lelystadcentrum2")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "lelystadcentrum2.jpg",
                    ParentDirectory = "/stations2",
                    FileHash = "lelystadcentrum2",
                    Tags = "lelystadcentrum2",
                    DateTime = new DateTime(2016,1,1,1,1,1),
                    AddToDatabase = new DateTime(2016,1,1,1,1,1)
                });
            }
            
            if (string.IsNullOrEmpty(_query.GetSubPathByHash("stationdeletedfile")))
            {
	            // add directory to search for
	            _query.AddItem(new FileIndexItem
	            {
		            FileName = "stations",
		            ParentDirectory = "/",
		            FileHash = "",
		            IsDirectory = true
	            });
        
                _query.AddItem(new FileIndexItem
                {
                    FileName = "deletedfile.jpg",
                    ParentDirectory = "/stations",
                    FileHash = "stationdeletedfile",
                    Tags = "!delete!"
                });
            }
            

            if (string.IsNullOrEmpty(_query.GetSubPathByHash("cityloop9")))
            {
                for (var i = 0; i < 61; i++)
                {
                    // 61 > used for three pages
                    _query.AddItem(new FileIndexItem
                    {
                        FileName = "cityloop" + i + ".jpg",
                        // FilePath = "/cities/cityloop" + i + ".jpg",
                        ParentDirectory = "/cities",
                        FileHash = "cityloop" + i,
                        Tags = "cityloop",
                        DateTime = new DateTime(2018,1,1,1,1,1)
                    });
                }
                
            }

        }

        [TestMethod]
        public void SearchService_SearchNull()
        {
            InsertSearchData();
            Assert.AreEqual(0, _search.Search(null).SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchCountStationTest()
        {
            InsertSearchData();
            // With deleted files is it 3
            // todo: check the value of this one

	        var t = _query.GetAllRecursive("/");
	        
            Assert.AreEqual(2, _search.Search("station").SearchCount);
        }

        [TestMethod]
        public void SearchService_SearchLastPageCityloopTest()
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
        public void SearchService_SearchSchipholFilenameTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("-filename:'schipholairplane.jpg'").SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchSchipholTitleTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("-title:Schiphol").SearchCount);
        }
           
	    
	    [TestMethod]
	    public void SearchService_SearchSchipholFileHashTest()
	    {
		    InsertSearchData();
		    Assert.AreEqual(1, _search.Search("-filehash:schipholairplane").SearchCount);
	    }
	    
	    
        [TestMethod]
        public void SearchService_SearchCityloopTest()
        {
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("cityloop").SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchStationLelystadTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("station lelystad").SearchCount);
        }

        [TestMethod]
        public void SearchService_SearchParenthesisTreinTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("\"de trein\"").SearchCount);
        }
	    
        [TestMethod]
        public void SearchService_SearchCityloopCaseSensitiveTest()
        {
             InsertSearchData();
             //    Check case sensitive!
             Assert.AreEqual(61, _search.Search("CityLoop").SearchCount);
        }

        [TestMethod]
        public void SearchService_SearchCityloopTrimTest()
        {
            // Test TRIM
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("   cityloop    ").SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchCityloopFilePathTest()
        {
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("-FilePath:cityloop").SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchCityloopFileNameTest()
        {
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("-FilePath:cityloop").SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchCityloopParentDirectoryTest()
        {
            InsertSearchData();
            Assert.AreEqual(61, _search.Search("-ParentDirectory:/cities").SearchCount);
        }
	    
	    
	    [TestMethod]
	    public void SearchService_SearchForDirectories()
	    {
		    InsertSearchData();
		    Assert.AreEqual(1, _search.Search("-isDirectory:true -inurl:stations").SearchCount);
	    }
        
        [TestMethod]
        public void SearchService_SearchInUrlTest()
        {
            InsertSearchData();
            // Not 3, because one file is marked as deleted!
            // todo: check the value of this one
            Assert.AreEqual(5, _search.Search("-inurl:/stations").SearchCount);
            Assert.AreEqual(5, _search.Search("-inurl:\"/stations\"").SearchCount);
        }

        [TestMethod]
        public void SearchService_SearchNarrowFileNameTags()
        {
            InsertSearchData();
            // Not 2 > but needs to be narrow
            // todo: check the value of this one
            Assert.AreEqual(1, _search.Search("lelystad -ParentDirectory:/stations2").SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchNarrow2FileNameTags()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("-ParentDirectory:/station -ParentDirectory:2").SearchCount);
        }

        [TestMethod]
        public void SearchService_SearchDateTime()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("-DateTime>2015-01-01T01:01:01 -DateTime<2017-01-01T01:01:01").SearchCount);
            // lowercase is the same
            Assert.AreEqual(1, _search.Search("-DateTime>2015-01-01t01:01:01 -DateTime<2017-01-01t01:01:01").SearchCount);
        }
	    
	    [TestMethod]
	    public void SearchService_SearchOnOnlyDay()
	    {
		    InsertSearchData();

		    var item = _search.Search("-DateTime=2016-01-01");
		    Assert.AreEqual(1, item.SearchCount);
	    }
	    
	    [TestMethod]
	    public void SearchService_SearchOnToday()
	    {
		    InsertSearchData();

		    var item = _search.Search("-DateTime=0");
		    Assert.AreEqual(1, item.SearchCount);
	    }
        
        [TestMethod]
        public void SearchService_Searchaddtodatabase()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("-addtodatabase>2015-01-01T01:01:01 -addtodatabase<2017-01-01T01:01:01").SearchCount);
        }
	    
	    [TestMethod]
	    public void SearchService_SearchForImageFormat()
	    {
		    InsertSearchData();
		    Assert.AreEqual(1, _search.Search("-ImageFormat:jpg").SearchCount);
	    }
        
        [TestMethod]
        public void SearchService_SearchSetSearchInStringTypeTest()
        {
            var model = new SearchViewModel();
            model.SetAddSearchInStringType("Tags");
            Assert.AreEqual("Tags", model.SearchIn.FirstOrDefault());

            // Case insensitive!
            model.SetAddSearchInStringType("tAgs");
            Assert.AreEqual("Tags", model.SearchIn.FirstOrDefault());
        }

        [TestMethod]
        public void SearchService_MatchSearchTwoKeywordsTest()
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
        public void SearchService_MatchSearchOneKeywordsTest()
        {
            // Single keyword
            var model = new SearchViewModel {SearchQuery = "-Tags:dion"};
            _search.MatchSearch(model);
            Assert.AreEqual(model.SearchIn.Contains("Tags"), true);
        }

        [TestMethod]
        public void SearchService_SearchPageTypeTest()
        {
            var model = _search.Search();
            Assert.AreEqual("Search",model.PageType);
        }
        
//        [TestMethod]
//        public void SearchElapsedSecondsIsNotZeroSecondsTest()
//        {
//            InsertSearchData();
//            var model = _search.Search("cityloop");
//            // Search is fast so one item is in the unit test 0 seconds;
//            Console.WriteLine(model.ElapsedSeconds);
//            // Sometimes it fails randomly that is 0 seconds
//            if (model.ElapsedSeconds == 0f)
//            {
//                model = _search.Search("cityloop");
//            }
////            Assert.AreNotEqual(0f,model.ElapsedSeconds);
//        }

        [TestMethod]
        public void SearchService_MatchSearchFileNameAndDefaultOptionTest()
        {
            // Single keyword
            var model = new SearchViewModel {SearchQuery = "-Filename:dion test"};
            _search.MatchSearch(model);
            Assert.AreEqual(model.SearchIn.Contains("FileName"), true);
            Assert.AreEqual(model.SearchIn.Contains("Tags"), true);
        }

        [TestMethod]
        public void SearchService_QuerySafeTest()
        {
            var query = _search.QuerySafe("   d   ");
            Assert.AreEqual("d",query);
        }

        [TestMethod]
        public void SearchService_QueryShortcutsInurlTest()
        {
            var query = _search.QueryShortcuts("-inurl");
            Assert.AreEqual("-FilePath",query);
        }

        [TestMethod]
        public void SearchService_MatchSearchDefaultOptionTest()
        {
            // Single keyword
            var model = new SearchViewModel {SearchQuery = "test"};
            _search.MatchSearch(model);
            Assert.AreEqual(model.SearchIn.Contains("Tags"), true);
        }

        [TestMethod]
        public void SearchService_SearchForDeletedFiles()
        {
            InsertSearchData();

	        var all = _query.GetAllRecursive().Where(p => p.Tags.Contains("!delete!")).ToList();
            var del = _search.Search("!delete!");
	        var count = del.FileIndexItems.Count();
            Assert.AreEqual(1,count);
            Assert.AreEqual(del.FileIndexItems.FirstOrDefault().FileHash, "stationdeletedfile");
        }

        [TestMethod]
        public void SearchService_RoundDownTest()
        {
            Assert.AreEqual(_search.RoundDown(12),10);
        }
        
        [TestMethod]
        public void SearchService_RoundUpTest()
        {
            Assert.AreEqual(_search.RoundUp(8),20); // NumberOfResultsInView
        }


		[TestMethod]
		public void SearchService_cacheTest()
		{
			var searchService = new SearchService(_dbContext,new FakeMemoryCache(),new AppSettings());
			var result = searchService.Search("t"); // <= t is only to detect in fakeCache
			Assert.AreEqual(1,result.FileIndexItems.Count);
			
		}
	    

	    [TestMethod]
	    public void SearchService_thisORThisFileHashes()
	    {
		    InsertSearchData();
		    var result = _search.Search("-FileHash=stationdeletedfile || -FileHash=lelystadcentrum",0,false);
		    Assert.AreEqual(2,result.FileIndexItems.Count);
	    }

	    
//	    [TestMethod]
//	    public void SearchService_thisORThis2()
//	    {
//		    InsertSearchData();
//		    var result = _search.Search("-DateTime=2016-01-01 || -FileHash=lelystadcentrum",0,false);
//		    Assert.AreEqual(2,result.FileIndexItems.Count);
//	    }
//
//	    
//	    [TestMethod]
//	    public void SearchService_thisORThisDate()
//	    {
//		    InsertSearchData();
//		    //todo": test FAILING!!
//		    var result = _search.Search("-DateTime=2016-01-01 || -FileHash=lelystadcentrum",0,false);
//		    Assert.AreEqual(2,result.FileIndexItems.Count);
//
//	    }
	    
	    [TestMethod]
	    public void SearchService_thisORAndCombination()
	    {
		    InsertSearchData();
		    var result = _search.Search("-FileName=lelystadcentrum2.jpg || -FileHash=lelystadcentrum2 && station",0,false);
			//  -FileHash=lelystadcentrum2 && station >= 1 item
		    // -DateTime=lelystadcentrum2.jpg >= 1 item
		    // station = duplicate in this example but triggers other results when using || instead of &&
		    Assert.AreEqual(2,result.FileIndexItems.Count);

	    }
	    
	    
//	    [TestMethod]
//	    public void SearchService_DoubleSearchOnOnlyDay()
//	    {
//		    InsertSearchData();
//
//		    var item = _search.Search("-DateTime=2016-01-01 || -DateTime=0");
//		    // This are actually four queries
//		    
//		    // todo: test FAIL
//		    //Assert.AreEqual(2, item.SearchCount);
//	    }
    }
}

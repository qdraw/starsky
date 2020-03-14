using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
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
        private readonly SearchService _search;
        private readonly Query _query;
	    private readonly ApplicationDbContext _dbContext;
	    private readonly IMemoryCache _memoryCache;

	    public SearchServiceTest()
        {
            var provider = new ServiceCollection()
                .AddMemoryCache()
                .BuildServiceProvider();
            _memoryCache = provider.GetService<IMemoryCache>();
            
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("searchService");
            var options = builder.Options;
            _dbContext = new ApplicationDbContext(options);
            _search = new SearchService(_dbContext);
            _query = new Query(_dbContext,_memoryCache);
        }
	    
	    private const int NumberOfResultsInView = 120;
	    private const int NumberOfFakeResults = 241;

	    
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
					ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
	                DateTime = new DateTime(2014,1,1,1,1,1),
	                MakeModel = "Apple|iPhone SE|"
                });
            }

            if (string.IsNullOrEmpty(_query.GetSubPathByHash("lelystadcentrum")))
            {
                _query.AddItem(new FileIndexItem
                {
                    FileName = "lelystadcentrum.jpg",
                    ParentDirectory = "/stations",
                    FileHash = "lelystadcentrum",
                    Tags = "station, train, lelystad, de trein, delete",
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
                    Description = "lelystadcentrum2",
                    ImageFormat = ExtensionRolesHelper.ImageFormat.tiff,
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
		            IsDirectory = true,
		            DateTime = new DateTime(2013,1,1,1,1,1),
	            });
        
                _query.AddItem(new FileIndexItem
                {
                    FileName = "deletedfile.jpg",
                    ParentDirectory = "/stations",
                    FileHash = "stationdeletedfile",
                    Tags = "!delete!",
	                DateTime = new DateTime(2013,1,1,1,1,1),
                });
            }
            

            if (string.IsNullOrEmpty(_query.GetSubPathByHash("cityloop9")))
            {
                for (var i = 0; i < NumberOfFakeResults; i++)
                {
                    // NumberOfFakeResults > used for three pages
                    _query.AddItem(new FileIndexItem
                    {
                        FileName = "cityloop" + i + ".jpg",
                        ParentDirectory = "/cities",
                        FileHash = "cityloop" + i,
                        Tags = "cityloop",
                        Id = 5000 + i,
                        DateTime = new DateTime(2018,1,1,1,1,1)
                    });
                }
            }
        }

	    [TestMethod]
	    public void SearchService_CacheInjection_CacheTest()
	    {
		    var search = new SearchService(_dbContext,_memoryCache);

		    // fill cache with data realdata;
		    var result = search.Search("test");
		    Assert.AreEqual("test",result.SearchQuery);

		    // now update only the name direct in the cache
		    _memoryCache.Set("search-test", new SearchViewModel{SearchQuery = "cache"}, 
			    new TimeSpan(0,10,0));
		    
		    // now query again
		    result = search.Search("test");
		    // and get the cached value
		    Assert.AreEqual("cache",result.SearchQuery);
	    }

	    [TestMethod]
	    public void SearchService_RemoveCache_Test()
	    {
		    _memoryCache.Set("search-test", new SearchViewModel {SearchQuery = "cache"},
			    new TimeSpan(0, 10, 0));
		    var search = new SearchService(_dbContext,_memoryCache);
		    
		    // Test if the pre-condition is good
		    var cachedResult = search.Search("test").SearchQuery;
			Assert.AreEqual(cachedResult,"cache");
			
			// Remove cache
			search.RemoveCache("test");
			
			// test if cache is removed
		    var result = search.Search("test");
		    Assert.AreEqual("test",result.SearchQuery);
	    }

	    [TestMethod]
	    public void SearchService_RemoveCache_Disabled_Test()
	    {
		    var search = new SearchService(_dbContext,null); // cache is null!
		    Assert.AreEqual(null,search.RemoveCache("test"));
	    }
	    
	    [TestMethod]
	    public void SearchService_RemoveCache_NoCachedItem_Test()
	    {
		    var search = new SearchService(_dbContext,_memoryCache);
		    Assert.AreEqual(false,search.RemoveCache("test"));
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
            Assert.AreEqual(2, _search.Search("cityloop").LastPageNumber);
        }
	    
	    
	    [TestMethod]
	    public void SearchService_SearchForDatetime()
	    {
		    InsertSearchData();
		    Assert.AreEqual(1, _search.Search("-Datetime=\"2016-01-01 01:01:01\"").SearchCount);
	    }
	    
	    [TestMethod]
	    public void SearchService_SearchForDatetimeSmallerThen()
	    {
		    InsertSearchData();
		    Assert.AreEqual(2, _search.Search("-Datetime<\"2013-01-01 02:01:01\"").SearchCount);
	    }
	    
	    [TestMethod]
	    public void SearchService_SearchForDatetimeGreaterThen()
	    {
		    InsertSearchData();
		    Assert.AreEqual(NumberOfFakeResults+1, _search.Search("-Datetime>\"2018-01-01 01:01:01\"").SearchCount);
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
        public void SearchService_Make_Search()
        {
	        InsertSearchData();
	        Assert.AreEqual(1, _search.Search("-Make:Apple").SearchCount);
	        // schipholairplane.jpg
        }

        [TestMethod]
        public void SearchService_Make_SearchNonExist()
        {
	        InsertSearchData();
	        Assert.AreEqual(0, _search.Search("-Make:SE").SearchCount);
	        // NOT schipholairplane.jpg
        }
        
        [TestMethod]
        public void SearchService_Model_Search()
        {
	        InsertSearchData();
	        Assert.AreEqual(1, _search.Search("-Model:SE").SearchCount);
	        // schipholairplane.jpg
        }
        
        [TestMethod]
        public void SearchService_NotQueryCityloop240()
        {
	        InsertSearchData();
	        Assert.AreEqual(NumberOfFakeResults-1, _search.Search("-filehash:cityloop -filehash-cityloop240").SearchCount);
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
            Assert.AreEqual(NumberOfFakeResults, _search.Search("cityloop").SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchStationLelystadTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("station lelystad").SearchCount);
        }
	    
	    [TestMethod]
	    public void SearchService_SearchStationLelystadQuotedTest()
	    {
		    InsertSearchData();
		    Assert.AreEqual(1, _search.Search("\"station\" \"lelystad\"").SearchCount);
	    }

        [TestMethod]
        public void SearchService_SearchParenthesisTreinTest()
        {
            InsertSearchData();
            Assert.AreEqual(1, _search.Search("\"de trein\"").SearchCount);
        }
	    
	    
	    [TestMethod]
	    public void SearchService_SearchIOSDoubleParenthesisTreinTest()
	    {
		    InsertSearchData();
		    Assert.AreEqual(1, _search.Search("“!delete!”").SearchCount);
	    }
	    
	    [TestMethod]
	    public void SearchService_SearchIOSSingleParenthesisTreinTest()
	    {
		    InsertSearchData();
		    Assert.AreEqual(1, _search.Search("‘!delete!’").SearchCount);
	    }
	    
	    
	    [TestMethod]
	    public void SearchService_SearchNonParenthesisTreinTest()
	    {
		    InsertSearchData();
		    Assert.AreEqual(1, _search.Search("de trein").SearchCount);
	    }
	    
        [TestMethod]
        public void SearchService_SearchCityloopCaseSensitiveTest()
        {
             InsertSearchData();
             //    Check case sensitive!
             Assert.AreEqual(NumberOfFakeResults, _search.Search("CityLoop").SearchCount);
        }

        [TestMethod]
        public void SearchService_SearchCityloopTrimTest()
        {
            // Test TRIM
            InsertSearchData();
            Assert.AreEqual(NumberOfFakeResults, _search.Search("   cityloop    ").SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchCityloopFilePathTest()
        {
            InsertSearchData();
            Assert.AreEqual(NumberOfFakeResults, _search.Search("-FilePath:cityloop").SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchCityloopFileNameTest()
        {
            InsertSearchData();
            Assert.AreEqual(NumberOfFakeResults, _search.Search("-FilePath:cityloop").SearchCount);
        }
        
        [TestMethod]
        public void SearchService_SearchCityloopParentDirectoryTest()
        {
            InsertSearchData();
            Assert.AreEqual(NumberOfFakeResults, _search.Search("-ParentDirectory:/cities").SearchCount);
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
        public void SearchService_SearchForDateTimeBetween()
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
        public void SearchService_SearchPageTypeTest_StringEmpty()
        {
            var model = _search.Search();
            Assert.AreEqual("Search",model.PageType);
        }
	    
	    
	    [TestMethod]
	    public void SearchService_MatchSearch_StringEmpty()
	    {
		    var model = _search.MatchSearch(new SearchViewModel
		    {
			    SearchQuery = string.Empty
		    });
		    Assert.AreEqual(string.Empty,model.SearchQuery);
	    }

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

//	        var all = _query.GetAllRecursive().Where(p => p.Tags.Contains("!delete!")).ToList();
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
            Assert.AreEqual(_search.RoundUp(8),120); // NumberOfResultsInView
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
		    var result = _search.Search("-FileHash=stationdeletedfile || -FileHash=lelystadcentrum2",0,false);
		    Assert.AreEqual(2,result.FileIndexItems.Count);
	    }
	    
	    [TestMethod]
	    public void SearchService_DescriptionImageFormat()
	    {
		    InsertSearchData();
		    var result = _search.Search("-Description=lelystadcentrum2 -ImageFormat=tiff",0,false);
		    Assert.AreEqual(1,result.FileIndexItems.Count);
	    }
	    
	    [TestMethod]
	    public void SearchService_DescriptionOne()
	    {
		    InsertSearchData();
		    var result = _search.Search("-Description=lelystadcentrum2",0,false);
		    Assert.AreEqual(1,result.FileIndexItems.Count);
	    }

	    [TestMethod]
	    public void SearchService_thisORAndCombination()
	    {
		    InsertSearchData();
		    var result = _search.Search("-FileName=lelystadcentrum.jpg || -FileHash=lelystadcentrum && lelystad",
			    0,false);
			//  -FileHash=lelystadcentrum2 && station >= 1 item
		    // -DateTime=lelystadcentrum2.jpg >= 1 item
		    // the and applies to all previous items
		    // lelystadcentrum && lelystadcentrum2 are items
		    // station = duplicate in this example but triggers other results when using || instead of &&
		    Assert.AreEqual(2,result.FileIndexItems.Count);
	    }

	    [TestMethod]
	    public void SearchService_thisORDefaultKeyword()
	    {
		    InsertSearchData();
		    var result = _search.Search("station || lelystad",0,false);
		    Assert.AreEqual(3,result.FileIndexItems.Count);
	    }

	    [TestMethod]
	    public void SearchViewModel_ParseDefaultOption()
	    {
		    var modelSearchQuery = "station || lelystad";
		    var result = new SearchViewModel().ParseDefaultOption(modelSearchQuery);
		    Assert.AreEqual("-Tags:\"station\" -Tags:\"lelystad\" ",result);
	    }

	    [TestMethod]
	    public void SearchViewModel_Duplicate_ParseDefaultOption()
	    {
		    var modelSearchQuery = "station || station";
		    var result = new SearchViewModel().ParseDefaultOption(modelSearchQuery);
		    Assert.AreEqual("-Tags:\"station\" -Tags:\"station\" ",result);
	    }

	    [TestMethod]
	    public void SearchViewModel_Quoted_OR_ParseDefaultOption()
	    {
		    var modelSearchQuery = " \"station test\" || lelystad || key2";
		    var result = new SearchViewModel().ParseDefaultOption(modelSearchQuery);
		    Assert.AreEqual("-Tags:\"station test\" -Tags:\"lelystad\" -Tags:\"key2\" ",result);
	    }

	    [TestMethod]
	    public void SearchViewModel_Quoted_NotSearch_ParseDefaultOption()
	    {
		    var modelSearchQuery = "-\"station test\"";
		    var model = new SearchViewModel();
		    model.ParseDefaultOption(modelSearchQuery);
		    Assert.AreEqual(SearchViewModel.SearchForOptionType.Not,model.SearchForOptions[0]);
	    }
	    
	    [TestMethod]
	    public void SearchViewModel_NotSearch_ParseDefaultOption()
	    {
		    var modelSearchQuery = "-station";
		    var model = new SearchViewModel();
		    model.ParseDefaultOption(modelSearchQuery);
		    Assert.AreEqual(SearchViewModel.SearchForOptionType.Not,model.SearchForOptions[0]);
	    }
	    
	    [TestMethod]
	    public void SearchViewModel_Quoted_DefaultSplit_ParseDefaultOption()
	    {
		    var modelSearchQuery = " \"station test\" key2";
		    var result = new SearchViewModel().ParseDefaultOption(modelSearchQuery);
		    Assert.AreEqual("-Tags:\"station test\" -Tags:\"key2\" ",result);
	    }
	    
	    [TestMethod]
	    public void SearchViewModel_SearchOperatorOptions_Quoted_with_ParseDefaultOption()
	    {
		    var modelSearchQuery = " \"station test\" \"station test\"";
		    var searchViewModel = new SearchViewModel();
		    
			searchViewModel.ParseDefaultOption(modelSearchQuery);
		    
		    var searchOperatorOptions = searchViewModel.SearchOperatorOptions;
		    
		    Assert.AreEqual(true,searchOperatorOptions[0]);
		    Assert.AreEqual(true,searchOperatorOptions[1]);
	    }

	    [TestMethod]
	    public void SearchViewModel_SearchOperatorOptions_NonQuoted_with_ParseDefaultOption()
	    {
		    var modelSearchQuery = "station test";
		    var searchViewModel = new SearchViewModel();
		    
		    searchViewModel.ParseDefaultOption(modelSearchQuery);
		    
		    var searchOperatorOptions = searchViewModel.SearchOperatorOptions;
		    
		    Assert.AreEqual(true,searchOperatorOptions[0]);
		    Assert.AreEqual(true,searchOperatorOptions[1]);
	    }

	    [TestMethod]
	    public void SearchViewModel_SearchOperatorOptions_NonQuoted_with_OR_Situation_ParseDefaultOption()
	    {
		    var modelSearchQuery = "station || test";
		    var searchViewModel = new SearchViewModel();
		    
		    searchViewModel.ParseDefaultOption(modelSearchQuery);
		    
		    var searchOperatorOptions = searchViewModel.SearchOperatorOptions;
		    
		    Assert.AreEqual(false,searchOperatorOptions[0]);
		    Assert.AreEqual(false,searchOperatorOptions[1]);
	    }
	    
	    [TestMethod]
	    public void SearchViewModel_SearchOperatorOptions_Quoted_with_OR_Situation_ParseDefaultOption()
	    {
		    var modelSearchQuery = "\"station\" || \"test\"";
		    var searchViewModel = new SearchViewModel();
		    
		    searchViewModel.ParseDefaultOption(modelSearchQuery);
		    
		    var searchOperatorOptions = searchViewModel.SearchOperatorOptions;
		    
		    Assert.AreEqual(false,searchOperatorOptions[0]);
		    Assert.AreEqual(false,searchOperatorOptions[1]);
	    }

	    [TestMethod]
	    public void SearchViewModel_SearchOperatorOptions_ShortWord()
	    {
		    // of -> wrong detected due searching for not queries
		    var modelSearchQuery = "query of";
		    var searchViewModel = new SearchViewModel();
		    searchViewModel.ParseDefaultOption(modelSearchQuery);
		    Assert.AreEqual("query", searchViewModel.SearchFor[0]);
		    Assert.AreEqual("of", searchViewModel.SearchFor[1]);
	    }
	    
	    [TestMethod]
	    public void SearchViewModel_TwoCharWords_ShortWord()
	    {
		    // two chars used have an exception
		    var modelSearchQuery = "ns";
		    var searchViewModel = new SearchViewModel();
		    searchViewModel.ParseDefaultOption(modelSearchQuery);

		    Assert.AreEqual(SearchViewModel.SearchForOptionType.Equal, searchViewModel.SearchForOptions[0]);
		    Assert.AreEqual("Tags", searchViewModel.SearchIn[0]);
		    Assert.AreEqual("ns", searchViewModel.SearchFor[0]);
	    }

	    [TestMethod]
	    public void SearchViewModel_ParseDateTimeLowInt()
	    {
		    var p = new SearchViewModel().ParseDateTime("0");
		    // today
		    Assert.AreEqual(p.Day,DateTime.Now.Day);
		    Assert.AreEqual(p.Month,DateTime.Now.Month);
	    }
	    
	    
	    [TestMethod]
	    public void SearchViewModel_ParseDateTimeLargeInt()
	    {
		    var p = new SearchViewModel().ParseDateTime("20180911");
		    // defaults to today
			Assert.AreEqual(p.Day,DateTime.Now.Day);
		    Assert.AreEqual(p.Month,DateTime.Now.Month);
	    }
	    
	    [TestMethod]
	    public void SearchViewModel_ParseDateTimeExample()
	    {
		    var p = new SearchViewModel().ParseDateTime("2018-09-11");
		    // defaults to today
		    Assert.AreEqual(DateTime.Parse("2018-09-11"),p);
	    }
	    
		[TestMethod]
		public void SearchService_NotSingleKeywordsSearch()
		{
			var model = new SearchViewModel
			{
				SearchIn =
				{
					"tags"
				},
				FileIndexItems = new List<FileIndexItem>{new FileIndexItem
					{
						Tags = "lelystadcentrum"
					},
					new FileIndexItem
					{
						Tags = "lelystadcentrum2"
					},
					new FileIndexItem
					{
						Tags = "else"
					}
				}
			};
			model.SetAddSearchFor("lelystadcentrum");
			
			model.SetAddSearchForOptions("=");

			var result = model.NarrowSearch(model);
			Assert.AreEqual(2,result.FileIndexItems.Count);

			// Add extra NOT query			
			model.SearchIn.Add("tags");
			model.SetAddSearchFor("lelystadcentrum2"); // not query
			model.SetAddSearchForOptions("-");

			var result2 = model.NarrowSearch(model);

			Assert.AreEqual("lelystadcentrum",result.FileIndexItems[0].Tags);

			
		}
	    
	    [TestMethod]
	    public void SearchService_NotImageFormatSearch()
	    {
		    var model = new SearchViewModel
		    {
			    SearchIn =
			    {
				    "tags"
			    },
			    FileIndexItems = new List<FileIndexItem>{new FileIndexItem
				    {
					    Tags = "lelystadcentrum",
					    ImageFormat = ExtensionRolesHelper.ImageFormat.tiff // NOT right format
				    },
				    new FileIndexItem
				    {
					    Tags = "lelystadcentrum2", // return this one
					    ImageFormat = ExtensionRolesHelper.ImageFormat.bmp
				    },
				    new FileIndexItem
				    {
					    Tags = "else", //<= out of query
					    ImageFormat = ExtensionRolesHelper.ImageFormat.bmp
				    }
			    }
		    };
		    model.SetAddSearchFor("lelystadcentrum");
		    model.SetAddSearchForOptions("=");

		    // Add extra NOT query			
		    model.SearchIn.Add("imageformat");
		    model.SetAddSearchFor("tiff"); // not query
		    model.SetAddSearchForOptions("-");

		    var result = model.NarrowSearch(model);

		    Assert.AreEqual("lelystadcentrum2",result.FileIndexItems[0].Tags);
		    Assert.AreEqual(1,result.FileIndexItems.Count);
	    }

	    [TestMethod]
	    public void SearchService_Search_Percentage()
	    {
		    var results = _search.Search("%", 0);
		    Assert.AreEqual(0,results.FileIndexItems.Count);
		    Assert.AreEqual("%",results.SearchQuery);
	    }


	    [TestMethod]
	    [ExpectedException(typeof(ArgumentException))]
	    public void SearchService_Search_ToLong_ArgumentException()
	    {
		    var longTestText =
			    "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor" +
			    " incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor sit amet, consectetur" +
			    "adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Lorem ipsum " +
			    "dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut lab" +
			    "ore et dolore magna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing elit, se" +
			    "d do eiusmod tempor incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor sit " +
			    "amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolor" +
			    "e magna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod" +
			    " tempor incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor sit amet, consecte" +
			    "tur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Lor" +
			    "em ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt " +
			    "ut labore et dolore magna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing eli" +
			    "t, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor " +
			    "sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolo" +
			    "re magna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod t" +
			    "empor incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor sit amet, consectetur" +
			    " adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Lorem" +
			    " ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut l" +
			    "abore et dolore magna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed" +
			    " do eiusmod tempor incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor sit amet" +
			    ", consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna " +
			    "aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incid" +
			    "idunt ut labore et dolore magna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing e" +
			    "lit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor si" +
			    "t amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore m" +
			    "agna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor i" +
			    "ncididunt ut labore et dolore magna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscin" +
			    "g elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor" +
			    " sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore" +
			    " magna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor" +
			    " incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor sit amet, consectetur adipisc" +
			    "ing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Lorem ipsum dolo" +
			    "r sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore" +
			    " magna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor " +
			    "incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor sit amet, consectetur adipiscing" +
			    " elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Lorem ipsum dolor sit" +
			    " amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna ";

		    _search.Search(longTestText);
		    // Expect ArgumentException
	    }

    }
}

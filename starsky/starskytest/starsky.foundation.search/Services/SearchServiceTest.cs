using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.search.Services;
using starsky.feature.search.ViewModels;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.search.Services
{
	[TestClass]
	public sealed class SearchServiceTest
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
			_memoryCache = provider.GetRequiredService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("searchService");
			var options = builder.Options;
			_dbContext = new ApplicationDbContext(options);
			_search = new SearchService(_dbContext,new FakeIWebLogger() );
			_query = new Query(_dbContext,
				new AppSettings(), null!, new FakeIWebLogger(),_memoryCache);
		}

		private const int NumberOfFakeResults = 241;
		private const int NumberOfFakeResultsThatFitOnPage = 240;

		public async Task InsertSearchData()
		{
			if (string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("schipholairplane")))
			{
				await _query.AddItemAsync(new FileIndexItem
				{
					FileName = "schipholairplane.jpg",
					ParentDirectory = "/stations",
					FileHash = "schipholairplane",
					Tags = "schiphol, airplane, station",
					Description = "schiphol",
					Title = "Schiphol",
					ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
					DateTime = new DateTime(2014,1,1,1,1,1, 
						kind: DateTimeKind.Local),
					MakeModel = "Apple|iPhone SE|",
					Software = "PhotoTool x",
					IsDirectory = false,
					ColorClass = ColorClassParser.Color.WinnerAlt,
					LastEdited = new DateTime(2020,10,10,10,10,10, 
						kind: DateTimeKind.Local)
				});
			}

			if (string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("lelystadcentrum")))
			{
				await _query.AddItemAsync(new FileIndexItem
				{
					FileName = "lelystadcentrum.jpg",
					ParentDirectory = "/stations",
					FileHash = "lelystadcentrum",
					Tags = "station, train, lelystad, de trein, delete",
					ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
					DateTime = DateTime.Now,
					IsDirectory = false
				});
				
				await _query.AddItemAsync(new FileIndexItem
				{
					FileName = "lelystadcentrum.xmp",
					ParentDirectory = "/stations",
					FileHash = "lelystadcentrum",
					Tags = "station, train, lelystad, de trein, delete",
					DateTime = DateTime.Now,
					ImageFormat = ExtensionRolesHelper.ImageFormat.xmp,
					IsDirectory = false
				});
			}
            
			if (string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("lelystadcentrum2")))
			{
				await _query.AddItemAsync(new FileIndexItem
				{
					FileName = "lelystadcentrum2.jpg",
					ParentDirectory = "/stations2",
					FileHash = "lelystadcentrum2",
					Tags = "lelystadcentrum2",
					Description = "lelystadcentrum2",
					ImageFormat = ExtensionRolesHelper.ImageFormat.tiff,
					DateTime = new DateTime(2016,1,1,1,1,1, 
						kind: DateTimeKind.Local),
					AddToDatabase = new DateTime(2016,1,1,1,1,1, 
						kind: DateTimeKind.Local),
					IsDirectory = false
				});
			}
            
			if (string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("stationdeletedfile")))
			{
				// add directory to search for
				await _query.AddItemAsync(new FileIndexItem
				{
					FileName = "stations",
					ParentDirectory = "/",
					FileHash = "",
					IsDirectory = true,
					DateTime = new DateTime(2013,1,1,1,1,1, 
						kind: DateTimeKind.Local),
				});
        
				await _query.AddItemAsync(new FileIndexItem
				{
					FileName = "deletedfile.jpg",
					ParentDirectory = "/stations",
					FileHash = "stationdeletedfile",
					ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
					Tags = TrashKeyword.TrashKeywordString,
					DateTime = new DateTime(2013,1,1,1,1,1, 
						kind: DateTimeKind.Local),
					IsDirectory = false
				});
			}
            

			if (string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("cityloop9")))
			{
				for (var i = 0; i < NumberOfFakeResults; i++)
				{
					// NumberOfFakeResults > used for three pages
					await _query.AddItemAsync(new FileIndexItem
					{
						FileName = "cityloop" + i + ".jpg",
						ParentDirectory = "/cities",
						FileHash = "cityloop" + i,
						// ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
						Tags = "cityloop",
						Id = 5000 + i,
						DateTime = new DateTime(2018,1,1,1,1,1, 
							kind: DateTimeKind.Local)
					});
				}
			}
		}

		[TestMethod]
		public async Task SearchService_CacheInjection_CacheTest()
		{
			var search = new SearchService(_dbContext,new FakeIWebLogger(),_memoryCache);

			// fill cache with data real data
			var result = await search.Search("test");
			Assert.AreEqual("test",result.SearchQuery);

			// now update only the name direct in the cache
			_memoryCache.Set("search-test", new SearchViewModel{SearchQuery = "cache"}, 
				new TimeSpan(0,10,0));
		    
			// now query again
			result = await search.Search("test");
			// and get the cached value
			Assert.AreEqual("cache",result.SearchQuery);
		}

		[TestMethod]
		public async Task SearchService_RemoveCache_Test()
		{
			_memoryCache.Set("search-test", new SearchViewModel {SearchQuery = "cache"},
				new TimeSpan(0, 10, 0));
			var search = new SearchService(_dbContext, new FakeIWebLogger(), _memoryCache);
		    
			// Test if the pre-condition is good
			var cachedResult = (await search.Search("test")).SearchQuery;
			Assert.AreEqual("cache",cachedResult);
			
			// Remove cache
			search.RemoveCache("test");
			
			// test if cache is removed
			var result= await search.Search("test");
			Assert.AreEqual("test",result.SearchQuery);
		}

		[TestMethod]
		public void SearchService_RemoveCache_Disabled_Test()
		{
			var search = new SearchService(_dbContext,new FakeIWebLogger()); // cache is null!
			Assert.AreEqual(null,search.RemoveCache("test"));
		}
	    
		[TestMethod]
		public void SearchService_RemoveCache_Disabled_AppSettings_Test()
		{
			var search = new SearchService(_dbContext,new FakeIWebLogger() ,_memoryCache, new AppSettings{AddMemoryCache = false}); 
			Assert.AreEqual(null,search.RemoveCache("test"));
		}
	    
		[TestMethod]
		public void SearchService_RemoveCache_NoCachedItem_Test()
		{
			var search = new SearchService(_dbContext,new FakeIWebLogger(),_memoryCache);
			Assert.AreEqual(false,search.RemoveCache("test"));
		}

		[TestMethod]
		public async Task SearchService_SearchNull()
		{
			await InsertSearchData();
			Assert.AreEqual(0, (await _search.Search(null!)).SearchCount);
		}
        
		[TestMethod]
		public async Task SearchService_SearchCountStationTest()
		{
			await InsertSearchData();
			// With deleted files & xmp file is it 4
			
			Assert.AreEqual(2, (await _search.Search("station")).SearchCount);
		}
		
		[TestMethod]
		public async Task SearchService_ShouldShowXmpFile()
		{
			await InsertSearchData();
			// With deleted files & xmp file is it 4
			
			Assert.AreEqual(1, (await _search.Search("-imageformat:xmp")).SearchCount);
		}
		
		[TestMethod]
		public async Task SearchService_ShouldShowXmpORJpegFile()
		{
			await InsertSearchData();
			Assert.AreEqual(4, (await _search.Search("-imageformat:xmp,jpg")).SearchCount);
		}
		
		[TestMethod]
		public async Task SearchService_ShouldShowXmpORJpegFile_WithFileName()
		{
			await InsertSearchData();
			
			Assert.AreEqual(2, (await _search.Search("-filename:lelystadcentrum -imageformat:xmp,jpg")).SearchCount);
		}
		
		[TestMethod]
		public async Task SearchService_ShouldShowXmpORJpegFile_WithFileName2()
		{
			await InsertSearchData();
			
			var result = await _search.Search("-filePath:/stations/lelystadcentrum -imageformat:\"xmp,jpg\"");
			
			Assert.AreEqual(2, result.SearchCount);
		}
		
		[TestMethod]
		public async Task SearchService_ShouldShowXmpORJpegFile_Not_WithFileName()
		{
			await InsertSearchData();
			
			var result = await _search.Search("-filePath:/stations/lelystadcentrum -imageformat-\"xmp,jpg\"");
			
			Assert.AreEqual(0, result.SearchCount);
		}
		
		[TestMethod]
		public async Task SearchService_SoftwareSearch()
		{
			await InsertSearchData();
        
			Assert.AreEqual(1, (await _search.Search("-software:photo")).SearchCount);
		}

		[TestMethod]
		public async Task SearchService_SearchLastPageCityloopTest()
		{
			await InsertSearchData();
			var result = await _search.Search("cityloop");
			Assert.AreEqual(2, result.LastPageNumber);
		}
	    
	    
		[TestMethod]
		public async Task SearchService_SearchForDatetime()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-Datetime=\"2016-01-01 01:01:01\"")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchForDatetimeSmallerThen()
		{
			await InsertSearchData();
			Assert.AreEqual(2, (await _search.Search("-Datetime<\"2013-01-01 02:01:01\"")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchForLastEdited()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-LastEdited=\"2020-10-10 10:10:10\"")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchForDatetimeGreaterThen()
		{
			await InsertSearchData();
			Assert.AreEqual(NumberOfFakeResults+1, (await _search.Search("-Datetime>\"2018-01-01 01:01:01\"")).SearchCount);
		}

		[TestMethod]
		public async Task SearchSchipholDescriptionTest()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-Description:Schiphol")).SearchCount);
		}
   
		[TestMethod]
		public async Task SearchService_SearchSchipholFilenameTest()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-filename:'schipholairplane.jpg'")).SearchCount);
		}
        
		[TestMethod]
		public async Task SearchService_SearchSchipholTitleTest()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-title:Schiphol")).SearchCount);
		}
        
		[TestMethod]
		public async Task SearchService_Make_Search()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-Make:Apple")).SearchCount);
			// schipholairplane.jpg
		}

		[TestMethod]
		public async Task SearchService_Make_SearchNonExist()
		{
			await InsertSearchData();
			
			var result = await _search.Search("-Make:SE");
			Assert.AreEqual(0, result.SearchCount);
			// NOT schipholairplane.jpg
		}
        
		[TestMethod]
		public async Task SearchService_Model_Search()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-Model:SE")).SearchCount);
			// schipholairplane.jpg
		}
        
		[TestMethod]
		public async Task SearchService_NotQueryCityloop240()
		{
			await InsertSearchData();
			Assert.AreEqual(NumberOfFakeResultsThatFitOnPage, (await _search.Search("-filehash:cityloop -filehash-cityloop240")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchSchipholFileHashTest()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-filehash:schipholairplane")).SearchCount);
		}
	    
	    
		[TestMethod]
		public async Task SearchService_SearchCityloopTest()
		{
			await InsertSearchData();
			Assert.AreEqual(NumberOfFakeResults, (await _search.Search("cityloop")).SearchCount);
		}
        
		[TestMethod]
		public async Task SearchService_SearchStationLelystadTest()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("station lelystad")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchStationLelystadQuotedTest()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("\"station\" \"lelystad\"")).SearchCount);
		}

		[TestMethod]
		public async Task SearchService_SearchParenthesisTreinTest()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("\"de trein\"")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchIOSDoubleParenthesisTreinTest()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search($"“{TrashKeyword.TrashKeywordString}”")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchIOSSingleParenthesisTreinTest()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search($"‘{TrashKeyword.TrashKeywordString}’")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchNonParenthesisTreinTest()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("de trein")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchCityLoopCaseSensitiveTest()
		{
			await InsertSearchData();
			//    Check case sensitive!
			Assert.AreEqual(NumberOfFakeResults, (await _search.Search("CityLoop")).SearchCount);
		}

		[TestMethod]
		public async Task SearchService_SearchCityLoopTrimTest()
		{
			// Test TRIM
			await InsertSearchData();
			Assert.AreEqual(NumberOfFakeResults, (await _search.Search("   cityloop    ")).SearchCount);
		}
        
		[TestMethod]
		public async Task SearchService_SearchCityLoopFilePathTest()
		{
			await InsertSearchData();
			Assert.AreEqual(NumberOfFakeResults, (await _search.Search("-FilePath:cityloop")).SearchCount);
		}
        
		[TestMethod]
		public async Task SearchService_SearchCityloopParentDirectoryTest()
		{
			await InsertSearchData();
			Assert.AreEqual(NumberOfFakeResults, (await _search.Search("-ParentDirectory:/cities")).SearchCount);
		}
	    
	    
		[TestMethod]
		public async Task SearchService_SearchForDirectories()
		{
			await InsertSearchData();
		    
			Assert.AreEqual(1, (await _search.Search("-isDirectory:true -inurl:stations")).SearchCount);
		}
        
		[TestMethod]
		public async Task SearchService_SearchInUrlTest()
		{
			await InsertSearchData();
			// Not 3, because one file is marked as deleted!
			// todo: check the value of this one
			Assert.AreEqual(5, (await _search.Search("-inurl:/stations")).SearchCount);
			Assert.AreEqual(5, (await _search.Search("-inurl:\"/stations\"")).SearchCount);
		}

		[TestMethod]
		public async Task SearchService_SearchNarrowFileNameTags()
		{
			await InsertSearchData();
			// Not 2 > but needs to be narrow
			// todo: check the value of this one
			Assert.AreEqual(1, (await _search.Search("lelystad -ParentDirectory:/stations2")).SearchCount);
		}
        
		[TestMethod]
		public async Task SearchService_SearchNarrow2FileNameTags()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-ParentDirectory:/station -ParentDirectory:2")).SearchCount);
		}

		[TestMethod]
		public async Task SearchService_SearchForDateTimeBetween()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-DateTime>2015-01-01T01:01:01 -DateTime<2017-01-01T01:01:01")).SearchCount);
			// lowercase is the same
			Assert.AreEqual(1, (await _search.Search("-DateTime>2015-01-01t01:01:01 -DateTime<2017-01-01t01:01:01")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchOnOnlyDay()
		{
			await InsertSearchData();

			var item = await _search.Search("-DateTime=2016-01-01");
			Assert.AreEqual(1, item.SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchOnToday()
		{
			await InsertSearchData();

			var item = await _search.Search("-DateTime=0");
			Assert.AreEqual(1, item.SearchCount);
		}
        
		[TestMethod]
		public async Task SearchService_SearchAddToDatabase()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-addtodatabase>2015-01-01T01:01:01 -addtodatabase<2017-01-01T01:01:01")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchForImageFormat()
		{
			await InsertSearchData();
			Assert.AreEqual(3, (await _search.Search("-ImageFormat:jpg")).SearchCount);
		}
	    
		[TestMethod]
		public async Task SearchService_SearchForColorClass()
		{
			await InsertSearchData();
			Assert.AreEqual(1, (await _search.Search("-ColorClass:2")).SearchCount);
			// red aka WinnerAlt
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

			Assert.AreEqual(true, model.SearchIn.Contains("Tags"));
			Assert.AreEqual(true, model.SearchFor.Contains("dion.jpg"));
		}
		
		[TestMethod]
		public void SearchService_NullQuery()
		{
			var model = new SearchViewModel
			{
				SearchQuery = null
			};
			_search.MatchSearch(model);

			Assert.AreEqual(false, model.SearchIn.Count != 0);
		}

		[TestMethod]
		public void SearchService_MatchSearchOneKeywordsTest()
		{
			// Single keyword
			var model = new SearchViewModel {SearchQuery = "-Tags:dion"};
			_search.MatchSearch(model);
			Assert.AreEqual(true, model.SearchIn.Contains("Tags"));
		}

		[TestMethod]
		public async Task SearchService_SearchPageTypeTest_StringEmpty()
		{
			var model = await _search.Search();
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
			Assert.AreEqual(true, model.SearchIn.Contains("FileName"));
			Assert.AreEqual(true, model.SearchIn.Contains("Tags"));
		}

		[TestMethod]
		public void SearchService_QuerySafeTest()
		{
			var query = SearchService.QuerySafe("   d   ");
			Assert.AreEqual("d",query);
		}

		[TestMethod]
		public void SearchService_QueryShortcutsInurlTest()
		{
			var query = SearchService.QueryShortcuts("-inurl");
			Assert.AreEqual("-FilePath",query);
		}

		[TestMethod]
		public void SearchService_MatchSearchDefaultOptionTest()
		{
			// Single keyword
			var model = new SearchViewModel {SearchQuery = "test"};
			_search.MatchSearch(model);
			Assert.AreEqual(true, model.SearchIn.Contains("Tags"));
		}

		[TestMethod]
		public async Task SearchService_SearchForDeletedFiles()
		{
			await InsertSearchData();

			var del = await _search.Search(TrashKeyword.TrashKeywordString);
			var count = del.FileIndexItems?.Count;
			Assert.AreEqual(1,count);
			Assert.AreEqual("stationdeletedfile", del.FileIndexItems?.FirstOrDefault()?.FileHash);
		}

		[TestMethod]
		public void SearchService_RoundDownTest()
		{
			Assert.AreEqual(10,SearchService.RoundDown(12));
		}
        
		[TestMethod]
		public void SearchService_RoundUpTest()
		{
			Assert.AreEqual(120,SearchService.RoundUp(8)); // NumberOfResultsInView
		}

		[TestMethod]
		public async Task SearchService_cacheTest()
		{
			var fakeCache =
				new FakeMemoryCache(new Dictionary<string, object>{{"search-t",
						new SearchViewModel { FileIndexItems = new List<FileIndexItem>{ 
							new FileIndexItem {Tags = "t"}}
						}} 
				});
			var searchService = new SearchService(_dbContext,new FakeIWebLogger(),fakeCache);
			var result = await searchService.Search("t"); // <= t is only to detect in fakeCache
			Assert.AreEqual(1,result.FileIndexItems?.Count);
		}
	    

		[TestMethod]
		public async Task SearchService_thisORThisFileHashes()
		{
			await InsertSearchData();
			var result = await _search.Search("-FileHash=stationdeletedfile || -FileHash=lelystadcentrum2",0,false);
			Assert.AreEqual(2,result.FileIndexItems?.Count);
		}
	    
		[TestMethod]
		public async Task SearchService_DescriptionImageFormat()
		{
			await InsertSearchData();
			var result = await _search.Search("-Description=lelystadcentrum2 -ImageFormat=tiff",0,false);
			Assert.AreEqual(1,result.FileIndexItems?.Count);
		}
		
		[TestMethod]
		public async Task SearchService_DescriptionMultipleImageFormats()
		{
			await InsertSearchData();
			var result = await _search.Search("-FileHash=lelystadcentrum -ImageFormat=jpg || -ImageFormat=xmp",0,false);
			// not working at the moment
			Assert.AreEqual(0,result.FileIndexItems?.Count);
		}
		
		[TestMethod]
		public async Task SearchService_DescriptionOne()
		{
			await InsertSearchData();
			var result = await _search.Search("-Description=lelystadcentrum2",0,false);
			Assert.AreEqual(1,result.FileIndexItems?.Count);
		}

		[TestMethod]
		public async Task SearchService_thisORAndCombination()
		{
			await InsertSearchData();
			var result = await _search.Search("-FileName=lelystadcentrum.jpg || -FileHash=lelystadcentrum && lelystad",
				0,false);
			//  -FileHash=lelystadcentrum2 && station >= 1 item
			// -DateTime=lelystadcentrum2.jpg >= 1 item
			// the and applies to all previous items
			// lelystadcentrum && lelystadcentrum2 are items
			// station = duplicate in this example but triggers other results when using || instead of &&
			Assert.AreEqual(2,result.FileIndexItems?.Count);
		}

		[TestMethod]
		public async Task SearchService_thisORDefaultKeyword()
		{
			await InsertSearchData();
			var result = await _search.Search("station || lelystad",0,false);
			Assert.AreEqual(3,result.FileIndexItems?.Count);
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
			const string modelSearchQuery = " \"station test\" key2";
			var result = new SearchViewModel().ParseDefaultOption(modelSearchQuery);
			Assert.AreEqual("-Tags:\"station test\" -Tags:\"key2\" ",result);
		}
	    
		[TestMethod]
		public void SearchViewModel_SearchOperatorOptions_Quoted_with_ParseDefaultOption()
		{
			const string modelSearchQuery = " \"station test\" \"station test\"";
			var searchViewModel = new SearchViewModel();
		    
			searchViewModel.ParseDefaultOption(modelSearchQuery);
		    
			var searchOperatorOptions = searchViewModel.SearchOperatorOptions;
		    
			Assert.AreEqual(true,searchOperatorOptions[0]);
			Assert.AreEqual(true,searchOperatorOptions[1]);
		}

		[TestMethod]
		public void SearchViewModel_SearchOperatorOptions_NonQuoted_with_ParseDefaultOption()
		{
			const string modelSearchQuery = "station test";
			var searchViewModel = new SearchViewModel();
		    
			searchViewModel.ParseDefaultOption(modelSearchQuery);
		    
			var searchOperatorOptions = searchViewModel.SearchOperatorOptions;
		    
			Assert.AreEqual(true,searchOperatorOptions[0]);
			Assert.AreEqual(true,searchOperatorOptions[1]);
		}

		[TestMethod]
		public void SearchViewModel_SearchOperatorOptions_NonQuoted_with_OR_Situation_ParseDefaultOption()
		{
			const string modelSearchQuery = "station || test";
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
			const string modelSearchQuery = "query of";
			var searchViewModel = new SearchViewModel();
			searchViewModel.ParseDefaultOption(modelSearchQuery);
			Assert.AreEqual("query", searchViewModel.SearchFor[0]);
			Assert.AreEqual("of", searchViewModel.SearchFor[1]);
		}
	    
		[TestMethod]
		public void SearchViewModel_TwoCharWords_ShortWord()
		{
			// two chars used have an exception
			const string modelSearchQuery = "ns";
			var searchViewModel = new SearchViewModel();
			searchViewModel.ParseDefaultOption(modelSearchQuery);

			Assert.AreEqual(SearchViewModel.SearchForOptionType.Equal, searchViewModel.SearchForOptions[0]);
			Assert.AreEqual("Tags", searchViewModel.SearchIn[0]);
			Assert.AreEqual("ns", searchViewModel.SearchFor[0]);
		}

		[TestMethod]
		public void SearchViewModel_ParseDateTimeLowInt()
		{
			var p = SearchViewModel.ParseDateTime("0");
			// today
			Assert.AreEqual(p.Day,DateTime.Now.Day);
			Assert.AreEqual(p.Month,DateTime.Now.Month);
		}
	    
	    
		[TestMethod]
		public void SearchViewModel_ParseDateTimeLargeInt()
		{
			var p = SearchViewModel.ParseDateTime("20180911");
			// defaults to today
			Assert.AreEqual(p.Day,DateTime.Now.Day);
			Assert.AreEqual(p.Month,DateTime.Now.Month);
		}
	    
		[TestMethod]
		public void SearchViewModel_ParseDateTimeExample()
		{
			var p = SearchViewModel.ParseDateTime("2018-09-11");
			// defaults to today
			Assert.AreEqual(DateTime.Parse("2018-09-11", CultureInfo.InvariantCulture),p);
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

			var result = SearchViewModel.NarrowSearch(model);
			Assert.AreEqual(2,result.FileIndexItems?.Count);

			// Add extra NOT query			
			model.SearchIn.Add("tags");
			model.SetAddSearchFor("lelystadcentrum2"); // not query
			model.SetAddSearchForOptions("-");

			SearchViewModel.NarrowSearch(model);

			Assert.AreEqual("lelystadcentrum",result.FileIndexItems?[0].Tags);
		}
		
		[TestMethod]
		public void SearchService_SoftwareNull()
		{
			var model = new SearchViewModel
			{
				SearchIn =
				{
					"software"
				},
				FileIndexItems = new List<FileIndexItem>{
					new FileIndexItem
					{
						Tags = "software:test123"
					},
					new FileIndexItem
					{
						Tags = "software:test123"
					},
					new FileIndexItem
					{
						Tags = "lelystadcentrum",
						Software = "test123"
					}
				}
			};
			
			model.SetAddSearchFor("test1"); 
			model.SetAddSearchForOptions("=");

			var result = SearchViewModel.NarrowSearch(model);
			Assert.AreEqual(1,result.FileIndexItems?.Count);
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

			var result = SearchViewModel.NarrowSearch(model);

			Assert.AreEqual(1,result.FileIndexItems?.Count);
			Assert.AreEqual("lelystadcentrum2",result.FileIndexItems?[0].Tags);
		}

		[TestMethod]
		[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
		public async Task SearchService_Search_Percentage()
		{
			var results = await _search.Search("%", 0);
			Assert.AreEqual(0,results.FileIndexItems?.Count);
			Assert.AreEqual("%",results.SearchQuery);
		}


		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public async Task SearchService_Search_ToLong_ArgumentException()
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

			await _search.Search(longTestText);
			// Expect ArgumentException
		}

	}
}

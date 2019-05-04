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

namespace starskytest.Services
{
	[TestClass]
	public class SearchSuggestionsServiceTest
	{
		private SearchSuggestionsService _suggest;
		private Query _query;
		private ApplicationDbContext _dbContext;
		private IMemoryCache _memoryCache;
		
		public SearchSuggestionsServiceTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("searchService");
			var options = builder.Options;
			_dbContext = new ApplicationDbContext(options);
			_suggest = new SearchSuggestionsService(_dbContext,_memoryCache,new AppSettings());
			_query = new Query(_dbContext,_memoryCache);
			
			
		}
		
		[TestInitialize]
		public void TestInitialize()
		{

			if (string.IsNullOrEmpty(_query.GetSubPathByHash("schipholairplane")))
			{
				_query.AddItem(new FileIndexItem
				{
					FileName = "schipholairplane.jpg",
					ParentDirectory = "/stations",
					FileHash = "schipholairplane",
					Tags = "schiphol, airplane, zebra",
				});
			}

			if (string.IsNullOrEmpty(_query.GetSubPathByHash("lelystadcentrum")))
			{
				_query.AddItem(new FileIndexItem
				{
					FileName = "schipholairplane1.jpg",
					ParentDirectory = "/stations",
					Tags = "schiphol, airplane",
					DateTime = DateTime.Now
				});
			}

		}
		
		[TestCleanup]
		public void TestCleanup()
		{
			
		}
		
		[TestMethod]
		public void SearchSuggestionsService_MoreThat2_StartArray()
		{

			// Schiphol is 2 times in the list
			var result = _suggest.SearchSuggest("sch").ToList();
			
			Assert.AreEqual("schiphol", result.FirstOrDefault());
			
		}
		
		[TestMethod]
		public void SearchSuggestionsService_MoreThat2_MiddleArray()
		{

			// airplane is 2 times in the list + middle of array
			var result = _suggest.SearchSuggest("airpl").ToList();
			
			Assert.AreEqual("airplane", result.FirstOrDefault());
			
		}
		[TestMethod]
		public void SearchSuggestionsService_Once()
		{

			// zebra is 1 time in the list
			var result = _suggest.SearchSuggest("zebr").ToList();
			
			Assert.AreEqual(0, result.Count);
			
		}
		
		
		[TestMethod]
		public void SearchSuggestionsService_NoCache_memCacheIsNull()
		{
			// The feature does not work without cache enabled
			var result = new SearchSuggestionsService(_dbContext,null,null)
				.SearchSuggest("sch").ToList();

			Assert.AreEqual(0, result.Count);
		
		}
		
		[TestMethod]
		public void SearchSuggestionsService_NoCache_AppSettingsDisabled()
		{
			// The feature does not work without cache enabled
			var result = new SearchSuggestionsService(_dbContext,_memoryCache,
				new AppSettings{AddMemoryCache = false}).SearchSuggest("sch").ToList();

			Assert.AreEqual(0, result.Count);
		
		}

	}
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starsky.feature.search.Services;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.search.Services
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
			builder.UseInMemoryDatabase("searchSuggestionService");
			var options = builder.Options;
			_dbContext = new ApplicationDbContext(options);
			_suggest = new SearchSuggestionsService(_dbContext,_memoryCache,new FakeIWebLogger(),new AppSettings());
			_query = new Query(_dbContext, new AppSettings(), null, new FakeIWebLogger(),_memoryCache);
		}
		
		[TestInitialize]
		public void TestInitialize()
		{

			if (string.IsNullOrEmpty(_query.GetSubPathByHash("schipholairplane2--suggestions")))
			{
				for ( int i = 0; i < 9; i++ )
				{
					_query.AddItem(new FileIndexItem
					{
						FileName = "schipholairplane2.jpg",
						ParentDirectory = "/stations",
						FileHash = "schipholairplane2--suggestions",
						Tags = "schiphol, airplane, zebra",
					});
				}
			}

			if (string.IsNullOrEmpty(_query.GetSubPathByHash("schipholairplane1")))
			{
				for ( int i = 0; i < 9; i++ )
				{
					_query.AddItem(new FileIndexItem
					{
						FileName = "schipholairplane1.jpg",
						ParentDirectory = "/stations",
						FileHash = "schipholairplane1--suggestions",
						Tags = "schiphol, airplane",
						DateTime = DateTime.Now
					});
				}

			}

		}
		
		[TestCleanup]
		public void TestCleanup()
		{
			
		}
		
		[TestMethod]
		public async Task SearchSuggestionsService_MoreThat2_StartArray()
		{
			// Schiphol is 10 times in the list
			var result = await _suggest.SearchSuggest("sch");
			
			Assert.AreEqual("schiphol", result.FirstOrDefault());
			
		}
		
		[TestMethod]
		public async Task SearchSuggestionsService_MoreThat2_MiddleArray()
		{

			// airplane is 10 times in the list + middle of array
			var result = await _suggest.SearchSuggest("airpl");
			
			Assert.AreEqual("airplane", result.FirstOrDefault());
			
		}
		[TestMethod]
		public async Task SearchSuggestionsService_Once()
		{

			// zebra is 1 time in the list
			var result = await _suggest.SearchSuggest("zebr");
			
			Assert.AreEqual(0, result.Count());
			
		}
		
		
		[TestMethod]
		public async Task SearchSuggestionsService_NoCache_memCacheIsNull()
		{
			// The feature does not work without cache enabled
			var result = await new SearchSuggestionsService(_dbContext, null, null)
				.SearchSuggest("sch");

			Assert.AreEqual(0, result.Count());
		
		}
		
		[TestMethod]
		public async Task SearchSuggestionsService_NoCache_AppSettingsDisabled()
		{
			// The feature does not work without cache enabled
			var result = await new SearchSuggestionsService(_dbContext,_memoryCache,
				new FakeIWebLogger(),
				new AppSettings{AddMemoryCache = false}).SearchSuggest("sch");

			Assert.AreEqual(0, result.Count());
		
		}
		
		[TestMethod]
		public async Task SearchSuggestionsService_MySqlError()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseMySql("Server=test;database=test;uid=test;pwd=test;", new MariaDbServerVersion("10.2"));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			var fakeLogger = new FakeIWebLogger();
			var suggest = new SearchSuggestionsService(dbContext,_memoryCache,fakeLogger,new AppSettings());

			await suggest.Inflate();
			
			Assert.AreEqual("mysql search suggest exception catch-ed", fakeLogger.TrackedExceptions.LastOrDefault().Item2);
		}

	}
}

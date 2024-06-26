using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.search.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.search.Services
{
	[TestClass]
	public sealed class SearchSuggestionsServiceTest
	{
		private readonly SearchSuggestionsService _suggest;
		private readonly Query _query;
		private readonly ApplicationDbContext _dbContext;
		private readonly IMemoryCache _memoryCache;
		
		public SearchSuggestionsServiceTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetRequiredService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("searchSuggestionService");
			var options = builder.Options;
			_dbContext = new ApplicationDbContext(options);
			_suggest = new SearchSuggestionsService(_dbContext,_memoryCache,new FakeIWebLogger(),new AppSettings());
			_query = new Query(_dbContext, new AppSettings(), null!, new FakeIWebLogger(),_memoryCache);
		}
		
		[TestInitialize]
		public async Task TestInitialize()
		{

			if (string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("schipholairplane2--suggestions")))
			{
				for ( int i = 0; i < 9; i++ )
				{
					await _query.AddItemAsync(new FileIndexItem
					{
						FileName = "schipholairplane2.jpg",
						ParentDirectory = "/stations",
						FileHash = "schipholairplane2--suggestions",
						Tags = "schiphol, airplane, zebra",
					});
				}
			}

			if (string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("schipholairplane1")))
			{
				for ( int i = 0; i < 9; i++ )
				{
					await _query.AddItemAsync(new FileIndexItem
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
			var result = await new SearchSuggestionsService(_dbContext, null, null!, new AppSettings())
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
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			// this database does NOT exists! and should also not exists
			builder.UseMySql("Server=127.0.0.1;port=7544;database=test;uid=test;pwd=test;", new MariaDbServerVersion("10.2"));
			var options = builder.Options;
			var dbContext = new ApplicationDbContext(options);
			var fakeLogger = new FakeIWebLogger();
			var suggest = new SearchSuggestionsService(dbContext,_memoryCache,fakeLogger,new AppSettings());

			await suggest.Inflate();
			
			Assert.IsTrue(fakeLogger.TrackedExceptions.LastOrDefault().Item2?.Contains("[SearchSuggestionsService] exception catch-ed"));
		}

	}
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starsky.feature.search.Services;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public sealed class SearchSuggestControllerTest
	{
		private SearchSuggestionsService _searchSuggest;
		private Query _query;
		private IMemoryCache _memoryCache;

		public SearchSuggestControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();

			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(SearchSuggestController));
			var options = builder.Options;
			var context = new ApplicationDbContext(options);
			_query = new Query(context, new AppSettings(), null!, new FakeIWebLogger(), _memoryCache);
			_searchSuggest = new SearchSuggestionsService(context,_memoryCache, null!, new AppSettings());
		}

		private void InjectMockedData()
		{
			if (!string.IsNullOrEmpty(_query.GetSubPathByHash("hash_9"))) return;
			    
			for ( var i = 0; i < 10; i++ )
			{
				var tags = i % 2 == 1 ? "enter, sandman, exit, live" : "enter, sandman, exit";
				_query.AddItem(new FileIndexItem
				{
					FileName = $"{i}.jpg",
					FileHash = $"hash_{i}",
					ParentDirectory = "/",
					Tags = tags
				});
			}
		}

		[TestMethod]
		public async Task Suggestion_IsMoreThan10()
		{
			InjectMockedData();
			var controller = new SearchSuggestController(_searchSuggest);
			var result = await controller.Suggest("e") as JsonResult;
			var list = result!.Value as List<string>;
			CollectionAssert.AreEqual(new List<string>{"enter","exit"}, list);
		}

		[TestMethod]
		public async Task Suggestion_Nothing()
		{
			var controller = new SearchSuggestController(_searchSuggest)
			{
				ControllerContext = {HttpContext = new DefaultHttpContext()}
			};
			var result = await controller.Suggest(string.Empty) as JsonResult;
			var list = result!.Value as List<string>;
	        
			Assert.IsFalse(list!.Any());
		}

		[TestMethod]
		public async Task Suggestion_IsLessThan10()
		{
			InjectMockedData();
			var controller = new SearchSuggestController(_searchSuggest);
			var result = await controller.Suggest("l") as JsonResult; // search for live
			var list = result!.Value as List<string>;
			CollectionAssert.AreEqual(new List<string>(), list);
		}
        
		[TestMethod]
		public async Task AllResultsCheck()
		{
			InjectMockedData();
			var controller = new SearchSuggestController(_searchSuggest);
			var result = await controller.All() as JsonResult; // search for live
			var list = result!.Value as List<KeyValuePair<string, int>>;

			var expected = new List<KeyValuePair<string, int>>
			{
				new KeyValuePair<string, int>("enter", 10),
				new KeyValuePair<string, int>("sandman", 10),
				new KeyValuePair<string, int>("exit", 10)
			};
	        
			CollectionAssert.AreEqual(expected, list);
		}
        
		[TestMethod]
		public async Task Inflate_HappyFlow()
		{
			// Clean cache if not exist
			_memoryCache.Remove(nameof(SearchSuggestionsService));
	        
			InjectMockedData();
			var controller = new SearchSuggestController(_searchSuggest);
			await controller.Inflate();
	        
			_memoryCache.TryGetValue(nameof(SearchSuggestionsService),
				out var cacheResult);

			var keyValuePairs = cacheResult as List<KeyValuePair<string, int>>;
			var expected = new List<KeyValuePair<string, int>>
			{
				new KeyValuePair<string, int>("enter", 10),
				new KeyValuePair<string, int>("sandman", 10),
				new KeyValuePair<string, int>("exit", 10)
			};
			CollectionAssert.AreEqual(expected, keyValuePairs);
		}
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.search.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class SearchSuggestControllerTest
{
	private readonly IMemoryCache _memoryCache;
	private readonly Query _query;
	private readonly SearchSuggestionsService _searchSuggest;

	public SearchSuggestControllerTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		_memoryCache = provider.GetRequiredService<IMemoryCache>();

		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase(nameof(SearchSuggestController));
		var options = builder.Options;
		var context = new ApplicationDbContext(options);
		_query = new Query(context, new AppSettings(), null!, new FakeIWebLogger(), _memoryCache);
		_searchSuggest =
			new SearchSuggestionsService(context, _memoryCache, null!, new AppSettings());
	}

	private async Task InjectMockedData()
	{
		if ( !string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("hash_9")) )
		{
			return;
		}

		for ( var i = 0; i < 10; i++ )
		{
			var tags = i % 2 == 1 ? "enter, sandman, exit, live" : "enter, sandman, exit";
			await _query.AddItemAsync(new FileIndexItem
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
		await InjectMockedData();
		var controller = new SearchSuggestController(_searchSuggest);
		var result = await controller.Suggest("e") as JsonResult;
		var list = result!.Value as List<string>;
		CollectionAssert.AreEqual(new List<string> { "enter", "exit" }, list);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow(" ")]
	public async Task Suggestion_Nothing(string t)
	{
		var controller = new SearchSuggestController(_searchSuggest)
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};
		
		// this will hit ModelState.IsValid
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		
		var result = await controller.Suggest(t) as JsonResult;
		var list = result!.Value as List<string>;

		Assert.AreEqual(0, list!.Count);
	}

	[TestMethod]
	public async Task Suggest_InvalidModel()
	{
		var controller = new SearchSuggestController(_searchSuggest)
		{
			ControllerContext = { HttpContext = new DefaultHttpContext() }
		};
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		var result = await controller.Suggest("Invalid");
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task Suggestion_IsLessThan10()
	{
		await InjectMockedData();
		var controller = new SearchSuggestController(_searchSuggest);
		var result = await controller.Suggest("l") as JsonResult; // search for live
		var list = result!.Value as List<string>;
		CollectionAssert.AreEqual(new List<string>(), list);
	}

	[TestMethod]
	public async Task AllResultsCheck()
	{
		await InjectMockedData();
		var controller = new SearchSuggestController(_searchSuggest);
		var result = await controller.All() as JsonResult; // search for live
		var list = result!.Value as List<KeyValuePair<string, int>>;

		var expected = new List<KeyValuePair<string, int>>
		{
			new("enter", 10), new("sandman", 10), new("exit", 10)
		};

		CollectionAssert.AreEqual(expected, list);
	}

	[TestMethod]
	public async Task Inflate_HappyFlow()
	{
		// Clean cache if not exist
		_memoryCache.Remove(nameof(SearchSuggestionsService));

		await InjectMockedData();
		var controller = new SearchSuggestController(_searchSuggest);
		await controller.Inflate();

		_memoryCache.TryGetValue(nameof(SearchSuggestionsService),
			out var cacheResult);

		var keyValuePairs = cacheResult as List<KeyValuePair<string, int>>;
		var expected = new List<KeyValuePair<string, int>>
		{
			new("enter", 10), new("sandman", 10), new("exit", 10)
		};
		CollectionAssert.AreEqual(expected, keyValuePairs);
	}
}

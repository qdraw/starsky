using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;
using System.Collections.Generic;
using System.Linq;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public class QueryTestAddCacheItemDisposed
{
	[TestMethod]
	public void AddCacheItem_WhenCacheIsDisposed_CreatesNewCacheScope()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddMemoryCache();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase("AddCacheItemDisposedTest"));
		var serviceProvider = services.BuildServiceProvider();
		var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings { AddMemoryCache = true };
		var cache = serviceProvider.GetRequiredService<IMemoryCache>();
		var query = new Query(dbContext, appSettings, scopeFactory, logger, cache);

		var fileIndexItem = new FileIndexItem
		{
			FileName = "test.jpg", ParentDirectory = "/test", FilePath = "/test/test.jpg"
		};
		var cacheKey = Query.CachingDbName(nameof(FileIndexItem),
			fileIndexItem.ParentDirectory!);

		// Dispose the cache to simulate the error
		cache.Dispose();

		// Act & Assert: Should not throw
		query.AddCacheItem(fileIndexItem);

		// Get a new cache from a new scope to check if the item was added
		using var newScope = scopeFactory.CreateScope();
		var newCache = newScope.ServiceProvider.GetRequiredService<IMemoryCache>();
		var cacheItems = newCache.Get(cacheKey) as List<FileIndexItem>;
		Assert.IsNotNull(cacheItems);
		Assert.IsTrue(cacheItems.Any(i => i.FilePath == fileIndexItem.FilePath));
	}
}

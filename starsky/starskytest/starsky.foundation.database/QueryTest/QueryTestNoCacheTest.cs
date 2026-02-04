using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public sealed class QueryTestNoCacheTest
{
	private readonly Query _query;

	public QueryTestNoCacheTest()
	{
		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase("QueryTestNoCacheTest");
		var options = builder.Options;
		var context = new ApplicationDbContext(options);
		var logger = new FakeIWebLogger();
		_query = new Query(context, new AppSettings(), null, logger);
	}

	[TestMethod]
	public async Task QueryNoCache_SingleItem_Test()
	{
		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "nocache.jpg",
			ParentDirectory = "/nocache",
			FileHash = "eruiopds",
			ColorClass = ColorClassParser.Color.Winner, // 1
			Tags = "",
			Title = ""
		});

		var singleItem = _query.SingleItem("/nocache/nocache.jpg")?.FileIndexItem;
		Assert.AreEqual("/nocache/nocache.jpg", singleItem?.FilePath);
	}

	[TestMethod]
	public void Query_IsCacheEnabled_False()
	{
		Assert.IsFalse(_query.IsCacheEnabled());
	}

	[TestMethod]
	public void RemoveCacheItem_Disabled()
	{
		var updateStatusContent = new List<FileIndexItem>();
		_query.RemoveCacheItem(updateStatusContent);
		// it should not crash
		Assert.IsNotNull(updateStatusContent);
	}

	[TestMethod]
	public void AddCacheItem_WhenCacheIsDisposed()
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

		// Dispose the cache to simulate the error
		cache.Dispose();

		// Act & Assert: Should not throw
		query.AddCacheItem(fileIndexItem);

		// Verify that the item was added to the cache
		var lastLog = logger.TrackedExceptions.LastOrDefault().Item2 ?? string.Empty;
		Assert.Contains("[AddCacheItem] ObjectDisposedException cache is broken", lastLog);
	}
}

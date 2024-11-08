using System;
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
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public sealed class QueryGetObjectsByFilePathAsyncTest
{
	private readonly Query _query;
	private IMemoryCache? _memoryCache;

	public QueryGetObjectsByFilePathAsyncTest()
	{
		_query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetRequiredService<ApplicationDbContext>(), new AppSettings(),
			null!, new FakeIWebLogger(), new FakeMemoryCache());
	}

	private IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddMemoryCache();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(QueryGetObjectsByFilePathAsyncTest)));
		var serviceProvider = services.BuildServiceProvider();
		_memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}


	[TestMethod]
	public async Task GetObjectsByFilePathAsync_cache_collectionFalse()
	{
		var query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetRequiredService<ApplicationDbContext>(), new AppSettings(), CreateNewScope(),
			new FakeIWebLogger(), _memoryCache);

		var dirName = "/cache_01";
		query.AddCacheParentItem(dirName,
			new List<FileIndexItem>
			{
				new($"{dirName}/test1.jpg"),
				new($"{dirName}/test1.dng"),
				new($"{dirName}/test2.jpg"),
				new($"{dirName}/test2.dng")
			});

		var result =
			await query.GetObjectsByFilePathAsync(new List<string> { $"{dirName}/test1.jpg" },
				false);
		Assert.AreEqual(1, result.Count);

		Assert.AreEqual($"{dirName}/test1.jpg", result[0].FilePath);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_cache_collectionTrue()
	{
		var query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetRequiredService<ApplicationDbContext>(), new AppSettings(), CreateNewScope(),
			new FakeIWebLogger(), _memoryCache);

		const string dirName = "/cache_02";
		query.AddCacheParentItem(dirName,
			new List<FileIndexItem>
			{
				new($"{dirName}/test1.jpg"),
				new($"{dirName}/test1.dng"),
				new($"{dirName}/test2.jpg"),
				new($"{dirName}/test2.dng")
			});

		var result =
			await query.GetObjectsByFilePathAsync(new List<string> { $"{dirName}/test1.jpg" },
				true);
		Assert.AreEqual(2, result.Count);
		Assert.AreEqual($"{dirName}/test1.jpg", result[0].FilePath);
		Assert.AreEqual($"{dirName}/test1.dng", result[1].FilePath);
	}

	[TestMethod]
	[DataRow(true)]
	[DataRow(false)]
	public async Task GetObjectsByFilePathAsync_collectionTrue_HomeItem(bool isCollection)
	{
		const string dirName = "/";
		var item = new FileIndexItem(dirName);
		await _query.AddItemAsync(item);

		var result =
			await _query.GetObjectsByFilePathAsync(new List<string> { dirName },
				isCollection);

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(dirName, result[0].FilePath);
		
		await _query.RemoveItemAsync(item);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_notImplicitSet_cache_collectionTrue()
	{
		var query = new Query(CreateNewScope().CreateScope().ServiceProvider
				.GetRequiredService<ApplicationDbContext>(), new AppSettings(), CreateNewScope(),
			new FakeIWebLogger(), _memoryCache);

		const string dirName = "/implicit_item_02";
		await query.AddRangeAsync(new List<FileIndexItem>
		{
			new($"{dirName}/test1.jpg"),
			new($"{dirName}/test1.dng"),
			new($"{dirName}/test2.jpg"),
			new($"{dirName}/test2.dng")
		});

		await query.GetObjectsByFilePathAsync(
			new List<string> { $"{dirName}/test1.jpg" }, true);

		var cacheResult = query.CacheGetParentFolder(dirName);

		Assert.IsFalse(cacheResult.Item1);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_SingleItem_1()
	{
		await _query.AddRangeAsync(new List<FileIndexItem>
		{
			new("/single_item1a.jpg"), new("/single_item2a.jpg")
		});

		var result = await _query.GetObjectsByFilePathQueryAsync(
			new List<string> { "/single_item1a.jpg" });

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/single_item1a.jpg", result[0].FilePath);

		await _query.RemoveItemAsync(result[0]);

		await _query.RemoveItemAsync(await _query.GetObjectByFilePathAsync("/single_item2a.jpg")
		                             ?? throw new InvalidOperationException(
			                             "Should have a result"));
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_singleItem_SingleItem()
	{
		await _query.AddRangeAsync(new List<FileIndexItem>
		{
			new("/single_item1a.jpg"), new("/single_item2a.jpg")
		});

		var result = await _query.GetObjectsByFilePathAsync("/single_item1a.jpg", true);

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/single_item1a.jpg", result[0].FilePath);

		await _query.RemoveItemAsync(result[0]);

		await _query.RemoveItemAsync(await _query.GetObjectByFilePathAsync("/single_item2a.jpg")
		                             ?? throw new InvalidOperationException(
			                             "[GetObjectsByFilePathAsync_singleItem_SingleItem]	" +
			                             "Should have a result "));
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_Single_ButDuplicate_Item()
	{
		await _query.AddRangeAsync(new List<FileIndexItem>
		{
			new("/single_duplicate_1.jpg"), new("/single_duplicate_1.jpg")
		});

		var result = await _query.GetObjectsByFilePathQueryAsync(
			new List<string> { "/single_duplicate_1.jpg" });

		Assert.AreEqual(2, result.Count);
		Assert.AreEqual("/single_duplicate_1.jpg", result[0].FilePath);
		Assert.AreEqual("/single_duplicate_1.jpg", result[1].FilePath);

		await _query.RemoveItemAsync(result[0]);
		await _query.RemoveItemAsync(result[1]);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_MultipleItems()
	{
		async Task AddItems()
		{
			Console.WriteLine("Retrying Add Items");
			await _query.AddRangeAsync(new List<FileIndexItem>
			{
				new("/multiple_item_t"), // <= should never match this one
				new("/multiple_item_t_0.jpg"),
				new("/multiple_item_t_1.jpg"),
				new("/multiple_item_t_2.jpg"),
				new("/multiple_item_t_3.jpg")
			});
		}

		async Task<List<FileIndexItem>> GetResult()
		{
			return await _query.GetObjectsByFilePathQueryAsync(
				new List<string>
				{
					"/multiple_item_t_0.jpg",
					"/multiple_item_t_1.jpg",
					"/multiple_item_t_2.jpg",
					"/multiple_item_t_3.jpg"
				});
		}

		await AddItems();
		var result = await GetResult();

		if ( result.Count != 4 )
		{
			Console.WriteLine("Retrying");
			await Task.Delay(100);
			await AddItems();
			result = await GetResult();
		}

		Assert.AreEqual(4, result.Count);

		var orderedResults = result.OrderBy(p => p.FileName).ToList();
		Assert.AreEqual("/multiple_item_t_0.jpg", orderedResults[0].FilePath);
		Assert.AreEqual("/multiple_item_t_1.jpg", orderedResults[1].FilePath);
		Assert.AreEqual("/multiple_item_t_2.jpg", orderedResults[2].FilePath);

		await _query.RemoveItemAsync(result[0]);
		await _query.RemoveItemAsync(result[1]);
		await _query.RemoveItemAsync(result[2]);
		await _query.RemoveItemAsync(result[3]);
		var dir = await _query.GetObjectByFilePathAsync("/multiple_item_t");
		Assert.IsNotNull(dir);
		await _query.RemoveItemAsync(dir);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_TwoItems()
	{
		await _query.AddRangeAsync(new List<FileIndexItem>
		{
			new("/two_item_0.jpg"), new("/two_item_1.jpg")
		});

		var result = await _query.GetObjectsByFilePathQueryAsync(
			new List<string> { "/two_item_0.jpg", "/two_item_1.jpg" });

		Assert.AreEqual(2, result.Count);

		var orderedResults = result.OrderBy(p => p.FileName).ToList();
		Assert.AreEqual("/two_item_0.jpg", orderedResults[0].FilePath);
		Assert.AreEqual("/two_item_1.jpg", orderedResults[1].FilePath);

		await _query.RemoveItemAsync(result[0]);
		await _query.RemoveItemAsync(result[1]);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_SingleItem_Disposed()
	{
		await _query.AddRangeAsync(new List<FileIndexItem>
		{
			new("/disposed/single_item_disposed_1_a.jpg")
		});

		// get context
		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		// Dispose here
		await dbContextDisposed.DisposeAsync();

		var result = await new Query(dbContextDisposed,
				new AppSettings(), serviceScopeFactory, new FakeIWebLogger(),
				new FakeMemoryCache(new Dictionary<string, object>()))
			.GetObjectsByFilePathQueryAsync(new List<string>
			{
				"/disposed/single_item_disposed_1_a.jpg"
			});

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("/disposed/single_item_disposed_1_a.jpg", result[0].FilePath);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathQueryAsyncTest_NullReferenceException()
	{
		var logger = new FakeIWebLogger();
		var fakeQuery = new Query(null!, null!, null!, logger);

		// Assert that a NullReferenceException is thrown when GetObjectsByFilePathQueryAsync is called
		await Assert.ThrowsExceptionAsync<NullReferenceException>(async () =>
			await fakeQuery.GetObjectsByFilePathQueryAsync(["test"]));
	}

	[TestMethod]
	public async Task GetObjectsByFilePathQuery_NoContentInList()
	{
		var logger = new FakeIWebLogger();
		var fakeQuery = new Query(null!,
			null!, null!, logger);

		var result = await fakeQuery.GetObjectsByFilePathQuery(new List<string>().ToArray(), true);
		Assert.AreEqual(0, result.Count);
	}
}

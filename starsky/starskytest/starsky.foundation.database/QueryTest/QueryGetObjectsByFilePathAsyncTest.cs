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
		[
			new FileIndexItem($"{dirName}/test1.jpg"),
			new FileIndexItem($"{dirName}/test1.dng"),
			new FileIndexItem($"{dirName}/test2.jpg"),
			new FileIndexItem($"{dirName}/test2.dng")
		]);

		var result =
			await query.GetObjectsByFilePathAsync([$"{dirName}/test1.jpg"],
				false);
		Assert.HasCount(1, result);

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
		[
			new FileIndexItem($"{dirName}/test1.jpg"),
			new FileIndexItem($"{dirName}/test1.dng"),
			new FileIndexItem($"{dirName}/test2.jpg"),
			new FileIndexItem($"{dirName}/test2.dng")
		]);

		var result =
			await query.GetObjectsByFilePathAsync([$"{dirName}/test1.jpg"],
				true);
		Assert.HasCount(2, result);
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
			await _query.GetObjectsByFilePathAsync([dirName],
				isCollection);

		Assert.HasCount(1, result);
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
		await query.AddRangeAsync([
			new FileIndexItem($"{dirName}/test1.jpg"),
			new FileIndexItem($"{dirName}/test1.dng"),
			new FileIndexItem($"{dirName}/test2.jpg"),
			new FileIndexItem($"{dirName}/test2.dng")
		]);

		await query.GetObjectsByFilePathAsync(
			[$"{dirName}/test1.jpg"], true);

		var cacheResult = query.CacheGetParentFolder(dirName);

		Assert.IsFalse(cacheResult.Item1);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_SingleItem_1()
	{
		await _query.AddRangeAsync([
			new FileIndexItem("/single_item1a.jpg"), new FileIndexItem("/single_item2a.jpg")
		]);

		var result = await _query.GetObjectsByFilePathQueryAsync(
			["/single_item1a.jpg"]);

		Assert.HasCount(1, result);
		Assert.AreEqual("/single_item1a.jpg", result[0].FilePath);

		await _query.RemoveItemAsync(result[0]);

		await _query.RemoveItemAsync(await _query.GetObjectByFilePathAsync("/single_item2a.jpg")
		                             ?? throw new InvalidOperationException(
			                             "Should have a result"));
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_singleItem_SingleItem()
	{
		await _query.AddRangeAsync([
			new FileIndexItem("/single_item1a.jpg"), new FileIndexItem("/single_item2a.jpg")
		]);

		var result = await _query.GetObjectsByFilePathAsync("/single_item1a.jpg", true);

		Assert.HasCount(1, result);
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
		await _query.AddRangeAsync([
			new FileIndexItem("/single_duplicate_1.jpg"),
			new FileIndexItem("/single_duplicate_1.jpg")
		]);

		var result = await _query.GetObjectsByFilePathQueryAsync(
			["/single_duplicate_1.jpg"]);

		Assert.HasCount(2, result);
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
			await _query.AddRangeAsync([
				new FileIndexItem("/multiple_item_t"), // <= should never match this one
				new FileIndexItem("/multiple_item_t_0.jpg"),
				new FileIndexItem("/multiple_item_t_1.jpg"),
				new FileIndexItem("/multiple_item_t_2.jpg"),
				new FileIndexItem("/multiple_item_t_3.jpg")
			]);
		}

		async Task<List<FileIndexItem>> GetResult()
		{
			return await _query.GetObjectsByFilePathQueryAsync(
			[
				"/multiple_item_t_0.jpg",
				"/multiple_item_t_1.jpg",
				"/multiple_item_t_2.jpg",
				"/multiple_item_t_3.jpg"
			]);
		}

		await AddItems();
		var result = await GetResult();

		if ( result.Count != 4 )
		{
			Console.WriteLine("Retrying");
			await Task.Delay(100, TestContext.CancellationTokenSource.Token);
			await AddItems();
			result = await GetResult();
		}

		Assert.HasCount(4, result);

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
		await _query.AddRangeAsync([
			new FileIndexItem("/two_item_0.jpg"), new FileIndexItem("/two_item_1.jpg")
		]);

		var result = await _query.GetObjectsByFilePathQueryAsync(
			["/two_item_0.jpg", "/two_item_1.jpg"]);

		Assert.HasCount(2, result);

		var orderedResults = result.OrderBy(p => p.FileName).ToList();
		Assert.AreEqual("/two_item_0.jpg", orderedResults[0].FilePath);
		Assert.AreEqual("/two_item_1.jpg", orderedResults[1].FilePath);

		await _query.RemoveItemAsync(result[0]);
		await _query.RemoveItemAsync(result[1]);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathAsync_SingleItem_Disposed()
	{
		await _query.AddRangeAsync([new FileIndexItem("/disposed/single_item_disposed_1_a.jpg")]);

		// get context
		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		// Dispose here
		await dbContextDisposed.DisposeAsync();

		var result = await new Query(dbContextDisposed,
				new AppSettings(), serviceScopeFactory, new FakeIWebLogger(),
				new FakeMemoryCache(new Dictionary<string, object>()))
			.GetObjectsByFilePathQueryAsync(["/disposed/single_item_disposed_1_a.jpg"]);

		Assert.HasCount(1, result);
		Assert.AreEqual("/disposed/single_item_disposed_1_a.jpg", result[0].FilePath);
	}

	[TestMethod]
	public async Task GetObjectsByFilePathQueryAsyncTest_NullReferenceException()
	{
		var logger = new FakeIWebLogger();
		var fakeQuery = new Query(null!, null!, null!, logger);

		// Assert that a NullReferenceException is thrown when GetObjectsByFilePathQueryAsync is called
		await Assert.ThrowsExactlyAsync<NullReferenceException>(async () =>
			await fakeQuery.GetObjectsByFilePathQueryAsync(["test"]));
	}

	[TestMethod]
	public async Task GetObjectsByFilePathQuery_NoContentInList()
	{
		var logger = new FakeIWebLogger();
		var fakeQuery = new Query(null!,
			null!, null!, logger);

		var result = await fakeQuery.GetObjectsByFilePathQuery(new List<string>().ToArray(), true);
		Assert.IsEmpty(result);
	}

	public TestContext TestContext { get; set; }
}

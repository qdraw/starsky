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
public class QueryRemoveItemAsyncTest
{
	private readonly Query _query;

	public QueryRemoveItemAsyncTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		var memoryCache = provider.GetRequiredService<IMemoryCache>();
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var logger = new FakeIWebLogger();
		_query = new Query(dbContext,
			new AppSettings { Verbose = true }, serviceScope, logger, memoryCache);
	}

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(QueryTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	[TestMethod]
	public async Task QueryRemoveItemAsyncTest_SingleItem_AddOneItem()
	{
		const string path = "/QueryRemoveItemAsyncTest_SingleItem_AddOneItem";
		var result = await _query.AddItemAsync(new FileIndexItem(path));
		Assert.AreEqual(path, _query.GetObjectByFilePath(path)?.FilePath);

		await _query.RemoveItemAsync(result);
		Assert.IsNull(_query.GetObjectByFilePath(path));
	}

	[TestMethod]
	public async Task QueryRemoveItemAsyncTest_List_AddOneItem()
	{
		var result1 =
			await _query.AddItemAsync(
				new FileIndexItem("/QueryRemoveItemAsyncTest_AddOneItem_List_1"));
		var result2 =
			await _query.AddItemAsync(
				new FileIndexItem("/QueryRemoveItemAsyncTest_AddOneItem_List_2"));

		await _query.RemoveItemAsync(new List<FileIndexItem> { result1, result2 });

		Assert.IsNull(_query.GetObjectByFilePath("/QueryRemoveItemAsyncTest_AddOneItem_List_1"));
		Assert.IsNull(_query.GetObjectByFilePath("/QueryRemoveItemAsyncTest_AddOneItem_List_2"));
	}

	[TestMethod]
	public async Task Query_RemoveAsync_Disposed()
	{
		var addedItems = new List<FileIndexItem>
		{
			new() { FileHash = "RemoveAsync_Disposed__1" },
			new() { FileHash = "RemoveAsync_Disposed__2" }
		};

		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		await dbContextDisposed.FileIndex.AddRangeAsync(addedItems, TestContext.CancellationTokenSource.Token);
		await dbContextDisposed.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

		// Dispose here
		await dbContextDisposed.DisposeAsync();

		var service = new Query(dbContextDisposed,
			new AppSettings { AddMemoryCache = false }, serviceScopeFactory, new FakeIWebLogger(),
			new FakeMemoryCache());
		await service.RemoveItemAsync(addedItems);

		var context = new InjectServiceScope(serviceScopeFactory).Context();
		var queryFromDb = await context.FileIndex.Where(p =>
			p.FileHash == addedItems[0].FilePath || p.FileHash == addedItems[1].FilePath
		).ToListAsync(TestContext.CancellationTokenSource.Token);

		Assert.IsEmpty(queryFromDb);
	}

	public TestContext TestContext { get; set; }
}

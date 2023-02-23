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
	private readonly IMemoryCache _memoryCache;
	private readonly FakeIWebLogger _logger;
	private readonly Query _queryNoVerbose;
	
	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}
	
	public QueryRemoveItemAsyncTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		_memoryCache = provider.GetService<IMemoryCache>();
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		_logger = new FakeIWebLogger();
		_query = new Query(dbContext, 
			new AppSettings{Verbose = true}, serviceScope,_logger ,_memoryCache);
		_queryNoVerbose = new Query(dbContext, 
			new AppSettings{Verbose = false}, serviceScope,_logger ,_memoryCache);
	}
	
	[TestMethod]
	public async Task QueryRemoveItemAsyncTest_SingleItem_AddOneItem()
	{
		const string path = "/QueryRemoveItemAsyncTest_SingleItem_AddOneItem";
		var result = await _query.AddItemAsync(new FileIndexItem(path));
		Assert.AreEqual(path, _query.GetObjectByFilePath(path)?.FilePath);

		await _query.RemoveItemAsync(result);
		Assert.AreEqual(null, _query.GetObjectByFilePath(path));
	}
	
		
	[TestMethod]
	public async Task QueryRemoveItemAsyncTest_List_AddOneItem()
	{
		var result1 = await _query.AddItemAsync(new FileIndexItem("/QueryRemoveItemAsyncTest_AddOneItem_List_1"));
		var result2 = await _query.AddItemAsync(new FileIndexItem("/QueryRemoveItemAsyncTest_AddOneItem_List_2"));

		await _query.RemoveItemAsync(new List<FileIndexItem>{result1, result2});

		Assert.AreEqual(null, _query.GetObjectByFilePath("/QueryRemoveItemAsyncTest_AddOneItem_List_1"));
		Assert.AreEqual(null, _query.GetObjectByFilePath("/QueryRemoveItemAsyncTest_AddOneItem_List_2"));
	}
	
	[TestMethod]
	public async Task RemoveAsync_Disposed()
	{
		var addedItems = new List<FileIndexItem>
		{
			new FileIndexItem {FileHash = "RemoveAsync_Disposed__1"},
			new FileIndexItem {FileHash = "RemoveAsync_Disposed__2"}
		};
			
		var serviceScopeFactory = CreateNewScope();
		var scope = serviceScopeFactory.CreateScope();
		var dbContextDisposed = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		await dbContextDisposed.FileIndex.AddRangeAsync(addedItems);
		await dbContextDisposed.SaveChangesAsync();
		
		// Dispose here
		await dbContextDisposed.DisposeAsync();
			
		await new Query(dbContextDisposed, 
			new AppSettings {
				AddMemoryCache = false 
			}, serviceScopeFactory, new FakeIWebLogger(), new FakeMemoryCache()).RemoveItemAsync(addedItems);
			
		var context = new InjectServiceScope(serviceScopeFactory).Context();
		var queryFromDb = context.FileIndex.Where(p => 
			p.FileHash == addedItems[0].FilePath || p.FileHash == addedItems[1].FilePath
			).ToList();

		Assert.AreEqual(0, queryFromDb.Count);
	}
}

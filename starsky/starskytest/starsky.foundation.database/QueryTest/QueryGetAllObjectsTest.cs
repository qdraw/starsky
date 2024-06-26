using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public sealed class QueryGetAllObjectsTest
	{
		private readonly IMemoryCache _memoryCache;

		public QueryGetAllObjectsTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetRequiredService<IMemoryCache>();
		}
		
		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryGetAllFilesTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		[TestMethod]
		public async Task GetAllObjectsAsync_GetResult()
		{
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			var dbContext = new SetupDatabaseTypes(appSettings).BuilderDbFactory();
			var query = new Query(dbContext, new AppSettings(), null, new FakeIWebLogger(),new FakeMemoryCache());

			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllObjectsAsync") {IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllObjectsAsync/test") {IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllObjectsAsync/test.jpg"));
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllObjectsAsync/test/test.jpg"));
			await dbContext.SaveChangesAsync();
	        
			var items = (await query.GetAllObjectsAsync("/GetAllObjectsAsync"))
				.OrderBy(p => p.FileName).ToList();

			Assert.AreEqual(2, items.Count);
			Assert.AreEqual("/GetAllObjectsAsync/test", items[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, items[0].Status);
			
			Assert.AreEqual("/GetAllObjectsAsync/test.jpg", items[1].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, items[1].Status);
		}
		
		[TestMethod]
		public async Task GetAllObjectsAsync_MultiQuery_GetResult()
		{
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			var dbContext = new SetupDatabaseTypes(appSettings).BuilderDbFactory();
			var query = new Query(dbContext, new AppSettings(), null, new FakeIWebLogger(), new FakeMemoryCache());

			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllObjects_multi_01") {IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllObjects_multi_01/test.jpg"));
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllObjects_multi_02") {IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllObjects_multi_02/test.jpg"));
			await dbContext.SaveChangesAsync();
	        
			var items = (await query.GetAllObjectsAsync(
					new List<string>{
						"/GetAllObjects_multi_01",
						"/GetAllObjects_multi_02"
					}))
				.OrderBy(p => p.FileName).ToList();

			Assert.AreEqual(2, items.Count);
			Assert.AreEqual("/GetAllObjects_multi_01/test.jpg", items[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, items[0].Status);
			
			Assert.AreEqual("/GetAllObjects_multi_02/test.jpg", items[1].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, items[1].Status);
		}

		[TestMethod]
		public async Task GetAllObjectsAsync_NoParameters()
		{
			var query = new Query(null!, new AppSettings(), null, 
				new FakeIWebLogger(), new FakeMemoryCache());

			var result= await query.GetAllObjectsAsync(new List<string>());
			Assert.AreEqual(0,result.Count);
		}

		[TestMethod]
		public async Task GetAllObjectsAsync_DisposedItem()
		{
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var query = new Query(dbContext, new AppSettings(), serviceScope, new FakeIWebLogger(),_memoryCache);
	        
			// item sub folder
			var item = new FileIndexItem("/test_3457834583/test_0191919.jpg");
			await dbContext.FileIndex.AddAsync(item);
			await dbContext.SaveChangesAsync();
	        
			// Important to dispose!
			await dbContext.DisposeAsync();

			item.Tags = "test";
			await query.UpdateItemAsync(item);

			var getItem = await query.GetAllObjectsAsync("/test_3457834583");
			Assert.IsNotNull(getItem);
			Assert.AreEqual("test", getItem.FirstOrDefault()?.Tags);

			var cleanItem = getItem.FirstOrDefault();
			Assert.IsNotNull(cleanItem);
			await query.RemoveItemAsync(cleanItem);
		}
	}
}

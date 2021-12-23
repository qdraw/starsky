using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public class QueryGetAllFolderAsyncTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly IQuery _query;

		public QueryGetAllFolderAsyncTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			_query = new Query(dbContext, 
				new AppSettings{Verbose = true}, serviceScope, new FakeIWebLogger(),_memoryCache);
		}
		
		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryGetAllFilesTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}

		
		[TestMethod]
		public async Task QueryGetFoldersAsync_GetResult()
		{
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			var dbContext = new SetupDatabaseTypes(appSettings, null).BuilderDbFactory();
			var query = new Query(dbContext, new AppSettings(), null, 
				new FakeIWebLogger(), new FakeMemoryCache());

			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetFoldersAsync") {IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetFoldersAsync/test") {IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetFoldersAsync/test.jpg"));
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetFoldersAsync/test/test.jpg"));
			await dbContext.SaveChangesAsync();
	        
			var items = (await query.GetFoldersAsync("/GetFoldersAsync"))
				.OrderBy(p => p.FileName).ToList();

			Assert.AreEqual(1, items.Count);
			Assert.AreEqual("/GetFoldersAsync/test", items[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, items[0].Status);
		}
		
		[TestMethod]
		public async Task QueryGetFoldersAsync_MultiQuery_GetResult()
		{
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			var dbContext = new SetupDatabaseTypes(appSettings, null).BuilderDbFactory();
			var query = new Query(dbContext, new AppSettings(), 
				null, new FakeIWebLogger(), new FakeMemoryCache());

			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetFolders_multi_01") {IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetFolders_multi_01/test"){IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetFolders_multi_02") {IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetFolders_multi_02/test"){IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetFolders_multi_02/test.jpg"));
			await dbContext.SaveChangesAsync();
	        
			var items = (await query.GetFoldersAsync(
					new List<string>{
						"/GetFolders_multi_01",
						"/GetFolders_multi_02"
					}))
				.OrderBy(p => p.FilePath).ToList();

			Assert.AreEqual(2, items.Count);
			Assert.AreEqual("/GetFolders_multi_01/test", items[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, items[0].Status);
			
			Assert.AreEqual("/GetFolders_multi_02/test", items[1].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, items[1].Status);
		}
		
		[TestMethod]
		public async Task GetFoldersAsync_DisposedItem()
		{
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var query = new Query(dbContext, new AppSettings(), serviceScope, new FakeIWebLogger(),_memoryCache);
	        
			// item sub folder
			var item = new FileIndexItem("/test_324323423/test_3423434"){IsDirectory = true};
			await dbContext.FileIndex.AddAsync(item);
			await dbContext.SaveChangesAsync();
	        
			// Important to dispose!
			await dbContext.DisposeAsync();

			item.Tags = "test";
			await query.UpdateItemAsync(item);

			var getItem = await query.GetFoldersAsync("/test_324323423");
			Assert.IsNotNull(getItem);
			Assert.AreEqual("test", getItem.FirstOrDefault().Tags);

			await query.RemoveItemAsync(getItem.FirstOrDefault());
		}
	}
}

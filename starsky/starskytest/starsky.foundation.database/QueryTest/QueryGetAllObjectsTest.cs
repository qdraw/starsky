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
	public class QueryGetAllObjectsTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly IQuery _query;

		public QueryGetAllObjectsTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			_query = new Query(dbContext,_memoryCache, 
				new AppSettings{Verbose = true}, serviceScope, new FakeIWebLogger());
		}
		
		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryGetAllFilesTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		private static FileIndexItem _insertSearchDatahiJpgInput;
		private static FileIndexItem _insertSearchDatahi2JpgInput;
		private static FileIndexItem _insertSearchDatahi2SubfolderJpgInput;

		private void InsertSearchData()
		{
			if ( !string.IsNullOrEmpty(
				_query.GetSubPathByHash("09876543456789")) ) return;
			
			_insertSearchDatahiJpgInput = _query.AddItem(new FileIndexItem
			{
				FileName = "hi.jpg",
				ParentDirectory = "/basic",
				FileHash = "09876543456789",
				ColorClass = ColorClassParser.Color.Winner, // 1
				Tags = "",
				Title = "",
				IsDirectory = false
			});

			_insertSearchDatahi2JpgInput = _query.AddItem(new FileIndexItem
			{
				FileName = "hi2.jpg",
				Tags = "!delete!",
				ParentDirectory = "/basic",
				IsDirectory = false
			});
			
			_insertSearchDatahi2SubfolderJpgInput =  _query.AddItem(new FileIndexItem
			{
				FileName = "hi2.jpg",
				ParentDirectory = "/basic/subfolder",
				FileHash = "234567876543",
				IsDirectory = false
			});
		}
		
		[TestMethod]
		public async Task GetAllObjectsAsync_GetResult()
		{
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			var dbContext = new SetupDatabaseTypes(appSettings, null).BuilderDbFactory();
			var query = new Query(dbContext, new FakeMemoryCache(), new AppSettings(), null, new FakeIWebLogger());

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
			var dbContext = new SetupDatabaseTypes(appSettings, null).BuilderDbFactory();
			var query = new Query(dbContext, new FakeMemoryCache(), new AppSettings(), null, new FakeIWebLogger());

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
			var query = new Query(null, new FakeMemoryCache(), new AppSettings(), null, new FakeIWebLogger());

			var result= await query.GetAllObjectsAsync(new List<string>());
			Assert.AreEqual(0,result.Count);
		}

		[TestMethod]
		public async Task GetAllObjectsAsync_DisposedItem()
		{
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var query = new Query(dbContext,_memoryCache, new AppSettings(), serviceScope, new FakeIWebLogger());
	        
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
			Assert.AreEqual("test", getItem.FirstOrDefault().Tags);

			await query.RemoveItemAsync(getItem.FirstOrDefault());
		}
	}
}

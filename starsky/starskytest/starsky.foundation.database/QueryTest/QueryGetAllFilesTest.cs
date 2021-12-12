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
	public class QueryGetAllFilesTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly IQuery _query;

		public QueryGetAllFilesTest()
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
		public void QueryAddSingleItemRootFolderTest()
		{
			InsertSearchData();
			// Test root folder ("/)
			var getAllFilesExpectedResult = new List<FileIndexItem>
			{
				_insertSearchDatahiJpgInput,
				_insertSearchDatahi2JpgInput,
			};

			var getAllResult = _query.GetAllFiles("/basic");

			CollectionAssert.AreEqual(getAllFilesExpectedResult.Select(p => p.FilePath).ToList(), 
				getAllResult.Select(p => p.FilePath).ToList());
		}
		
		[TestMethod]
		public void GetAllFiles_DisposedItem()
		{
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var query = new Query(dbContext,_memoryCache, new AppSettings(), serviceScope, new FakeIWebLogger());
	        
			// item sub folder
			var item = new FileIndexItem("/test_821827/test_0191919.jpg");
			dbContext.FileIndex.Add(item);
			dbContext.SaveChanges();
	        
			// Important to dispose!
			dbContext.Dispose();

			item.Tags = "test";
			query.UpdateItem(item);

			var getItem = query.GetAllFiles("/test_821827");
			Assert.IsNotNull(getItem);
			Assert.AreEqual("test", getItem.FirstOrDefault().Tags);

			query.RemoveItem(getItem.FirstOrDefault());
		}
		
		
		[TestMethod]
		public void QueryAddSingleItemSubFolderTest()
		{
			InsertSearchData();

			// Test subfolder
			var getAllFilesSubFolderExpectedResult = new List<FileIndexItem> {_insertSearchDatahi2SubfolderJpgInput};

			var getAllResultSubfolder = _query.GetAllFiles("/basic/subfolder");
            
			CollectionAssert.AreEqual(getAllFilesSubFolderExpectedResult.Select(p => p.FilePath).ToList(), 
				getAllResultSubfolder.Select(p => p.FilePath).ToList());
		}
		
		[TestMethod]
		public async Task GetAllFilesAsync_GetResult()
		{
			var appSettings = new AppSettings
			{
				DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
			};
			var dbContext = new SetupDatabaseTypes(appSettings, null).BuilderDbFactory();
			var query = new Query(dbContext, new FakeMemoryCache(), new AppSettings(), null, new FakeIWebLogger());

			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllFilesAsync") {IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllFilesAsync/test") {IsDirectory = true});
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllFilesAsync/test.jpg"));
			await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllFilesAsync/test/test.jpg"));
			await dbContext.SaveChangesAsync();
	        
			var items = await query.GetAllFilesAsync("/GetAllFilesAsync");

			Assert.AreEqual(1, items.Count);

			Assert.AreEqual("/GetAllFilesAsync/test.jpg", items[0].FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, items[0].Status);
		}
	}
}

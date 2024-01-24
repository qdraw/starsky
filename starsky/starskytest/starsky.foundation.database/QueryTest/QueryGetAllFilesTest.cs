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
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest
{
	[TestClass]
	public sealed class QueryGetAllFilesTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly Query _query;

		public QueryGetAllFilesTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetRequiredService<IMemoryCache>();
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			_query = new Query(dbContext, 
				new AppSettings{Verbose = true}, 
				serviceScope, new FakeIWebLogger(),_memoryCache);
		}
		
		private IServiceScopeFactory CreateNewScope()
		{
			var services = new ServiceCollection();
			services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nameof(QueryGetAllFilesTest)));
			var serviceProvider = services.BuildServiceProvider();
			return serviceProvider.GetRequiredService<IServiceScopeFactory>();
		}
		
		private static FileIndexItem _insertSearchDataHiJpgInput = new FileIndexItem();
		private static FileIndexItem _insertSearchDataHi2JpgInput= new FileIndexItem();
		private static FileIndexItem _insertSearchDataHi2SubfolderJpgInput= new FileIndexItem();

		private async Task InsertSearchData()
		{
			if ( !string.IsNullOrEmpty(
				await _query.GetSubPathByHashAsync("09876543456789")) ) return;
			
			_insertSearchDataHiJpgInput = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "hi.jpg",
				ParentDirectory = "/basic",
				FileHash = "09876543456789",
				ColorClass = ColorClassParser.Color.Winner, // 1
				Tags = "",
				Title = "",
				IsDirectory = false
			});

			_insertSearchDataHi2JpgInput = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "hi2.jpg",
				Tags = TrashKeyword.TrashKeywordString,
				ParentDirectory = "/basic",
				IsDirectory = false
			});
			
			_insertSearchDataHi2SubfolderJpgInput =  await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "hi2.jpg",
				ParentDirectory = "/basic/subfolder",
				FileHash = "234567876543",
				IsDirectory = false
			});
		}

		[TestMethod]
		public async Task QueryAddSingleItemRootFolderTest()
		{
			await InsertSearchData();
			// Test root folder ("/)
			var getAllFilesExpectedResult = new List<FileIndexItem>
			{
				_insertSearchDataHiJpgInput,
				_insertSearchDataHi2JpgInput,
			};

			var getAllResult = await _query.GetAllFilesAsync("/basic");

			CollectionAssert.AreEqual(getAllFilesExpectedResult.Select(p => p.FilePath).ToList(), 
				getAllResult.Select(p => p.FilePath).ToList());
		}
		
		[TestMethod]
		public async Task GetAllFilesAsync_DisposedItem()
		{
			var serviceScope = CreateNewScope();
			var scope = serviceScope.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			var query = new Query(dbContext, new AppSettings(), serviceScope, new FakeIWebLogger(),_memoryCache);
	        
			// item sub folder
			var item = new FileIndexItem("/test_821827/test_0191919.jpg");
			dbContext.FileIndex.Add(item);
			await dbContext.SaveChangesAsync();
	        
			// Important to dispose!
			await dbContext.DisposeAsync();

			item.Tags = "test";
			await query.UpdateItemAsync(item);

			var getItem = await query.GetAllFilesAsync("/test_821827");
			Assert.IsNotNull(getItem);
			Assert.AreEqual("test", getItem.FirstOrDefault()?.Tags);

			var cleanItem = getItem.FirstOrDefault();
			Assert.IsNotNull(cleanItem);
			await query.RemoveItemAsync(cleanItem);
		}
		
		
		[TestMethod]
		public async Task QueryAddSingleItemSubFolderTest()
		{
			await InsertSearchData();

			// Test subfolder
			var getAllFilesSubFolderExpectedResult = new List<FileIndexItem> {_insertSearchDataHi2SubfolderJpgInput};

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
			var dbContext = new SetupDatabaseTypes(appSettings).BuilderDbFactory();
			var query = new Query(dbContext, new AppSettings(), null, new FakeIWebLogger(), new FakeMemoryCache());

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

		[TestMethod]
		public void FormatOkTest_Null()
		{
			var result = Query.FormatOk(null!);
			Assert.AreEqual(0, result.Count);
		}
		
		[TestMethod]
		public void FormatOkTest_NullValue()
		{
			var result = Query.FormatOk(new List<FileIndexItem>{null!});
			Assert.AreEqual(0, result.Count);
		}
		
				
		[TestMethod]
		public void FormatOkTest_ShouldPassContent()
		{
			var result = Query.FormatOk(new List<FileIndexItem>{new FileIndexItem{ Status = FileIndexItem.ExifStatus.Unauthorized}});
			Assert.AreEqual(1, result.Count);
		}
		
		[TestMethod]
		public void FormatOkTest_ShouldFormatDefaultToOk()
		{
			var result = Query.FormatOk(new List<FileIndexItem>{new FileIndexItem{ Status = FileIndexItem.ExifStatus.Default}});
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
		}
	}
}

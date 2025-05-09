using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using starsky.project.web.Attributes;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.database.QueryTest;

[TestClass]
public sealed class QueryTest
{
	private static FileIndexItem _insertSearchDatahiJpgInput = new();
	private static FileIndexItem _insertSearchDatahi2JpgInput = new();
	private static FileIndexItem _insertSearchDatahi3JpgInput = new();
	private static FileIndexItem _insertSearchDatahi4JpgInput = new();
	private static FileIndexItem _insertSearchDatahi2SubfolderJpgInput = new();
	private readonly FakeIWebLogger _logger;
	private readonly IMemoryCache _memoryCache;

	private readonly Query _query;
	private readonly Query _queryNoVerbose;

	public QueryTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		_memoryCache = provider.GetRequiredService<IMemoryCache>();
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		_logger = new FakeIWebLogger();
		_query = new Query(dbContext,
			new AppSettings { Verbose = true }, serviceScope, _logger, _memoryCache);
		_queryNoVerbose = new Query(dbContext,
			new AppSettings { Verbose = false }, serviceScope, _logger, _memoryCache);
	}

	private static IServiceScopeFactory CreateNewScope()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseInMemoryDatabase(nameof(QueryTest)));
		var serviceProvider = services.BuildServiceProvider();
		return serviceProvider.GetRequiredService<IServiceScopeFactory>();
	}

	private async Task InsertSearchData()
	{
		if ( string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("09876543456789")) )
		{
			_insertSearchDatahiJpgInput = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "hi.jpg",
				ParentDirectory = "/basic",
				FileHash = "09876543456789",
				ColorClass = ColorClassParser.Color.Winner, // 1
				Tags = "",
				Title = "",
				IsDirectory = false
			});

			_insertSearchDatahi2JpgInput = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "hi2.jpg",
				Tags = TrashKeyword.TrashKeywordString,
				ParentDirectory = "/basic",
				IsDirectory = false
			});

			_insertSearchDatahi3JpgInput = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "hi3.jpg",
				ParentDirectory = "/basic",
				ColorClass = ColorClassParser.Color.Trash, // 9
				IsDirectory = false
			});

			_insertSearchDatahi4JpgInput = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "hi4.jpg",
				ParentDirectory = "/basic",
				ColorClass = ColorClassParser.Color.Winner, // 1
				IsDirectory = false
			});

			_insertSearchDatahi2SubfolderJpgInput = await _query.AddItemAsync(new FileIndexItem
			{
				FileName = "hi2.jpg",
				ParentDirectory = "/basic/subfolder",
				FileHash = "234567876543",
				IsDirectory = false
			});
		}
	}

	[TestMethod]
	public async Task QueryForHomeDoesNotExist_Null()
	{
		// remove if item exist
		var homeItem = _query.SingleItem("/");
		if ( homeItem?.FileIndexItem != null )
		{
			await _query.RemoveItemAsync(homeItem.FileIndexItem);

			// Query again if needed
			homeItem = _query.SingleItem("/");
		}

		// retry 2
		if ( homeItem?.FileIndexItem != null )
		{
			await _query.RemoveItemAsync(homeItem.FileIndexItem);

			// Query again if needed
			homeItem = _query.SingleItem("/");
		}

		Assert.IsNull(homeItem);
	}


	[TestMethod]
	public async Task QueryForHome()
	{
		var item = await _query.AddItemAsync(new FileIndexItem("/"));
		var home = _query.SingleItem("/")?.FileIndexItem;
		Assert.AreEqual("/", home?.FilePath);
		await _query.RemoveItemAsync(item);
	}

	[TestMethod]
	public async Task QueryAddSingleItemHiJpgOutputTest()
	{
		await InsertSearchData();
		var hiJpgOutput = _query.SingleItem(_insertSearchDatahiJpgInput.FilePath!)
			?.FileIndexItem;

		Console.WriteLine(_insertSearchDatahiJpgInput.FileHash);
		Console.WriteLine(hiJpgOutput?.FileHash);

		Assert.AreEqual(_insertSearchDatahiJpgInput.FileHash, hiJpgOutput?.FileHash);

		// other api Get Object By FilePath
		hiJpgOutput = await _query.GetObjectByFilePathAsync(_insertSearchDatahiJpgInput.FilePath!);
		Assert.AreEqual(_insertSearchDatahiJpgInput.FilePath, hiJpgOutput?.FilePath);
	}

	/// <summary>
	///     Item exist but not in folder cache, it now adds this item to cache #228
	/// </summary>
	[TestMethod]
	public async Task SingleItem_ItemExistInDbButNotInFolderCache()
	{
		await _query.AddItemAsync(new FileIndexItem("/cache_test") { IsDirectory = true });
		var existingItem = new FileIndexItem("/cache_test/test.jpg");
		await _query.AddItemAsync(existingItem);
		_query.AddCacheParentItem("/cache_test", [existingItem]);
		const string newItem = "/cache_test/test2.jpg";
		await _query.AddItemAsync(new FileIndexItem(newItem));

		var queryResult = _query.SingleItem(newItem);
		Assert.IsNotNull(queryResult);
		Assert.AreEqual(newItem, queryResult.FileIndexItem?.FilePath);

		await _query.RemoveItemAsync(queryResult.FileIndexItem!);
	}


	[TestMethod]
	public async Task GetAllFilesAsync_Disposed()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext,
			new AppSettings { Verbose = true }, serviceScope,
			new FakeIWebLogger(), _memoryCache);

		await dbContext.FileIndex.AddAsync(new FileIndexItem("/") { IsDirectory = true });
		await dbContext.FileIndex.AddAsync(new FileIndexItem("/test.jpg"));
		await dbContext.SaveChangesAsync();

		// And dispose
		await dbContext.DisposeAsync();

		var items = await query.GetAllFilesAsync(["/"], 0);

		Assert.AreEqual("/test.jpg", items[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, items[0].Status);
	}

	[TestMethod]
	public async Task GetAllRecursiveAsync_GetResult()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase
		};
		var dbContext = new SetupDatabaseTypes(appSettings).BuilderDbFactory();
		var query = new Query(dbContext, null!, null!, new FakeIWebLogger());

		await dbContext.FileIndex.AddAsync(
			new FileIndexItem("/GetAllRecursiveAsync") { IsDirectory = true });
		await dbContext.FileIndex.AddAsync(
			new FileIndexItem("/GetAllRecursiveAsync/test") { IsDirectory = true });
		await dbContext.FileIndex.AddAsync(new FileIndexItem("/GetAllRecursiveAsync/test.jpg"));
		await dbContext.FileIndex.AddAsync(
			new FileIndexItem("/GetAllRecursiveAsync/test/test.jpg"));
		await dbContext.SaveChangesAsync();

		var items = await query.GetAllRecursiveAsync("/GetAllRecursiveAsync");

		Assert.AreEqual(3, items.Count);
		Assert.AreEqual("/GetAllRecursiveAsync/test", items[0].FilePath);
		Assert.AreEqual("/GetAllRecursiveAsync/test.jpg", items[1].FilePath);
		Assert.AreEqual("/GetAllRecursiveAsync/test/test.jpg", items[2].FilePath);

		Assert.AreEqual(FileIndexItem.ExifStatus.Default, items[0].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Default, items[1].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Default, items[2].Status);
	}

	[TestMethod]
	public async Task GetAllRecursiveAsync_Disposed()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext,
			new AppSettings { Verbose = true }, serviceScope,
			new FakeIWebLogger(), _memoryCache);

		await dbContext.FileIndex.AddAsync(new FileIndexItem("/gar") { IsDirectory = true });
		await dbContext.FileIndex.AddAsync(new FileIndexItem("/gar/test.jpg"));
		await dbContext.SaveChangesAsync();

		// And dispose
		await dbContext.DisposeAsync();

		var items = await query.GetAllRecursiveAsync("/gar");

		Assert.AreEqual(1, items.Count);
		Assert.AreEqual("/gar/test.jpg", items[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Default, items[0].Status);
	}

	[TestMethod]
	public async Task QueryAddSingleItemGetAllRecursiveTest()
	{
		await InsertSearchData();

		// GetAllRecursive
		var getAllRecursiveExpectedResult123 = new List<FileIndexItem>
		{
			_insertSearchDatahiJpgInput,
			_insertSearchDatahi2JpgInput,
			_insertSearchDatahi2SubfolderJpgInput,
			_insertSearchDatahi3JpgInput,
			_insertSearchDatahi4JpgInput
		}.OrderBy(p => p.FileName).ToList();

		var getAllRecursive123 = ( await _query.GetAllRecursiveAsync() )
			.Where(p => p.FilePath?.Contains("/basic") == true)
			.OrderBy(p => p.FileName).ToList();

		Assert.AreEqual(getAllRecursive123.Count, getAllRecursiveExpectedResult123.Count);

		CollectionAssert.AreEqual(getAllRecursive123.Select(p => p.FileHash).ToList(),
			getAllRecursiveExpectedResult123.Select(p => p.FileHash).ToList());

		await _query.RemoveItemAsync(getAllRecursive123);
	}

	[TestMethod]
	public async Task QueryAddSingleItemGetAllRecursiveTest_DisposedItem()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext, new AppSettings(), serviceScope, new FakeIWebLogger(),
			_memoryCache);

		// item sub folder
		var item = new FileIndexItem("/test_1231331/sub/test_0191919.jpg");
		dbContext.FileIndex.Add(item);
		await dbContext.SaveChangesAsync();

		// normal item
		var item2 = new FileIndexItem("/test_1231331/test_0191919.jpg");
		dbContext.FileIndex.Add(item2);
		await dbContext.SaveChangesAsync();

		// Important to dispose!
		await dbContext.DisposeAsync();

		item.Tags = "test";
		await query.UpdateItemAsync(item);

		var getItem = await query.GetAllRecursiveAsync("/test_1231331");
		Assert.IsNotNull(getItem);
		Assert.AreEqual("test", getItem.FirstOrDefault()?.Tags);

		Assert.IsNotNull(getItem.FirstOrDefault());
		await query.RemoveItemAsync(getItem.FirstOrDefault()!);
	}

	[TestMethod]
	public async Task QueryAddSingleItemGetItemByHashTest()
	{
		await InsertSearchData();
		// GetSubPathByHash
		// See above for objects
		Assert.AreEqual("/basic/hi.jpg", await _query.GetSubPathByHashAsync("09876543456789"));
	}

	[TestMethod]
	public async Task QueryAddSingleItemNextWinnerTest()
	{
		await InsertSearchData();
		// Next Winner
		var colorClassActiveList = FileIndexItem.GetColorClassList("1");
		var next = _query.SingleItem("/basic/hi.jpg", colorClassActiveList);
		Assert.AreEqual("/basic/hi4.jpg", next?.RelativeObjects.NextFilePath);
	}

	[TestMethod]
	public async Task QueryAddSingleItemPrevWinnerTest()
	{
		await InsertSearchData();
		// Prev Winner
		var colorClassActiveList = FileIndexItem.GetColorClassList("1");
		var prev = _query.SingleItem("/basic/hi4.jpg", colorClassActiveList)?.RelativeObjects
			.PrevFilePath;
		Assert.AreEqual("/basic/hi.jpg", prev);
	}

	[TestMethod]
	public async Task QueryAddSingleItemDeletedStatus()
	{
		await InsertSearchData();
		var status = _query.SingleItem("/basic/hi2.jpg")?.FileIndexItem?.Status;
		Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, status);
	}

	[TestMethod]
	[ExcludeFromCoverage]
	public async Task QueryFolder_DisplayFileFoldersTest()
	{
		var hiJpgInput = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "hi.jpg",
			ParentDirectory = "/display", // without slash
			FileHash = "123458465522",
			ColorClass = ColorClassParser.Color.Winner // 1
		});

		var hi3JpgInput = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "hi3.jpg",
			ParentDirectory = "/display", // without slash
			FileHash = "78539048765",
			ColorClass = ColorClassParser.Color.Extras
		});

		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "hi2.jpg",
			ParentDirectory = "/display",
			FileHash = "98765432123456",
			Tags = TrashKeyword.TrashKeywordString
		});

		// All Color Classes
		var getDisplayExpectedResult = new List<FileIndexItem> { hiJpgInput, hi3JpgInput };
		var getDisplay = _query.DisplayFileFolders("/display").ToList();


		foreach ( var expectedResult in getDisplayExpectedResult )
		{
			Assert.AreEqual(expectedResult.FilePath,
				getDisplay.Find(p =>
					p.FilePath == expectedResult.FilePath)?.FilePath);
			Assert.AreEqual(expectedResult.ParentDirectory,
				getDisplay.Find(p =>
					p.ParentDirectory == expectedResult.ParentDirectory)?.ParentDirectory);
			Assert.AreEqual(expectedResult.Tags,
				getDisplay.Find(p =>
					p.Tags == expectedResult.Tags)?.Tags);
			Assert.AreEqual(expectedResult.ColorClass,
				getDisplay.Find(p =>
					p.ColorClass == expectedResult.ColorClass)?.ColorClass);
			Assert.AreEqual(expectedResult.FileHash,
				getDisplay.Find(p =>
					p.FileHash == expectedResult.FileHash)?.FileHash);
		}

		// Compare filter
		var getDisplayExpectedResultSuperior = new List<FileIndexItem> { hiJpgInput };
		var colorClassActiveList = FileIndexItem.GetColorClassList("1");

		var getDisplaySuperior =
			_query.DisplayFileFolders("/display", colorClassActiveList).ToList();

		foreach ( var expectedResult in getDisplayExpectedResultSuperior )
		{
			Assert.AreEqual(expectedResult.FilePath,
				getDisplaySuperior.Find(p =>
					p.FilePath == expectedResult.FilePath)?.FilePath);
			Assert.AreEqual(expectedResult.ParentDirectory,
				getDisplaySuperior.Find(p =>
					p.ParentDirectory == expectedResult.ParentDirectory)?.ParentDirectory);
			Assert.AreEqual(expectedResult.Tags,
				getDisplaySuperior.Find(p =>
					p.Tags == expectedResult.Tags)?.Tags);
			Assert.AreEqual(expectedResult.ColorClass,
				getDisplaySuperior.Find(p =>
					p.ColorClass == expectedResult.ColorClass)?.ColorClass);
			Assert.AreEqual(expectedResult.FileHash,
				getDisplaySuperior.Find(p =>
					p.FileHash == expectedResult.FileHash)?.FileHash);
		}

		// This feature is normal used for folders, for now it is done on files
		// Hi3.jpg Previous -- all mode
		var releative = _query.GetNextPrevInFolder("/display/hi3.jpg");

		// Folders ignore deleted items
		Assert.AreEqual("/display/hi2.jpg", releative.PrevFilePath);

		// Next  Relative -- all mode
		var releative2 = _query.GetNextPrevInFolder("/display/hi.jpg");

		Assert.AreEqual("/display/hi2.jpg", releative2.NextFilePath);
		Assert.IsNotNull(releative2);
		if ( releative2.PrevFilePath != null )
		{
			Assert.Fail(releative2.PrevFilePath);
		}
	}

	[TestMethod]
	public async Task QueryFolder_NextPrevDuplicates()
	{
		var folder01 =
			await _query.AddItemAsync(
				new FileIndexItem("/test_duplicate_01") { IsDirectory = true });
		var folder02 =
			await _query.AddItemAsync(
				new FileIndexItem("/test_duplicate_02") { IsDirectory = true });
		var folder02duplicate =
			await _query.AddItemAsync(
				new FileIndexItem("/test_duplicate_02") { IsDirectory = true });
		var folder03 =
			await _query.AddItemAsync(
				new FileIndexItem("/test_duplicate_03") { IsDirectory = true });

		var result = _query.GetNextPrevInFolder("/test_duplicate_02");
		Assert.AreEqual("/test_duplicate_01", result.PrevFilePath);
		Assert.AreEqual("/test_duplicate_03", result.NextFilePath);

		await _query.RemoveItemAsync(folder01);
		await _query.RemoveItemAsync(folder02);
		await _query.RemoveItemAsync(folder02duplicate);
		await _query.RemoveItemAsync(folder03);
	}

	[TestMethod]
	public async Task QueryDisplayFileFolders_Duplicates_Test()
	{
		var image0 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "0.jpg",
			ParentDirectory = "/duplicates_test",
			FileHash = "45782347832",
			ColorClass = ColorClassParser.Color.Winner // 1
		});

		var image1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "1.jpg",
			ParentDirectory = "/duplicates_test",
			FileHash = "123458465522",
			ColorClass = ColorClassParser.Color.Winner // 1
		});

		var image1duplicate = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "1.jpg",
			ParentDirectory = "/duplicates_test",
			FileHash = "123458465522",
			ColorClass = ColorClassParser.Color.Winner // 1
		});

		var image2 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "2.jpg",
			ParentDirectory = "/duplicates_test",
			FileHash = "98765432123456"
		});

		var result = _query.QueryDisplayFileFolders("/duplicates_test");

		Assert.AreEqual(3, result.Count);

		Assert.AreEqual(image0.FilePath, result[0].FilePath);
		Assert.AreEqual(image1.FilePath, result[1].FilePath);
		Assert.AreEqual(image2.FilePath, result[2].FilePath);

		await _query.RemoveItemAsync([image0, image1, image1duplicate, image2]);
	}

	[TestMethod]
	public async Task QueryDisplayFileFolders_XmpShowInQuery_Test()
	{
		var image1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "1.xmp",
			ParentDirectory = "/test_xmp",
			FileHash = "123458465522",
			ImageFormat = ExtensionRolesHelper.ImageFormat.xmp
		});

		var image1Jpg = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "1.jpg",
			ParentDirectory = "/test_xmp",
			FileHash = "123458465522",
			ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
		});

		var result = _query.QueryDisplayFileFolders("/test_xmp");

		Assert.AreEqual(2, result.Count);

		Assert.AreEqual(image1Jpg.FilePath, result[0].FilePath);

		await _query.RemoveItemAsync(image1);
		await _query.RemoveItemAsync(image1Jpg);
	}

	[TestMethod]
	public async Task DisplayFileFolders_hide_xmp()
	{
		var image1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "1.xmp",
			ParentDirectory = "/test_xmp2",
			FileHash = "123458465522",
			ImageFormat = ExtensionRolesHelper.ImageFormat.xmp
		});

		var image1Jpg = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "1.jpg",
			ParentDirectory = "/test_xmp2",
			FileHash = "123458465522",
			ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
		});

		var result = _query.DisplayFileFolders(new List<FileIndexItem> { image1, image1Jpg })
			.ToList();

		Assert.AreEqual(1, result.Count);

		Assert.AreEqual(image1Jpg.FilePath, result[0].FilePath);
	}

	[TestMethod]
	public void QueryFolder_DisplayFileFoldersNoResultTest()
	{
		var getDisplay = _query.DisplayFileFolders("/12345678987654").ToList();
		Assert.AreEqual(0, getDisplay.Count);
	}

	[TestMethod]
	public async Task QueryFolder_DisplayFileFolders_OneItemInFolder_DisposedItem()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext,
			new AppSettings(), serviceScope, new FakeIWebLogger(), _memoryCache);

		var item = new FileIndexItem("/test_0191919/test_0191919.jpg");
		dbContext.FileIndex.Add(item);
		await dbContext.SaveChangesAsync();

		// Important to dispose!
		await dbContext.DisposeAsync();

		item.Tags = "test";
		await query.UpdateItemAsync(item);

		var getItem = query.DisplayFileFolders("/test_0191919").ToList();
		Assert.IsNotNull(getItem);
		Assert.AreEqual("test", getItem.FirstOrDefault()?.Tags);

		Assert.IsNotNull(getItem.FirstOrDefault());
		await query.RemoveItemAsync(getItem.FirstOrDefault()!);
	}

	[ExcludeFromCoverage]
	[TestMethod]
	public async Task BreadcrumbDetailViewTest()
	{
		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "hi3.jpg",
			//FilePath = "/bread/hi3.jpg",
			ParentDirectory = "/bread", // without slash
			FileHash = "234565432",
			ColorClass = ColorClassParser.Color.Extras,
			IsDirectory = false
		});

		var exptectedOutput = new List<string> { "/", "/bread" };
		var output = _query.SingleItem("/bread/hi3.jpg")?.Breadcrumb;
		CollectionAssert.AreEqual(exptectedOutput, output);
	}

	[TestMethod]
	public async Task BreadcrumbDetailViewPagViewTypeTest()
	{
		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "hi4.jpg",
			//FilePath = "/bread/hi4.jpg",
			ParentDirectory = "/bread", // without slash
			FileHash = "23456543",
			ColorClass = ColorClassParser.Color.Extras,
			IsDirectory = false
		});

		// Used for react to get the context
		var pageTypeReact = _query.SingleItem("/bread/hi4.jpg")?.PageType;
		Assert.AreEqual("DetailView", pageTypeReact);
	}


	[TestMethod]
	public async Task QueryTest_NextFilePathCachingConflicts_Deleted()
	{
		// init items
		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "CachingDeleted_001.jpg",
			ParentDirectory = "/QueryTest_NextPrevCachingDeleted",
			FileHash = "0987345678654345678",
			Tags = string.Empty,
			IsDirectory = false
		});

		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "CachingDeleted_002.jpg",
			ParentDirectory = "/QueryTest_NextPrevCachingDeleted",
			FileHash = "3456783456780987654",
			Tags = string.Empty,
			IsDirectory = false
		});

		var single001 =
			_query.SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_001.jpg");
		Assert.AreEqual("/QueryTest_NextPrevCachingDeleted/CachingDeleted_002.jpg",
			single001?.RelativeObjects.NextFilePath);

		var single002 =
			_query
				.SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_002.jpg")
				?.FileIndexItem;
		single002!.Tags = TrashKeyword.TrashKeywordString;
		await _query.UpdateItemAsync(single002);

		// Request new; and check if content is updated in memory cache
		single001 =
			_query.SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_001.jpg");
		Assert.IsNull(single001?.RelativeObjects.NextFilePath);

		// For avoiding conflicts when running multiple unit tests
		single001!.FileIndexItem!.Tags = TrashKeyword.TrashKeywordString;
		await _query.UpdateItemAsync(single001.FileIndexItem);
	}

	[TestMethod]
	public async Task Query_UpdateItem_1_DisposedItem()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext,
			new AppSettings { Verbose = true }, serviceScope, new FakeIWebLogger(),
			_memoryCache);

		var item = new FileIndexItem("/test/010101.jpg");
		dbContext.FileIndex.Add(item);
		await dbContext.SaveChangesAsync();

		// Important to dispose!
		await dbContext.DisposeAsync();

		item.Tags = "test";
		await query.UpdateItemAsync(item);

		var getItem = await _query.GetObjectByFilePathAsync("/test/010101.jpg");
		Assert.IsNotNull(getItem);
		Assert.AreEqual("test", getItem.Tags);

		await query.RemoveItemAsync(getItem);
	}

	[TestMethod]
	[SuppressMessage("Usage", "S6966: GetObjectByFilePath")]
	public async Task Query_GetObjectByFilePath_home()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext,
			new AppSettings { Verbose = true }, serviceScope, new FakeIWebLogger(),
			_memoryCache);
		await query.AddItemAsync(new FileIndexItem("/"));

		var item = query.GetObjectByFilePath("/");
		Assert.IsNotNull(item);
		Assert.AreEqual("/", item.FilePath);
		Assert.AreEqual("/", item.FileName);
	}

	[TestMethod]
	public async Task Query_GetObjectByFilePathAsync_home()
	{
		var dbItem = await _query.AddItemAsync(new FileIndexItem("/"));

		var item = await _query.GetObjectByFilePathAsync("/");
		Assert.IsNotNull(item);
		Assert.AreEqual("/", item.FilePath);
		Assert.AreEqual("/", item.FileName);

		await _query.RemoveItemAsync(dbItem);
	}

	[TestMethod]
	public async Task Query_GetObjectByFilePathAsync_Cache_Ok()
	{
		_memoryCache.Set(Query.GetObjectByFilePathAsyncCacheName("/test135"),
			new FileIndexItem("/test135"));

		var item = await _query.GetObjectByFilePathAsync("/test135", TimeSpan.MaxValue);
		Assert.IsNotNull(item);
		Assert.AreEqual("/test135", item.FilePath);
		Assert.AreEqual("test135", item.FileName);

		_memoryCache.Remove(
			Query.GetObjectByFilePathAsyncCacheName("/test135"));
	}

	[TestMethod]
	public async Task Query_GetObjectByFilePathAsync_Cache_NoDateSet_SoIgnored()
	{
		_memoryCache.Set(Query.GetObjectByFilePathAsyncCacheName("/test135"),
			new FileIndexItem("/test135"));

		var item = await _query.GetObjectByFilePathAsync("/test135"); // <- -  no date added
		Assert.IsNull(item); // <- no date is added so cache is ignored

		_memoryCache.Remove(
			Query.GetObjectByFilePathAsyncCacheName("/test135"));
	}

	[TestMethod]
	public async Task Query_GetObjectByFilePathAsync_Disposed()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext,
			new AppSettings { Verbose = true }, serviceScope, new FakeIWebLogger(),
			_memoryCache);
		var item =
			await query.AddItemAsync(
				new FileIndexItem("/GetObjectByFilePathAsync/test.jpg") { Tags = "hi" });

		// important to Dispose
		await dbContext.DisposeAsync();

		item.Tags = "test";
		await query.UpdateItemAsync(item);

		var getItem =
			await query.GetObjectByFilePathAsync("/GetObjectByFilePathAsync/test.jpg");
		Assert.IsNotNull(getItem);
		Assert.AreEqual("/GetObjectByFilePathAsync/test.jpg", getItem.FilePath);
		Assert.AreEqual("test.jpg", getItem.FileName);
		Assert.AreEqual("test", getItem.Tags);
	}

	[TestMethod]
	public async Task Query_UpdateItem_Multiple_DisposedItem()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext,
			new AppSettings(), serviceScope, new FakeIWebLogger(), _memoryCache);

		var item = new FileIndexItem("/test/010101.jpg");
		dbContext.FileIndex.Add(item);
		await dbContext.SaveChangesAsync();

		// Important to dispose!
		await dbContext.DisposeAsync();

		item.Tags = "test";
		await query.UpdateItemAsync(new List<FileIndexItem> { item });

		var getItem = await query.GetObjectByFilePathAsync("/test/010101.jpg");
		Assert.IsNotNull(getItem);
		Assert.AreEqual("test", getItem.Tags);

		await query.RemoveItemAsync(getItem);
	}

	[TestMethod]
	public async Task UpdateItemAsync_Single()
	{
		var item2 = new FileIndexItem("/test2.jpg");
		await _query.AddItemAsync(item2);

		item2.Tags = "test";
		await _query.UpdateItemAsync(item2);

		var getItem = await _query.GetObjectByFilePathAsync("/test2.jpg");
		Assert.IsNotNull(getItem);
		Assert.AreEqual("test", getItem.Tags);

		await _query.RemoveItemAsync(getItem);
	}

	[TestMethod]
	public async Task AddItemAsync_SqliteRetry()
	{
		var services = new ServiceCollection();
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseSqlite("Data Source=app__data.db"));
		var serviceProvider = services.BuildServiceProvider();

		var logger = new FakeIWebLogger();
		var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext, new AppSettings(), serviceScope, logger, _memoryCache);

		var item = new FileIndexItem("/test/010101.jpg");

		await Assert.ThrowsExactlyAsync<AggregateException>(async () =>
			await query.AddItemAsync(item));
		// should fail due update
	}

	[TestMethod]
	public async Task UpdateItemAsync_Single_DisposedItem()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext, new AppSettings(), serviceScope, new FakeIWebLogger(),
			_memoryCache);

		var item = new FileIndexItem("/test/010101.jpg");
		await dbContext.FileIndex.AddAsync(item);
		await dbContext.SaveChangesAsync();

		// Important to dispose!
		await dbContext.DisposeAsync();

		item.Tags = "test";
		await query.UpdateItemAsync(item);

		var getItem = await query.GetObjectByFilePathAsync("/test/010101.jpg");
		Assert.IsNotNull(getItem);
		Assert.AreEqual("test", getItem.Tags);

		await query.RemoveItemAsync(getItem);
	}

	[TestMethod]
	public async Task UpdateItemAsync_Multiple()
	{
		var item1 = new FileIndexItem("/test24f1s54.jpg");
		var item2 = new FileIndexItem("/test885828.jpg");

		await _query.AddItemAsync(item1);
		await _query.AddItemAsync(item2);

		item1.Tags = "test";
		item2.Tags = "test";

		await _query.UpdateItemAsync(new List<FileIndexItem> { item1, item2 });

		var getItem = await _query.GetObjectByFilePathAsync("/test24f1s54.jpg");
		Assert.IsNotNull(getItem);
		Assert.AreEqual("test", getItem.Tags);

		var getItem2 = await _query.GetObjectByFilePathAsync("/test885828.jpg");
		Assert.IsNotNull(getItem2);
		Assert.AreEqual("test", getItem2.Tags);

		await _query.RemoveItemAsync(getItem);
		await _query.RemoveItemAsync(getItem2);
	}

	[TestMethod]
	public async Task UpdateItemAsync_Multiple_DisposedItem()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext, new AppSettings(), serviceScope, new FakeIWebLogger(),
			_memoryCache);

		var item = new FileIndexItem("/test/8284574.jpg");
		await dbContext.FileIndex.AddAsync(item);
		await dbContext.SaveChangesAsync();

		var item2 = new FileIndexItem("/test/8284575.jpg");
		await dbContext.FileIndex.AddAsync(item2);
		await dbContext.SaveChangesAsync();

		// Important to dispose!
		await dbContext.DisposeAsync();

		item.Tags = "test";
		item2.Tags = "test";

		await query.UpdateItemAsync(new List<FileIndexItem> { item, item2 });

		var getItem = await query.GetObjectByFilePathAsync("/test/8284574.jpg");
		Assert.IsNotNull(getItem);
		Assert.AreEqual("test", getItem.Tags);

		var getItem2 = await query.GetObjectByFilePathAsync("/test/8284575.jpg");
		Assert.IsNotNull(getItem2);
		Assert.AreEqual("test", getItem2.Tags);

		await query.RemoveItemAsync(getItem);
		await query.RemoveItemAsync(getItem2);
	}

	[TestMethod]
	public async Task QueryTest_PrevFilePathCachingConflicts_Deleted()
	{
		// For previous item check if caching has no conflicts

		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "CachingDeleted_003.jpg",
			ParentDirectory = "/QueryTest_NextPrevCachingDeleted",
			FileHash = "56787654",
			Tags = string.Empty,
			IsDirectory = false
		});

		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "CachingDeleted_004.jpg",
			ParentDirectory = "/QueryTest_NextPrevCachingDeleted",
			FileHash = "98765467876",
			Tags = string.Empty,
			IsDirectory = false
		});

		// For previous item
		var single004 =
			_query.SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_004.jpg");
		Assert.IsNotNull(single004);
		Assert.IsNotNull(single004.FileIndexItem);

		Assert.AreEqual("/QueryTest_NextPrevCachingDeleted/CachingDeleted_003.jpg",
			single004.RelativeObjects.PrevFilePath);

		var single003 =
			_query
				.SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_003.jpg")
				?.FileIndexItem;
		single003!.Tags = TrashKeyword.TrashKeywordString;
		await _query.UpdateItemAsync(single003);

		// Request new; item must be updated in cache
		single004 =
			_query.SingleItem("/QueryTest_NextPrevCachingDeleted/CachingDeleted_004.jpg");
		Assert.IsNull(single004?.RelativeObjects.PrevFilePath);

		// For avoiding conflicts when running multiple unit tests
		single004!.FileIndexItem!.Tags = TrashKeyword.TrashKeywordString;
		await _query.UpdateItemAsync(single004.FileIndexItem);
	}


	[TestMethod]
	public async Task QueryTest_TestPreviousFileHash()
	{
		// For previous item check if caching has no conflicts

		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "001.jpg",
			ParentDirectory = "/QueryTest_prev_hash",
			FileHash = "09457777777",
			Tags = string.Empty,
			IsDirectory = false
		});

		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "002.jpg",
			ParentDirectory = "/QueryTest_prev_hash",
			FileHash = "09457847385873",
			Tags = string.Empty,
			IsDirectory = false
		});

		// For previous item
		var single004 =
			_query.SingleItem("/QueryTest_prev_hash/002.jpg");

		// check hash
		Assert.AreEqual("09457777777",
			single004?.RelativeObjects.PrevHash);

		// check path
		Assert.AreEqual("/QueryTest_prev_hash/001.jpg",
			single004?.RelativeObjects.PrevFilePath);
	}


	[TestMethod]
	public async Task QueryTest_TestNextFileHash()
	{
		// For NEXT item check if caching has no conflicts

		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "001.jpg",
			ParentDirectory = "/QueryTest_next_hash",
			FileHash = "09457777777",
			Tags = string.Empty,
			IsDirectory = false
		});

		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "002.jpg",
			ParentDirectory = "/QueryTest_next_hash",
			FileHash = "09457847385873",
			Tags = string.Empty,
			IsDirectory = false
		});

		// For previous item
		var single004 =
			_query.SingleItem("/QueryTest_next_hash/001.jpg");

		// check hash
		Assert.AreEqual("09457847385873",
			single004?.RelativeObjects.NextHash);

		// check path
		Assert.AreEqual("/QueryTest_next_hash/002.jpg",
			single004?.RelativeObjects.NextFilePath);
	}

	[TestMethod]
	public async Task QueryTest_CachingDirectoryConflicts_CheckIfContentIsInCacheUpdated()
	{
		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "CachingDeleted_001.jpg",
			ParentDirectory = "/CheckIfContentIsInCacheUpdated",
			FileHash = "567897",
			IsDirectory = false
		});

		// trigger caching
		_query.DisplayFileFolders("/CheckIfContentIsInCacheUpdated");

		var cachingDeleted001 =
			_query.SingleItem("/CheckIfContentIsInCacheUpdated/CachingDeleted_001.jpg")
				?.FileIndexItem;
		cachingDeleted001!.Tags = "#";
		await _query.UpdateItemAsync(cachingDeleted001);
		var cachingDeleted001Update =
			_query.SingleItem("/CheckIfContentIsInCacheUpdated/CachingDeleted_001.jpg")!
				.FileIndexItem;
		Assert.AreEqual("#", cachingDeleted001Update!.Tags);
		Assert.AreNotEqual(string.Empty, cachingDeleted001Update.Tags);
		// AreNotEqual: When its item used cache  it will return string.empty
	}

	[TestMethod]
	public void QueryFolder_Add_And_UpdateFolderCache_UpdateCacheItemTest()
	{
		var name = Query.CachingDbName(nameof(FileIndexItem),
			"/");

		// Add folder to cache normally done by: CacheQueryDisplayFileFolders
		_memoryCache.Set(name, new List<FileIndexItem>(),
			new TimeSpan(1, 0, 0));
		// "List`1_" is from CachingDbName

		var item = new FileIndexItem { Id = 400, FileName = "cache" };
		_query.AddCacheItem(item);

		var item1 = new FileIndexItem { Id = 400, Tags = "hi", FileName = "cache" };
		_query.CacheUpdateItem(new List<FileIndexItem> { item1 });

		_memoryCache.TryGetValue(name, out var objectFileFolders);
		var displayFileFolders = ( List<FileIndexItem>? ) objectFileFolders;
		Assert.IsNotNull(displayFileFolders);

		Assert.AreEqual("hi", displayFileFolders.Find(p => p.FileName == "cache")?.Tags);

		Assert.AreEqual(1, displayFileFolders.Count(p => p.FileName == "cache"));
	}

	[TestMethod]
	public void CacheUpdateItem_Skip_ShouldSetItem()
	{
		var item1 = new FileIndexItem { Id = 400, Tags = "hi", FileName = "cache" };

		// already verbose
		_query.CacheUpdateItem(new List<FileIndexItem> { item1 });

		Assert.AreNotEqual(0, _logger.TrackedInformation.Count);
		Assert.IsTrue(_logger.TrackedInformation.FirstOrDefault().Item2
			?.Contains("[CacheUpdateItem]"));
	}

	[TestMethod]
	public void CacheUpdateItem_Skip_ShouldSetItem1()
	{
		_logger.TrackedInformation = new List<(Exception?, string?)>();
		var item1 = new FileIndexItem { Id = 400, Tags = "hi", FileName = "cache" };
		// not verbose
		_queryNoVerbose.CacheUpdateItem(new List<FileIndexItem> { item1 });

		Assert.AreEqual(0, _logger.TrackedInformation.Count);
	}

	[TestMethod]
	public void CacheUpdateItem_ignore_when_parent_does_notExist()
	{
		var item1 = new FileIndexItem
		{
			Id = 400, Tags = "hi", ParentDirectory = "/_fail_test1", FileName = "cache"
		};
		_query.CacheUpdateItem(new List<FileIndexItem> { item1 });

		var name = Query.CachingDbName(nameof(FileIndexItem),
			"/_fail_test1");
		var success = _memoryCache.TryGetValue(name, out _);
		Assert.IsFalse(success);
	}

	[TestMethod]
	public void CacheUpdateItem_ImplicitAdd()
	{
		_query.AddCacheParentItem("/456789", new List<FileIndexItem>());

		var item1 = new FileIndexItem
		{
			Id = 400, Tags = "hi", ParentDirectory = "/456789", FileName = "cache"
		};
		_query.CacheUpdateItem(new List<FileIndexItem> { item1 });

		var result = _query.DisplayFileFolders("/456789").ToList();

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("hi", result[0].Tags);
	}

	[TestMethod]
	public void CacheUpdateItem_UpdateByName()
	{
		_query.AddCacheParentItem("/3479824783",
			new List<FileIndexItem>
			{
				new()
				{
					Id = 401,
					Tags = "___not___",
					ParentDirectory = "/3479824783",
					FileName = "cache"
				}
			});

		var item1 = new FileIndexItem
		{
			Id = 400, Tags = "hi", ParentDirectory = "/3479824783", FileName = "cache"
		};
		_query.CacheUpdateItem(new List<FileIndexItem> { item1 });

		var result = _query.DisplayFileFolders("/3479824783").ToList();

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("hi", result[0].Tags);
	}

	[TestMethod]
	public void CacheUpdateItem_ignore_when_parent_does_notExistFakeLogger()
	{
		var logger = new FakeIWebLogger();
		var query = new Query(null!, new AppSettings(), null!, logger, _memoryCache);

		var item1 = new FileIndexItem
		{
			Id = 400, Tags = "hi", ParentDirectory = "/_fail_test2", FileName = "cache"
		};
		query.CacheUpdateItem(new List<FileIndexItem> { item1 });

		var success = _memoryCache.TryGetValue("List`1_/_fail_test2", out _);

		Assert.IsFalse(success);
	}

	[TestMethod]
	public void CacheUpdateItem_shouldHitParentCache()
	{
		var folderPath = "/_fail_test";
		// Add folder to cache normally done by: CacheQueryDisplayFileFolders
		_memoryCache.Set($"List`1_{folderPath}", new List<FileIndexItem>(),
			new TimeSpan(1, 0, 0));
		// "List`1_" is from CachingDbName

		var item = new FileIndexItem { Id = 400, FileName = "cache", ParentDirectory = folderPath };
		_query.AddCacheParentItem(folderPath, new List<FileIndexItem> { item });

		var item1 = new FileIndexItem
		{
			Id = 400, Tags = "hi", ParentDirectory = folderPath, FileName = "cache"
		};
		_query.CacheUpdateItem(new List<FileIndexItem> { item1 });

		var success = _memoryCache.TryGetValue($"List`1_{folderPath}", out _);
		Assert.IsTrue(success);
	}

	[TestMethod]
	public async Task DisplayFileFolders_StackCollection()
	{
		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "StackCollection001.jpg",
			ParentDirectory = "/StackCollection",
			FileHash = "9876789",
			Tags = string.Empty,
			IsDirectory = false
		});

		await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "StackCollection001.dng",
			ParentDirectory = "/StackCollection",
			FileHash = "9876789",
			Tags = string.Empty,
			IsDirectory = false
		});

		var dp1 = _query.DisplayFileFolders("/StackCollection");
		Assert.AreEqual(1, dp1.Count());

		var dp2 = _query.DisplayFileFolders("/StackCollection", null, false);
		Assert.AreEqual(2, dp2.Count());
	}

	[TestMethod]
	public async Task Query_updateStatusContentList()
	{
		// for updateing multiple items
		var toUpdate = new List<FileIndexItem>
		{
			new()
			{
				Tags = "test",
				FileName = "3456784567890987654.jpg",
				ParentDirectory = "/3456784567890987654",
				FileHash = "3456784567890987654"
			}
		};
		await _query.AddItemAsync(toUpdate.FirstOrDefault()!);

		foreach ( var item in toUpdate )
		{
			item.Tags = "updated";
		}

		await _query.UpdateItemAsync(toUpdate);

		var fileObjectByFilePath =
			await _query.GetObjectByFilePathAsync("/3456784567890987654/3456784567890987654.jpg");
		Assert.AreEqual("updated", fileObjectByFilePath?.Tags);
	}

	[TestMethod]
	public async Task Query_updateStatusContentList_Async()
	{
		// for update-ing multiple items
		var toUpdate = new List<FileIndexItem>
		{
			new()
			{
				Tags = "test",
				FileName = "9278521.jpg",
				ParentDirectory = "/8118",
				FileHash = "3456784567890987654"
			}
		};
		await _query.AddItemAsync(toUpdate.FirstOrDefault()!);

		foreach ( var item in toUpdate )
		{
			item.Tags = "updated";
		}

		await _query.UpdateItemAsync(toUpdate);

		var fileObjectByFilePath = await _query.GetObjectByFilePathAsync("/8118/9278521.jpg");
		Assert.AreEqual("updated", fileObjectByFilePath?.Tags);
	}

	[TestMethod]
	public void Query_IsCacheEnabled_True()
	{
		Assert.IsTrue(_query.IsCacheEnabled());
	}

	[TestMethod]
	public async Task AddItemAsync()
	{
		var item = await _query.AddItemAsync(new FileIndexItem("/test/test.jpg"));

		var result = _query.SingleItem("/test/test.jpg");

		Assert.IsNotNull(result?.FileIndexItem);
		Assert.AreEqual("/test/test.jpg", result.FileIndexItem.FilePath);

		await _query.RemoveItemAsync(item);
	}

	[TestMethod]
	public async Task AddItemAsync_Disposed()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext, new AppSettings(), serviceScope, new FakeIWebLogger(),
			_memoryCache);

		await dbContext.DisposeAsync();
		await query.AddItemAsync(new FileIndexItem("/test982.jpg") { Tags = "test" });

		var dbContext2 = new InjectServiceScope(serviceScope).Context();
		var itemItShouldContain =
			await dbContext2.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test982.jpg");
		Assert.IsNotNull(itemItShouldContain);
		Assert.AreEqual("test", itemItShouldContain.Tags);
	}

	[TestMethod]
	public async Task RemoveItemAsync()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext, new AppSettings(), serviceScope, new FakeIWebLogger(),
			_memoryCache);

		await dbContext.FileIndex.AddAsync(new FileIndexItem("/test44.jpg"));
		await dbContext.SaveChangesAsync();

		var item =
			await dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test44.jpg");
		Assert.IsNotNull(item);
		await query.RemoveItemAsync(item);

		var itemItShouldBeNull =
			await dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test44.jpg");
		Assert.IsNull(itemItShouldBeNull);
	}

	[TestMethod]
	public async Task RemoveItemAsync_Disposed()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		var query = new Query(dbContext, new AppSettings(), serviceScope, new FakeIWebLogger(),
			_memoryCache);

		await dbContext.FileIndex.AddAsync(new FileIndexItem("/test44.jpg"));
		await dbContext.SaveChangesAsync();

		var item =
			await dbContext.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test44.jpg");

		await dbContext.DisposeAsync();

		Assert.IsNotNull(item);
		await query.RemoveItemAsync(item);

		var dbContext2 = new InjectServiceScope(serviceScope).Context();
		var itemItShouldBeNull =
			await dbContext2.FileIndex.FirstOrDefaultAsync(p => p.FilePath == "/test44.jpg");
		Assert.IsNull(itemItShouldBeNull);
	}

	[TestMethod]
	public void RemoveCacheItem_ShouldKeepOne()
	{
		var dirPath = "/__cache__remove_test";
		var demoItems =
			new List<FileIndexItem>
			{
				new(dirPath + "/01.jpg"), new(dirPath + "/02.jpg"), new(dirPath + "/03.jpg")
			};

		_query.AddCacheParentItem(dirPath, demoItems);

		_query.RemoveCacheItem(new List<FileIndexItem> { demoItems[0], demoItems[1] });

		var result = _query.DisplayFileFolders(dirPath).ToList();
		Assert.AreEqual(1, result.Count);
		Assert.AreEqual(dirPath + "/03.jpg", result[0].FilePath);
	}

	[TestMethod]
	public async Task RetrySaveChangesAsync_AggregateException()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var query = new Query(dbContext, new AppSettings(), null!, new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<AggregateException>(async () =>
			await query.RetryQueryUpdateSaveChangesAsync(new FileIndexItem { Id = 1 },
				new Exception(), "test", 0));
	}

	[TestMethod]
	public async Task RetrySaveChangesAsync_id0_so_skip()
	{
		var serviceScope = CreateNewScope();
		var scope = serviceScope.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		var query = new Query(dbContext, new AppSettings(), null!, new FakeIWebLogger());
		var result =
			await query.RetryQueryUpdateSaveChangesAsync(new FileIndexItem { Id = 0 },
				new Exception(), "test", 0);

		Assert.IsNull(result);
	}
}

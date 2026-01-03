using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.sync.SyncServices;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.sync.SyncServices;

[TestClass]
public sealed class SyncFolderTest
{
	private readonly AppSettings _appSettings;
	private readonly IQuery _query;

	public SyncFolderTest()
	{
		_appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
			SyncIgnore = new List<string> { "/.git" }
		};

		( _query, _ ) = CreateNewExampleData();
	}

	[SuppressMessage("ReSharper",
		"ArrangeObjectCreationWhenTypeEvident",
		Justification = "new fileIndexItem")]
	private Tuple<IQuery, IServiceScopeFactory> CreateNewExampleData()
	{
		var services = new ServiceCollection();
		var serviceProvider = services.BuildServiceProvider();

		services.AddScoped(_ => _appSettings);
		var query = new FakeIQuery(new List<FileIndexItem>
		{
			new FileIndexItem("/folder_no_content/") { IsDirectory = true },
			new FileIndexItem("/folder_content") { IsDirectory = true },
			new FileIndexItem("/folder_content/test.jpg"),
			new FileIndexItem("/folder_content/test2.jpg")
		});
		services.AddScoped<IQuery, FakeIQuery>(_ => query);
		var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		return new Tuple<IQuery, IServiceScopeFactory>(query, serviceScopeFactory);
	}

	private static FakeIStorage GetStorage()
	{
		return new FakeIStorage(
			new List<string> { "/", "/folder_01", "/folder_no_content" },
			new List<string> { "/test1.jpg", "/test2.jpg", "/test3.jpg", "/folder_01/test4.jpg" },
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(),
				CreateAnImageColorClass.Bytes.ToArray(),
				CreateAnImageNoExif.Bytes.ToArray(),
				CreateAnImage.Bytes.ToArray()
			});
	}

	[TestMethod]
	public async Task Folder_Dir_NotFound()
	{
		var storage = new FakeIStorage();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);
		var result = await syncFolder.Folder("/not_found");

		Assert.AreEqual("/not_found", result[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result[0].Status);
	}

	[TestMethod]
	public async Task Folder_FolderWithNoContent()
	{
		var storage = GetStorage();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);
		var result = await syncFolder.Folder("/folder_no_content");

		Assert.AreEqual("/folder_no_content", result[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
	}

	[TestMethod]
	public async Task Folder_Ignored_Due_Filter()
	{
		var storage = new FakeIStorage(
			new List<string> { "/", "/test_01", "/test_01/test_02" },
			new List<string> { "/test_01/test_02/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);

		// with filter
		var result = await syncFolder.Folder("/test_01", // FILTER applied
			null, DateTime.MaxValue);

		var childFolder =
			result.Find(p => p.FilePath == "/test_01/test_02/test.jpg");

		// are NOT equal
		Assert.AreNotEqual("/test_01/test_02/test.jpg", childFolder?.FilePath);
		Assert.AreNotEqual(FileIndexItem.ExifStatus.Ok, childFolder?.Status);
	}

	[TestMethod]
	public async Task Folder_AppliedWith_Filter()
	{
		var storage = new FakeIStorage(
			["/", "/folder_01", "/folder_01/folder_02"],
			["/folder_01/folder_02/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);

		// Filter applied
		var result = await syncFolder.Folder("/folder_01", null,
			// Filter applied
			DateTime.Now.AddDays(-1));

		var childFolder =
			result.Find(p => p.FilePath == "/folder_01/folder_02/test.jpg");
		Assert.AreEqual("/folder_01/folder_02/test.jpg", childFolder?.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, childFolder?.Status);
	}

	[TestMethod]
	public async Task Folder_AppliedWith_No_Filter()
	{
		var storage = new FakeIStorage(
			["/", "/folder_01", "/folder_01/folder_02"],
			["/folder_01/folder_02/test.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);

		var result = await syncFolder.Folder("/folder_01");

		var childFolder =
			result.Find(p => p.FilePath == "/folder_01/folder_02/test.jpg");
		Assert.AreEqual("/folder_01/folder_02/test.jpg", childFolder?.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, childFolder?.Status);
	}

	[TestMethod]
	public async Task Folder_FileSizeIsChanged()
	{
		var subPath = "/change/test_change.jpg";
		await _query.AddItemAsync(new FileIndexItem(subPath) { Size = 123456 });

		var storage = GetStorage();
		await storage.WriteStreamAsync(new MemoryStream(CreateAnImage.Bytes.ToArray()),
			subPath);

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);
		var result = await syncFolder.Folder("/change");

		Assert.AreEqual(subPath, result[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
		Assert.AreNotEqual(123456, result[0].Size);
		Assert.IsFalse(string.IsNullOrWhiteSpace(result[0].Tags));
	}

	[TestMethod]
	public async Task Folder_DuplicateChildItems()
	{
		var storage = new FakeIStorage(
			new List<string> { "/", "/Folder_Duplicate" },
			new List<string> { "/Folder_Duplicate/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		// yes this is duplicate!
		await _query.AddItemAsync(new FileIndexItem("/Folder_Duplicate/test.jpg"));
		await _query.AddItemAsync(
			new FileIndexItem("/Folder_Duplicate/test.jpg")); // yes this is duplicate!

		var queryResultBefore = await _query.GetAllFilesAsync("/Folder_Duplicate");
		Assert.HasCount(2, queryResultBefore);

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);

		var result = ( await syncFolder.Folder(
			"/Folder_Duplicate") ).Where(p => p.FilePath != "/").ToList();

		Assert.HasCount(2, result);
		var queryResult = await _query.GetAllFilesAsync("/Folder_Duplicate");
		Assert.HasCount(1, queryResult);

		await _query.RemoveItemAsync(queryResult[0]);
	}

	[TestMethod]
	public async Task Folder_ChildItemDateTimeLastEditNotChanged()
	{
		var storage = new FakeIStorage(
			new List<string> { "/", "/same_test" },
			new List<string> { "/same_test/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() },
			new List<DateTime> { new(2000, 01, 01, 01, 01, 01, DateTimeKind.Local) });

		await _query.AddItemAsync(new FileIndexItem("/same_test/test.jpg")
		{
			LastEdited = new DateTime(2000, 01, 01,
				01, 01, 01, DateTimeKind.Local)
		});

		var queryResultBefore = await _query.GetAllFilesAsync("/same_test");
		Assert.HasCount(1, queryResultBefore);

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);

		var result = ( await syncFolder.Folder(
			"/same_test") ).Where(p => p.FilePath != "/").ToList();

		Assert.HasCount(2, result); // folder and item in folder
		Assert.AreEqual(1, result.Count(p => p.FileName == "test.jpg"));
		Assert.AreEqual(1, result.Count(p => p.FileName == "same_test"));
		Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, result[0].Status);


		var queryResult = await _query.GetAllFilesAsync("/same_test");
		Assert.HasCount(1, queryResult);
		Assert.AreEqual(1, queryResult.Count(p => p.FileName == "test.jpg"));

		Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame,
			queryResult.Find(p => p.FileName == "test.jpg")?.Status);

		await _query.RemoveItemAsync(queryResult[0]);
	}


	[TestMethod]
	public async Task Folder_ChildItemDateTimeLastEditChanged()
	{
		var storage = new FakeIStorage(
			new List<string> { "/", "/same_test" },
			new List<string> { "/same_test/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() }, new List<DateTime>
			{
				new(3000, 01,
					01, 01, 01, 01, DateTimeKind.Local)
			});

		await _query.AddItemAsync(new FileIndexItem("/same_test/test.jpg")
		{
			LastEdited = new DateTime(2000, 01, 01,
				01, 01, 01, DateTimeKind.Local)
		});

		var queryResultBefore = await _query.GetAllFilesAsync("/same_test");
		Assert.HasCount(1, queryResultBefore);

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);

		var result = ( await syncFolder.Folder(
			"/same_test") ).Where(p => p.FilePath != "/").ToList();

		Assert.HasCount(2, result); // folder and item in folder
		Assert.AreEqual(1, result.Count(p => p.FileName == "test.jpg"));
		Assert.AreEqual(1, result.Count(p => p.FileName == "same_test"));
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);


		var queryResult = await _query.GetAllFilesAsync("/same_test");
		Assert.HasCount(1, queryResult);
		Assert.AreEqual(1, queryResult.Count(p => p.FileName == "test.jpg"));

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok,
			queryResult.Find(p => p.FileName == "test.jpg")?.Status);

		await _query.RemoveItemAsync(queryResult[0]);
	}

	[TestMethod]
	public async Task Folder_ShouldAddFolderItSelfAndParentFolders()
	{
		var storage = GetStorage();
		var folderPath = "/should_add_root";
		storage.CreateDirectory(folderPath);

		var query = new FakeIQuery();

		var syncFolder = new SyncFolder(_appSettings, query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);
		var result = ( await syncFolder.Folder(folderPath) ).Where(p => p.FilePath != "/").ToList();

		Assert.IsNotNull(query.GetObjectByFilePathAsync("/"));
		Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
		Assert.HasCount(1, result);
		Assert.AreEqual(folderPath, result[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);
	}

	[TestMethod]
	public async Task AddParentFolder_NewFolders()
	{
		var storage = GetStorage();
		var folderPath = "/should_add_root2";
		storage.CreateDirectory(folderPath);
		storage.CreateDirectory("/");

		var query = new FakeIQuery();
		await query.AddItemAsync(new FileIndexItem("/") { IsDirectory = true });

		var syncFolder = new SyncFolder(_appSettings, query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);
		var result = await syncFolder.AddParentFolder(folderPath, null);

		Assert.IsNotNull(query.GetObjectByFilePathAsync("/"));
		Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
		var item = result.Find(p =>
			p.FilePath == folderPath &&
			p.Status == FileIndexItem.ExifStatus.Ok);

		Assert.AreEqual(folderPath, item?.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, item?.Status);
	}

	[TestMethod]
	public async Task AddParentFolder_ExistingFolder()
	{
		var storage = GetStorage();
		storage.CreateDirectory("/exist2");
		storage.CreateDirectory("/");

		var folderPath = "/exist2";

		var query = new FakeIQuery();

		var syncFolder = new SyncFolder(_appSettings, query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);
		var result = ( await syncFolder.AddParentFolder(folderPath, null) )
			.Where(p => p.FilePath != "/").ToList();

		Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
		Assert.AreEqual(folderPath, result.FirstOrDefault()?.FilePath);

		// should not add duplicate content
		var allItems = await query.GetAllRecursiveAsync();

		Assert.HasCount(1, allItems);
		Assert.AreEqual(folderPath, allItems[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, allItems[0].Status);
	}

	[TestMethod]
	public async Task AddParentFolder_NotFound()
	{
		var storage = GetStorage();
		var folderPath = "/not-found";

		var query = new FakeIQuery(new List<FileIndexItem> { new("/") });

		var syncFolder = new SyncFolder(_appSettings, query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);
		var result = await syncFolder.AddParentFolder(folderPath, null);

		Assert.IsNotNull(query.GetObjectByFilePathAsync(folderPath));
		var item = result.Find(p => p.FilePath == folderPath);

		Assert.IsNotNull(item);
		Assert.AreEqual(folderPath, item.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, item.Status);

		// should not add content
		var allItems = await query.GetAllRecursiveAsync();
		Assert.IsEmpty(allItems);
	}

	[TestMethod]
	public async Task AddParentFolder_InListSoSkip()
	{
		var query = new FakeIQuery(new List<FileIndexItem> { new("/") });
		var syncFolder = new SyncFolder(_appSettings, query, new FakeSelectorStorage(GetStorage()),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);
		var result = await syncFolder.AddParentFolder("/test",
			new List<FileIndexItem> { new("/test") });
		Assert.AreEqual(0,
			result.Count(p => p.Status != FileIndexItem.ExifStatus.OkAndSame));
	}

	[TestMethod]
	public void PathsToUpdateInDatabase_FilesOnDiskButNotInTheDb()
	{
		var results = SyncFolder.PathsToUpdateInDatabase(
			new List<FileIndexItem>(), new List<string> { "/test.jpg" });

		Assert.HasCount(1, results);
		Assert.AreEqual("/test.jpg", results[0].FilePath);
	}

	[TestMethod]
	public void PathsToUpdateInDatabase_InDbButNotOnDisk()
	{
		var results = SyncFolder.PathsToUpdateInDatabase(
			new List<FileIndexItem> { new("/test.jpg") }, Array.Empty<string>());

		Assert.HasCount(1, results);
		Assert.AreEqual("/test.jpg", results[0].FilePath);
	}

	[TestMethod]
	public void PathsToUpdateInDatabase_ExistBoth()
	{
		var results = SyncFolder.PathsToUpdateInDatabase(
			new List<FileIndexItem> { new("/test.jpg") }, new List<string> { "/test.jpg" });

		Assert.HasCount(1, results);
		Assert.AreEqual("/test.jpg", results[0].FilePath);
	}

	[TestMethod]
	public void PathsToUpdateInDatabase_Duplicates()
	{
		var results = SyncFolder.PathsToUpdateInDatabase(
			new List<FileIndexItem>(), new List<string> { "/test.jpg", "/test.jpg" });

		Assert.HasCount(1, results);
		Assert.AreEqual("/test.jpg", results[0].FilePath);
	}

	[TestMethod]
	public async Task Folder_DuplicateFolders_Implicit()
	{
		await _query.AddItemAsync(new FileIndexItem("/DuplicateFolder") { IsDirectory = true });
		// yes this is duplicate
		await _query.AddItemAsync(new FileIndexItem("/DuplicateFolder") { IsDirectory = true });

		var storage =
			new FakeIStorage(new List<string> { "/", "/DuplicateFolder" });
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);

		await syncFolder.Folder("/");

		var allFolders = ( _query as FakeIQuery )?.GetAllFolders();
		if ( allFolders == null )
		{
			throw new NullReferenceException(
				"all folder should not be null");
		}

		Assert.AreEqual("/", allFolders.Find(p => p.FilePath == "/")?.FilePath);
		Assert.AreEqual(1, allFolders.Count(p => p.FilePath == "/DuplicateFolder"));
	}

	[TestMethod]
	public async Task Folder_DuplicateFolders_Direct()
	{
		await _query.AddItemAsync(new FileIndexItem("/DuplicateFolder") { IsDirectory = true });
		// yes this is duplicate
		await _query.AddItemAsync(new FileIndexItem("/DuplicateFolder") { IsDirectory = true });

		var storage =
			new FakeIStorage(new List<string> { "/", "/DuplicateFolder" });
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);

		await syncFolder.Folder("/DuplicateFolder");

		var allFolders = ( _query as FakeIQuery )?.GetAllFolders()
			.Where(p => p.FilePath == "/DuplicateFolder").ToList();
		Assert.IsNotNull(allFolders);

		Assert.AreEqual("/DuplicateFolder",
			allFolders.Find(p => p.FilePath == "/DuplicateFolder")?.FilePath);
		Assert.AreEqual(1, allFolders.Count(p => p.FilePath == "/DuplicateFolder"));
	}

	[TestMethod]
	public async Task Folder_ShouldIgnore()
	{
		var storage = new FakeIStorage(
			new List<string> { "/", "/test_ignore", "/test_ignore/ignore" },
			new List<string> { "/test_ignore/ignore/test1.jpg" },
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(),
				CreateAnImageColorClass.Bytes.ToArray(),
				CreateAnImageNoExif.Bytes.ToArray()
			});

		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
			Verbose = true,
			SyncIgnore = new List<string> { "/test_ignore/ignore" }
		};

		var syncFolder = new SyncFolder(appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);
		var result = await syncFolder.Folder("/test_ignore");

		Assert.AreEqual("/test_ignore/ignore/test1.jpg", result[0].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result[0].Status);

		var files = await _query.GetAllFilesAsync("/test_ignore");

		Assert.IsEmpty(files);
	}

	[TestMethod]
	public async Task RemoveChildItems_Floating_items()
	{
		await _query.AddItemAsync(
			new FileIndexItem("/Folder_InDbButNotOnDisk3") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk3/test.jpg"));
		await _query.AddItemAsync(
			new FileIndexItem("/Folder_InDbButNotOnDisk3/test_dir") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/Folder_InDbButNotOnDisk3/test_dir/test.jpg"));

		var storage = new FakeIStorage();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var rootItem = await _query.GetObjectByFilePathAsync("/Folder_InDbButNotOnDisk3");
		Assert.IsNotNull(rootItem);
		var result = await syncFolder.RemoveChildItems(_query, rootItem);

		Assert.AreEqual("/Folder_InDbButNotOnDisk3", result.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result.Status);

		var data = await _query.GetAllRecursiveAsync("/Folder_InDbButNotOnDisk3");
		Assert.IsEmpty(data);
	}

	[TestMethod]
	public async Task CompareFolderListAndFixMissingFoldersTest_Ok()
	{
		var storage =
			new FakeIStorage(new List<string> { "/", "/2018", "/2018/02", "/2018/02/2018_02_01" });
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		await syncFolder.CompareFolderListAndFixMissingFolders(
			new List<string> { "/", "/2018", "/2018/02", "/2018/02/2018_02_01" },
			new List<FileIndexItem> { new("/2018") });

		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018"));
		Assert.AreEqual("/2018/02",
			( await _query.GetObjectByFilePathAsync("/2018/02") )?.FilePath);
		Assert.AreEqual("/2018/02/2018_02_01",
			( await _query.GetObjectByFilePathAsync("/2018/02/2018_02_01") )?.FilePath);
	}

	[TestMethod]
	public async Task CompareFolderListAndFixMissingFoldersTest_Ignored()
	{
		var storage = new FakeIStorage(new List<string> { "/", "/.git", "/test" });
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		await syncFolder.CompareFolderListAndFixMissingFolders(
			new List<string> { "/", "/.git" },
			new List<FileIndexItem> { new("/") });

		Assert.IsNull(await _query.GetObjectByFilePathAsync("/.git"));
	}

	[TestMethod]
	public async Task CompareFolderListAndFixMissingFoldersTest_Ok_SameCount()
	{
		var storage =
			new FakeIStorage(new List<string> { "/", "/2018", "/2018/02", "/2018/02/2018_02_01" });
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		await syncFolder.CompareFolderListAndFixMissingFolders(
			new List<string> { "/", "/2018", "/2018/02", "/2018/02/2018_02_01" },
			new List<FileIndexItem>
			{
				new("/"), new("/2018"), new("/2018/02"), new("/2018/02/2018_02_01")
			}
		);

		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018"));
		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018/02"));
		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018/02/2018_02_01"));
	}

	[TestMethod]
	public async Task CompareFolderListAndFixMissingFoldersTest_NotFound()
	{
		var storage = new FakeIStorage(new List<string> { "/" });
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		await syncFolder.CompareFolderListAndFixMissingFolders(
			new List<string> { "/", "/2018", "/2018/02", "/2018/02/2018_02_01" },
			new List<FileIndexItem> { new("/2018") });

		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018"));
		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018/02"));
		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018/02/2018_02_01"));
	}

	[TestMethod]
	public void DisplayInlineConsole_default()
	{
		var consoleWrapper = new FakeConsoleWrapper();
		SyncFolder.DisplayInlineConsole(consoleWrapper,
			new List<FileIndexItem> { new("/test.jpg") });
		Assert.AreEqual("⁑", consoleWrapper.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public void DisplayInlineConsole_DeletedAndSame()
	{
		var consoleWrapper = new FakeConsoleWrapper();
		SyncFolder.DisplayInlineConsole(consoleWrapper,
			new List<FileIndexItem>
			{
				new("/test.jpg") { Status = FileIndexItem.ExifStatus.DeletedAndSame }
			});
		Assert.AreEqual("✘", consoleWrapper.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public void DisplayInlineConsole_Deleted()
	{
		var consoleWrapper = new FakeConsoleWrapper();
		SyncFolder.DisplayInlineConsole(consoleWrapper,
			new List<FileIndexItem>
			{
				new("/test.jpg") { Status = FileIndexItem.ExifStatus.Deleted }
			});
		Assert.AreEqual("֍", consoleWrapper.WrittenLines.LastOrDefault());
	}
	
	[TestMethod]
	public async Task RemoveChildItems_ShouldAbort_WhenFolderExists()
	{
		// Setup: Folder in DB but exists on disk (race condition scenario)
		await _query.AddItemAsync(
			new FileIndexItem("/race_condition_folder") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/race_condition_folder/test.jpg"));

		// Storage has the folder (simulating folder created between checks)
		var storage = new FakeIStorage(
			new List<string> { "/", "/race_condition_folder" },
			new List<string> { "/race_condition_folder/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var rootItem = await _query.GetObjectByFilePathAsync("/race_condition_folder");
		Assert.IsNotNull(rootItem);

		// Act: Try to remove child items - should be aborted because folder exists
		var result = await syncFolder.RemoveChildItems(_query, rootItem);

		// Assert: Should NOT delete, status should be Ok
		Assert.AreEqual("/race_condition_folder", result.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result.Status);

		// Verify items still exist in database
		var childrenStillExist = await _query.GetAllRecursiveAsync("/race_condition_folder");
		Assert.HasCount(1, childrenStillExist);
		Assert.AreEqual("/race_condition_folder/test.jpg", childrenStillExist[0].FilePath);
	}

	[TestMethod]
	public async Task RemoveChildItems_ShouldDelete_WhenFolderTrulyDoesNotExist()
	{
		// Setup: Folder in DB but NOT on disk
		await _query.AddItemAsync(
			new FileIndexItem("/truly_missing_folder") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/truly_missing_folder/test.jpg"));

		// Storage does NOT have the folder
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string>(),
			new List<byte[]>());

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var rootItem = await _query.GetObjectByFilePathAsync("/truly_missing_folder");
		Assert.IsNotNull(rootItem);

		// Act: Remove child items
		var result = await syncFolder.RemoveChildItems(_query, rootItem);

		// Assert: Should delete, status should be NotFoundSourceMissing
		Assert.AreEqual("/truly_missing_folder", result.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result.Status);

		// Verify items are deleted from database
		var childrenDeleted = await _query.GetAllRecursiveAsync("/truly_missing_folder");
		Assert.IsEmpty(childrenDeleted);
	}

	[TestMethod]
	public async Task CheckIfFolderExistOnDisk_ShouldSkipDeletion_WhenChildrenExist()
	{
		// Setup: Folder doesn't exist on disk, but has children in DB (race condition)
		await _query.AddItemAsync(
			new FileIndexItem("/folder_with_children") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/folder_with_children/child1.jpg"));
		await _query.AddItemAsync(new FileIndexItem("/folder_with_children/child2.jpg"));

		// Storage does NOT have the folder (simulating timing issue)
		var storage = new FakeIStorage(new List<string> { "/" }, new List<string>(),
			new List<byte[]>());

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act: Sync the folder
		var result = await syncFolder.Folder("/");

		// Assert: Children should NOT be deleted
		var childrenStillExist = await _query.GetAllRecursiveAsync("/folder_with_children");
		Assert.HasCount(2, childrenStillExist);

		// Verify log message about skipping deletion
		Assert.IsTrue(logger.TrackedInformation.Any(log =>
			log.Item2?.Contains("[SyncFolder] Skipping deletion") &&
			log.Item2?.Contains("/folder_with_children")));
	}

	[TestMethod]
	public async Task CheckIfFolderExistOnDisk_ShouldSkipDeletion_WhenSubdirectoriesExist()
	{
		// Setup: Folder doesn't exist, but has subdirectories on disk
		await _query.AddItemAsync(
			new FileIndexItem("/parent_folder") { IsDirectory = true });

		// Storage has subdirectories (simulating folder structure being scanned)
		var storage = new FakeIStorage(
			new List<string> { "/", "/parent_folder/subfolder1", "/parent_folder/subfolder2" },
			new List<string>(), new List<byte[]>());

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act: Sync should skip deletion because subdirectories exist
		var result = await syncFolder.Folder("/");

		// Assert: Parent folder should still exist in DB
		var parentStillExists = await _query.GetObjectByFilePathAsync("/parent_folder");
		Assert.IsNotNull(parentStillExists);

		// Verify log message about skipping deletion
		Assert.IsTrue(logger.TrackedInformation.Any(log =>
			log.Contains("[SyncFolder] Skipping deletion") &&
			log.Contains("subdirectories exist")));
	}

	[TestMethod]
	public async Task Folder_RaceCondition_ParallelSync_ShouldNotDeleteContent()
	{
		// Setup: Complex folder structure to test parallel processing
		await _query.AddItemAsync(new FileIndexItem("/photos") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/photos/2024") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/photos/2024/01") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/photos/2024/01/photo1.jpg"));
		await _query.AddItemAsync(new FileIndexItem("/photos/2024/01/photo2.jpg"));

		// Storage has the full structure
		var storage = new FakeIStorage(
			new List<string> { "/", "/photos", "/photos/2024", "/photos/2024/01" },
			new List<string> { "/photos/2024/01/photo1.jpg", "/photos/2024/01/photo2.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });

		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
			MaxDegreesOfParallelism = 3 // Enable parallel processing
		};

		var syncFolder = new SyncFolder(appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act: Sync with parallel processing
		var result = await syncFolder.Folder("/photos");

		// Assert: All items should still exist
		var allItems = await _query.GetAllRecursiveAsync("/photos");
		Assert.IsGreaterThanOrEqualTo(2, allItems.Count, "Photos should not be deleted");

		var photo1 = await _query.GetObjectByFilePathAsync("/photos/2024/01/photo1.jpg");
		var photo2 = await _query.GetObjectByFilePathAsync("/photos/2024/01/photo2.jpg");

		Assert.IsNotNull(photo1, "photo1.jpg should not be deleted");
		Assert.IsNotNull(photo2, "photo2.jpg should not be deleted");
	}

	[TestMethod]
	public async Task RemoveChildItems_WithMultipleChildren_ShouldLogCount()
	{
		// Setup: Folder with multiple children
		await _query.AddItemAsync(
			new FileIndexItem("/folder_many_items") { IsDirectory = true });
		for ( var i = 1; i <= 10; i++ )
		{
			await _query.AddItemAsync(
				new FileIndexItem($"/folder_many_items/photo{i}.jpg"));
		}

		var storage = new FakeIStorage(new List<string> { "/" }, new List<string>(),
			new List<byte[]>());

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var rootItem = await _query.GetObjectByFilePathAsync("/folder_many_items");
		Assert.IsNotNull(rootItem);

		// Act: Remove child items
		var result = await syncFolder.RemoveChildItems(_query, rootItem);

		// Assert: Should log the count of items being removed
		Assert.IsTrue(logger.TrackedInformation.Any(log =>
			log.Item2.Contains("[SyncFolder] Removing 10 child items")));
	}

	[TestMethod]
	public async Task Folder_EmptyFolderInDB_NotOnDisk_ShouldBeRemoved()
	{
		// Setup: Empty folder in DB, not on disk - should be cleaned up
		await _query.AddItemAsync(
			new FileIndexItem("/truly_empty_folder") { IsDirectory = true });

		var storage = new FakeIStorage(new List<string> { "/" }, new List<string>(),
			new List<byte[]>());

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act: Sync should remove empty folder
		var result = await syncFolder.Folder("/");

		// Assert: Empty folder should be marked as deleted
		var deletedItem =
			result.FirstOrDefault(r => r.FilePath == "/truly_empty_folder");
		Assert.IsNotNull(deletedItem);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, deletedItem.Status);
	}

	[TestMethod]
	public async Task Folder_WithRecentlyAddedChildren_ShouldNotBeDeleted()
	{
		// Setup: Folder being actively synced (simulating DiskWatcher adding items)
		await _query.AddItemAsync(new FileIndexItem("/active_sync_folder")
			{ IsDirectory = true });

		var storage = new FakeIStorage(
			new List<string> { "/", "/active_sync_folder" },
			new List<string> { "/active_sync_folder/new_photo.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Simulate race: Add child to DB while sync is running
		await _query.AddItemAsync(new FileIndexItem("/active_sync_folder/new_photo.jpg"));

		// Act: Sync should detect the child and not delete folder
		var result = await syncFolder.Folder("/");

		// Assert: Folder and child should still exist
		var folder = await _query.GetObjectByFilePathAsync("/active_sync_folder");
		var child = await _query.GetObjectByFilePathAsync("/active_sync_folder/new_photo.jpg");

		Assert.IsNotNull(folder, "Folder should not be deleted");
		Assert.IsNotNull(child, "Child item should not be deleted");
	}
}

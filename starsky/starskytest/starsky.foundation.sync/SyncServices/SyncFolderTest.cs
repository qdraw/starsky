using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Helpers;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
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
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase, SyncIgnore = ["/.git"]
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
		var query = new FakeIQuery([
			new FileIndexItem("/folder_no_content/") { IsDirectory = true },
			new FileIndexItem("/folder_content") { IsDirectory = true },
			new FileIndexItem("/folder_content/test.jpg"),
			new FileIndexItem("/folder_content/test2.jpg")
		]);
		services.AddScoped<IQuery, FakeIQuery>(_ => query);
		var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
		return new Tuple<IQuery, IServiceScopeFactory>(query, serviceScopeFactory);
	}

	private static FakeIStorage GetStorage()
	{
		return new FakeIStorage(
			["/", "/folder_01", "/folder_no_content"],
			["/test1.jpg", "/test2.jpg", "/test3.jpg", "/folder_01/test4.jpg"],
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
			["/", "/test_01", "/test_01/test_02"],
			["/test_01/test_02/test.jpg"],
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
			["/", "/Folder_Duplicate"],
			["/Folder_Duplicate/test.jpg"],
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
			["/", "/same_test"],
			["/same_test/test.jpg"],
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
		Assert.ContainsSingle(p => p.FileName == "test.jpg", result);
		Assert.ContainsSingle(p => p.FileName == "same_test", result);
		Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame, result[0].Status);


		var queryResult = await _query.GetAllFilesAsync("/same_test");
		Assert.HasCount(1, queryResult);
		Assert.ContainsSingle(p => p.FileName == "test.jpg", queryResult);

		Assert.AreEqual(FileIndexItem.ExifStatus.OkAndSame,
			queryResult.Find(p => p.FileName == "test.jpg")?.Status);

		await _query.RemoveItemAsync(queryResult[0]);
	}


	[TestMethod]
	public async Task Folder_ChildItemDateTimeLastEditChanged()
	{
		var storage = new FakeIStorage(
			["/", "/same_test"],
			["/same_test/test.jpg"],
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
		Assert.ContainsSingle(p => p.FileName == "test.jpg", result);
		Assert.ContainsSingle(p => p.FileName == "same_test", result);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result[0].Status);


		var queryResult = await _query.GetAllFilesAsync("/same_test");
		Assert.HasCount(1, queryResult);
		Assert.ContainsSingle(p => p.FileName == "test.jpg", queryResult);

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

		var query = new FakeIQuery([new FileIndexItem("/")]);

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
		var query = new FakeIQuery([new FileIndexItem("/")]);
		var syncFolder = new SyncFolder(_appSettings, query, new FakeSelectorStorage(GetStorage()),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);
		var result = await syncFolder.AddParentFolder("/test",
			[new FileIndexItem("/test")]);
		Assert.AreEqual(0,
			result.Count(p => p.Status != FileIndexItem.ExifStatus.OkAndSame));
	}

	[TestMethod]
	public void PathsToUpdateInDatabase_FilesOnDiskButNotInTheDb()
	{
		var results = SyncFolder.PathsToUpdateInDatabase(
			[], new List<string> { "/test.jpg" });

		Assert.HasCount(1, results);
		Assert.AreEqual("/test.jpg", results[0].FilePath);
	}

	[TestMethod]
	public void PathsToUpdateInDatabase_InDbButNotOnDisk()
	{
		var results = SyncFolder.PathsToUpdateInDatabase(
			[new FileIndexItem("/test.jpg")], []);

		Assert.HasCount(1, results);
		Assert.AreEqual("/test.jpg", results[0].FilePath);
	}

	[TestMethod]
	public void PathsToUpdateInDatabase_ExistBoth()
	{
		var results = SyncFolder.PathsToUpdateInDatabase(
			[new FileIndexItem("/test.jpg")], new List<string> { "/test.jpg" });

		Assert.HasCount(1, results);
		Assert.AreEqual("/test.jpg", results[0].FilePath);
	}

	[TestMethod]
	public void PathsToUpdateInDatabase_Duplicates()
	{
		var results = SyncFolder.PathsToUpdateInDatabase(
			[], new List<string> { "/test.jpg", "/test.jpg" });

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
			new FakeIStorage(["/", "/DuplicateFolder"]);
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
		Assert.ContainsSingle(p => p.FilePath == "/DuplicateFolder", allFolders);
	}

	[TestMethod]
	public async Task Folder_DuplicateFolders_Direct()
	{
		await _query.AddItemAsync(new FileIndexItem("/DuplicateFolder") { IsDirectory = true });
		// yes this is duplicate
		await _query.AddItemAsync(new FileIndexItem("/DuplicateFolder") { IsDirectory = true });

		var storage =
			new FakeIStorage(["/", "/DuplicateFolder"]);
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(), new FakeMemoryCache(), null);

		await syncFolder.Folder("/DuplicateFolder");

		var allFolders = ( _query as FakeIQuery )?.GetAllFolders()
			.Where(p => p.FilePath == "/DuplicateFolder").ToList();
		Assert.IsNotNull(allFolders);

		Assert.AreEqual("/DuplicateFolder",
			allFolders.Find(p => p.FilePath == "/DuplicateFolder")?.FilePath);
		Assert.ContainsSingle(p => p.FilePath == "/DuplicateFolder", allFolders);
	}

	[TestMethod]
	public async Task Folder_ShouldIgnore()
	{
		var storage = new FakeIStorage(
			["/", "/test_ignore", "/test_ignore/ignore"],
			["/test_ignore/ignore/test1.jpg"],
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
			SyncIgnore = ["/test_ignore/ignore"]
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
			new FakeIStorage(["/", "/2018", "/2018/02", "/2018/02/2018_02_01"]);
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		await syncFolder.CompareFolderListAndFixMissingFolders(
			["/", "/2018", "/2018/02", "/2018/02/2018_02_01"],
			[new FileIndexItem("/2018")]);

		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018"));
		Assert.AreEqual("/2018/02",
			( await _query.GetObjectByFilePathAsync("/2018/02") )?.FilePath);
		Assert.AreEqual("/2018/02/2018_02_01",
			( await _query.GetObjectByFilePathAsync("/2018/02/2018_02_01") )?.FilePath);
	}

	[TestMethod]
	public async Task CompareFolderListAndFixMissingFoldersTest_Ignored()
	{
		var storage = new FakeIStorage(["/", "/.git", "/test"]);
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		await syncFolder.CompareFolderListAndFixMissingFolders(
			["/", "/.git"],
			[new FileIndexItem("/")]);

		Assert.IsNull(await _query.GetObjectByFilePathAsync("/.git"));
	}

	[TestMethod]
	public async Task CompareFolderListAndFixMissingFoldersTest_Ok_SameCount()
	{
		var storage =
			new FakeIStorage(["/", "/2018", "/2018/02", "/2018/02/2018_02_01"]);
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		await syncFolder.CompareFolderListAndFixMissingFolders(
			["/", "/2018", "/2018/02", "/2018/02/2018_02_01"],
			[
				new FileIndexItem("/"), new FileIndexItem("/2018"), new FileIndexItem("/2018/02"),
				new FileIndexItem("/2018/02/2018_02_01")
			]
		);

		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018"));
		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018/02"));
		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018/02/2018_02_01"));
	}

	[TestMethod]
	public async Task CompareFolderListAndFixMissingFoldersTest_NotFound()
	{
		var storage = new FakeIStorage(["/"]);
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		await syncFolder.CompareFolderListAndFixMissingFolders(
			["/", "/2018", "/2018/02", "/2018/02/2018_02_01"],
			[new FileIndexItem("/2018")]);

		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018"));
		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018/02"));
		Assert.IsNull(await _query.GetObjectByFilePathAsync("/2018/02/2018_02_01"));
	}

	[TestMethod]
	public void DisplayInlineConsole_default()
	{
		var consoleWrapper = new FakeConsoleWrapper();
		SyncFolder.DisplayInlineConsole(consoleWrapper,
			[new FileIndexItem("/test.jpg")]);
		Assert.AreEqual("⁑", consoleWrapper.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public void DisplayInlineConsole_DeletedAndSame()
	{
		var consoleWrapper = new FakeConsoleWrapper();
		SyncFolder.DisplayInlineConsole(consoleWrapper,
			[new FileIndexItem("/test.jpg") { Status = FileIndexItem.ExifStatus.DeletedAndSame }]);
		Assert.AreEqual("✘", consoleWrapper.WrittenLines.LastOrDefault());
	}

	[TestMethod]
	public void DisplayInlineConsole_Deleted()
	{
		var consoleWrapper = new FakeConsoleWrapper();
		SyncFolder.DisplayInlineConsole(consoleWrapper,
			[new FileIndexItem("/test.jpg") { Status = FileIndexItem.ExifStatus.Deleted }]);
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
			["/", "/race_condition_folder"],
			["/race_condition_folder/test.jpg"],
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
		var storage = new FakeIStorage(["/"], [],
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
			["/", "/photos", "/photos/2024", "/photos/2024/01"],
			["/photos/2024/01/photo1.jpg", "/photos/2024/01/photo2.jpg"],
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
		await syncFolder.Folder("/photos");

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

		var storage = new FakeIStorage(["/"], [],
			new List<byte[]>());

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var rootItem = await _query.GetObjectByFilePathAsync("/folder_many_items");
		Assert.IsNotNull(rootItem);

		// Act: Remove child items
		await syncFolder.RemoveChildItems(_query, rootItem);

		// Assert: Should log the count of items being removed
		Assert.Contains(log =>
			log.Item2!.Contains("[SyncFolder] Removing 10 child items"), logger.TrackedInformation);
	}

	[TestMethod]
	public async Task Folder_EmptyFolderInDB_NotOnDisk_ShouldBeRemoved()
	{
		// Setup: Empty folder in DB, not on disk - should be cleaned up
		await _query.AddItemAsync(
			new FileIndexItem("/truly_empty_folder") { IsDirectory = true });

		var storage = new FakeIStorage(["/"], [],
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
		await _query.AddItemAsync(new FileIndexItem("/active_sync_folder") { IsDirectory = true });

		var storage = new FakeIStorage(
			["/", "/active_sync_folder"],
			["/active_sync_folder/new_photo.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Simulate race: Add child to DB while sync is running
		await _query.AddItemAsync(new FileIndexItem("/active_sync_folder/new_photo.jpg"));

		// Act: Sync should detect the child and not delete folder
		await syncFolder.Folder("/");

		// Assert: Folder and child should still exist
		var folder = await _query.GetObjectByFilePathAsync("/active_sync_folder");
		var child = await _query.GetObjectByFilePathAsync("/active_sync_folder/new_photo.jpg");

		Assert.IsNotNull(folder, "Folder should not be deleted");
		Assert.IsNotNull(child, "Child item should not be deleted");
	}

	[TestMethod]
	public async Task CheckIfFolderExistOnDisk_WithSubdirectoriesOnDisk_ShouldSkipDeletion()
	{
		// Setup: Folder in DB but not on disk, BUT has subdirectories/files on disk
		// This simulates a race condition where folder structure is being created
		await _query.AddItemAsync(
			new FileIndexItem("/folder_with_subdirs") { IsDirectory = true });

		// Storage: Parent folder exists with subdirectories (simulating active folder creation)
		var storage = new FakeIStorage(
			["/", "/folder_with_subdirs/subfolder"],
			["/folder_with_subdirs/subfolder/file.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act: Run folder sync
		await syncFolder.Folder("/");

		// Assert: Should log the skip message
		Assert.Contains(log =>
				log.Item2!.Contains(
					"[SyncFolder] Skipping deletion of /folder_with_subdirs - subdirectories exist on disk"),
			logger.TrackedInformation,
			"Expected log message about skipping deletion due to subdirectories");

		// Assert: Folder should still exist in database (not deleted)
		var folder = await _query.GetObjectByFilePathAsync("/folder_with_subdirs");
		Assert.IsNotNull(folder, "Folder should not be deleted when subdirectories exist on disk");
	}

	/// <summary>
	///     Edge Case: Multiple nested subdirectories on disk when folder not in DB
	/// </summary>
	[TestMethod]
	public async Task CheckIfFolderExistOnDisk_WithDeeplyNestedSubdirectories_ShouldSkipDeletion()
	{
		// Setup: Folder in DB but not on disk, with deeply nested subdirectories
		await _query.AddItemAsync(
			new FileIndexItem("/deep_nested_folder") { IsDirectory = true });

		// Storage: Has deeply nested structure
		var storage = new FakeIStorage(
			[
				"/",
				"/deep_nested_folder/level1",
				"/deep_nested_folder/level1/level2",
				"/deep_nested_folder/level1/level2/level3"
			],
			["/deep_nested_folder/level1/level2/level3/file.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act: Run folder sync
		await syncFolder.Folder("/");

		// Assert: Folder should NOT be deleted
		var folder = await _query.GetObjectByFilePathAsync("/deep_nested_folder");
		Assert.IsNotNull(folder, "Deeply nested folder should not be deleted");
		Assert.Contains(log =>
				log.Item2!.Contains("Skipping deletion of /deep_nested_folder"),
			logger.TrackedInformation,
			"Should log skip message for deeply nested folder");
	}

	/// <summary>
	///     Edge Case: Multiple sibling subdirectories on disk
	/// </summary>
	[TestMethod]
	public async Task
		CheckIfFolderExistOnDisk_WithMultipleSiblingSubdirectories_ShouldSkipDeletion()
	{
		// Setup: Folder with multiple sibling subdirectories
		await _query.AddItemAsync(
			new FileIndexItem("/multi_sibling_folder") { IsDirectory = true });

		var storage = new FakeIStorage(
			[
				"/",
				"/multi_sibling_folder/sub1",
				"/multi_sibling_folder/sub2",
				"/multi_sibling_folder/sub3",
				"/multi_sibling_folder/sub4"
			],
			[
				"/multi_sibling_folder/sub1/file1.jpg",
				"/multi_sibling_folder/sub2/file2.jpg",
				"/multi_sibling_folder/sub3/file3.jpg",
				"/multi_sibling_folder/sub4/file4.jpg"
			],
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(),
				CreateAnImage.Bytes.ToArray(),
				CreateAnImage.Bytes.ToArray(),
				CreateAnImage.Bytes.ToArray()
			});

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act
		await syncFolder.Folder("/");

		// Assert
		var folder = await _query.GetObjectByFilePathAsync("/multi_sibling_folder");
		Assert.IsNotNull(folder, "Folder with multiple siblings should not be deleted");
	}

	/// <summary>
	///     Edge Case: RemoveChildItems with zero children (empty folder)
	/// </summary>
	[TestMethod]
	public async Task RemoveChildItems_WithZeroChildren_ShouldDeleteFolderItself()
	{
		// Setup: Empty folder in DB, not on disk
		await _query.AddItemAsync(
			new FileIndexItem("/truly_empty_orphan_folder") { IsDirectory = true });

		var storage = new FakeIStorage(["/"], [],
			new List<byte[]>());

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var rootItem = await _query.GetObjectByFilePathAsync("/truly_empty_orphan_folder");
		Assert.IsNotNull(rootItem);

		var queryFactory = new QueryFactory(new SetupDatabaseTypes(_appSettings),
			_query, new FakeMemoryCache(), _appSettings, null, new FakeIWebLogger());
		var query = queryFactory.Query()!;

		// Act
		var result = await syncFolder.RemoveChildItems(query, rootItem);

		// Assert
		Assert.AreEqual("/truly_empty_orphan_folder", result.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result.Status);
		Assert.IsNull(await _query.GetObjectByFilePathAsync("/truly_empty_orphan_folder"));
	}

	/// <summary>
	///     Edge Case: RemoveChildItems called with very large number of children
	/// </summary>
	[TestMethod]
	public async Task RemoveChildItems_WithManyChildren_ShouldRemoveAll()
	{
		// Setup: Folder with many children
		const int childCount = 100;
		await _query.AddItemAsync(
			new FileIndexItem("/folder_with_many_children") { IsDirectory = true });

		for ( var i = 0; i < childCount; i++ )
		{
			await _query.AddItemAsync(
				new FileIndexItem($"/folder_with_many_children/file{i:D3}.jpg"));
		}

		var storage = new FakeIStorage(["/"], [],
			new List<byte[]>());

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var rootItem = await _query.GetObjectByFilePathAsync("/folder_with_many_children");
		Assert.IsNotNull(rootItem);

		var queryFactory = new QueryFactory(new SetupDatabaseTypes(_appSettings),
			_query, new FakeMemoryCache(), _appSettings, null, logger);
		var query = queryFactory.Query()!;

		// Act
		var result = await syncFolder.RemoveChildItems(query, rootItem);

		// Assert
		Assert.AreEqual("/folder_with_many_children", result.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result.Status);

		// Verify all children are removed
		var remainingChildren = await _query.GetAllRecursiveAsync("/folder_with_many_children");
		Assert.IsEmpty(remainingChildren);

		// Verify logging includes count
		Assert.Contains(log =>
			log.Item2!.Contains($"Removing {childCount} child items"), logger.TrackedInformation);
	}

	/// <summary>
	///     Edge Case: RemoveChildItems with subdirectories as children
	/// </summary>
	[TestMethod]
	public async Task RemoveChildItems_WithSubdirectoriesAsChildren_ShouldRemoveAllRecursive()
	{
		// Setup: Folder with subdirectories and files
		await _query.AddItemAsync(
			new FileIndexItem("/parent_with_subdirs") { IsDirectory = true });
		await _query.AddItemAsync(
			new FileIndexItem("/parent_with_subdirs/child_dir1") { IsDirectory = true });
		await _query.AddItemAsync(
			new FileIndexItem("/parent_with_subdirs/child_dir2") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/parent_with_subdirs/child_dir1/file1.jpg"));
		await _query.AddItemAsync(new FileIndexItem("/parent_with_subdirs/child_dir2/file2.jpg"));
		await _query.AddItemAsync(new FileIndexItem("/parent_with_subdirs/rootfile.jpg"));

		var storage = new FakeIStorage(["/"], [],
			new List<byte[]>());

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var rootItem = await _query.GetObjectByFilePathAsync("/parent_with_subdirs");
		Assert.IsNotNull(rootItem);

		var queryFactory = new QueryFactory(new SetupDatabaseTypes(_appSettings),
			_query, new FakeMemoryCache(), _appSettings, null, new FakeIWebLogger());
		var query = queryFactory.Query()!;

		// Act
		var result = await syncFolder.RemoveChildItems(query, rootItem);

		// Assert
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result.Status);

		// Verify entire tree is removed
		var remaining = await _query.GetAllRecursiveAsync("/parent_with_subdirs");
		Assert.IsEmpty(remaining);
	}

	/// <summary>
	///     Edge Case: Folder appears after sync started (race condition)
	/// </summary>
	[TestMethod]
	public async Task RemoveChildItems_FolderAppearsAfterSync_ShouldAbort()
	{
		// Setup: Folder not on disk initially, appears during sync
		await _query.AddItemAsync(
			new FileIndexItem("/race_appears_later") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/race_appears_later/orphan.jpg"));

		// Storage initially empty
		var storage = new FakeIStorage(["/"], [],
			new List<byte[]>());

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var rootItem = await _query.GetObjectByFilePathAsync("/race_appears_later");
		Assert.IsNotNull(rootItem);

		var queryFactory = new QueryFactory(new SetupDatabaseTypes(_appSettings),
			_query, new FakeMemoryCache(), _appSettings, null, new FakeIWebLogger());
		var query = queryFactory.Query()!;

		// Act: Simulate folder appearing
		storage.CreateDirectory("/race_appears_later");

		var result = await syncFolder.RemoveChildItems(query, rootItem);

		// Assert: Should abort deletion
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, result.Status);
		var children = await _query.GetAllRecursiveAsync("/race_appears_later");
		Assert.HasCount(1, children);
	}

	/// <summary>
	///     Edge Case: Multiple folders with subdirectories in parallel processing
	/// </summary>
	[TestMethod]
	public async Task Folder_MultipleFoldersWithSubdirectories_ParallelProcessing()
	{
		// Setup: Multiple folders with subdirectories
		await _query.AddItemAsync(new FileIndexItem("/parallel_folder1") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/parallel_folder2") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/parallel_folder3") { IsDirectory = true });

		var storage = new FakeIStorage(
			[
				"/",
				"/parallel_folder1/sub1",
				"/parallel_folder2/sub2",
				"/parallel_folder3/sub3"
			],
			[
				"/parallel_folder1/sub1/file.jpg",
				"/parallel_folder2/sub2/file.jpg",
				"/parallel_folder3/sub3/file.jpg"
			],
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(),
				CreateAnImage.Bytes.ToArray(),
				CreateAnImage.Bytes.ToArray()
			});

		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
			MaxDegreesOfParallelism = 3
		};

		var syncFolder = new SyncFolder(appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act
		await syncFolder.Folder("/");

		// Assert: All folders should be present
		foreach ( var folderName in new[]
		         {
			         "/parallel_folder1", "/parallel_folder2", "/parallel_folder3"
		         } )
		{
			var folder = await _query.GetObjectByFilePathAsync(folderName);
			Assert.IsNotNull(folder, $"{folderName} should exist");
		}
	}

	/// <summary>
	///     Edge Case: Folder with mix of files and subdirectories, some orphaned
	/// </summary>
	[TestMethod]
	public async Task Folder_MixedContentWithOrphans_ShouldCleanupCorrectly()
	{
		// Setup: Folder with mix of files and subdirectories
		await _query.AddItemAsync(new FileIndexItem("/mixed_folder") { IsDirectory = true });
		await _query.AddItemAsync(
			new FileIndexItem("/mixed_folder/subdir1") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/mixed_folder/subdir1/orphan.jpg"));
		await _query.AddItemAsync(new FileIndexItem("/mixed_folder/file_on_disk.jpg"));
		await _query.AddItemAsync(new FileIndexItem("/mixed_folder/orphan_file.jpg"));

		// Storage has only some files
		var storage = new FakeIStorage(
			["/", "/mixed_folder"],
			["/mixed_folder/file_on_disk.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act
		await syncFolder.Folder("/mixed_folder");

		// Assert: Files on disk should remain
		var fileOnDisk = await _query.GetObjectByFilePathAsync("/mixed_folder/file_on_disk.jpg");
		Assert.IsNotNull(fileOnDisk);

		// Orphan files should be marked as missing
		var orphanFile = await _query.GetObjectByFilePathAsync("/mixed_folder/orphan_file.jpg");
		Assert.IsNull(orphanFile, "Orphan file should be removed from database");
	}

	/// <summary>
	///     Edge Case: Subdirectory skip logic should work independently of folder ignore list
	/// </summary>
	[TestMethod]
	public async Task Folder_IgnoredFolderWithSubdirectories_ShouldNotBreakSubdirectoryLogic()
	{
		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
			SyncIgnore = ["/ignored_folder"]
		};

		// Add both ignored and normal folders
		await _query.AddItemAsync(new FileIndexItem("/ignored_folder") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/normal_folder") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/normal_folder/sub/file.jpg"));

		// Storage has subdirectories in both ignored and normal folders
		var storage = new FakeIStorage(
			["/", "/ignored_folder/sub", "/normal_folder/sub"],
			["/ignored_folder/sub/file1.jpg", "/normal_folder/sub/file.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act
		await syncFolder.Folder("/");

		// Assert: Normal folder with subdirectories should still exist
		var normalFolder = await _query.GetObjectByFilePathAsync("/normal_folder");
		Assert.IsNotNull(normalFolder, "Normal folder should exist");

		// The skip deletion log should work for normal folders
		Assert.Contains(log =>
			log.Item2!.Contains("Skipping deletion of /normal_folder"), logger.TrackedInformation);
	}

	/// <summary>
	///     Edge Case: Empty subdirectory (no files, just folder)
	/// </summary>
	[TestMethod]
	public async Task Folder_EmptySubdirectory_ShouldBePreserved()
	{
		await _query.AddItemAsync(
			new FileIndexItem("/parent_with_empty_sub") { IsDirectory = true });
		await _query.AddItemAsync(
			new FileIndexItem("/parent_with_empty_sub/empty_sub") { IsDirectory = true });

		// Storage has empty subdirectory
		var storage = new FakeIStorage(
			["/", "/parent_with_empty_sub", "/parent_with_empty_sub/empty_sub"],
			[],
			new List<byte[]>());

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act
		await syncFolder.Folder("/parent_with_empty_sub");

		// Assert: Empty subdirectory should still exist
		var emptySubDir = await _query.GetObjectByFilePathAsync("/parent_with_empty_sub/empty_sub");
		Assert.IsNotNull(emptySubDir);
	}

	/// <summary>
	///     Edge Case: GetDirectoryRecursive returns results but folder itself is missing
	/// </summary>
	[TestMethod]
	public async Task CheckIfFolderExistOnDisk_GetDirectoryRecursiveHasResults_FolderMissing()
	{
		await _query.AddItemAsync(
			new FileIndexItem("/folder_missing_but_subs_exist") { IsDirectory = true });

		// Special case: subdirectories exist but parent folder path doesn't exist as folder
		var storage = new FakeIStorage(
			["/", "/folder_missing_but_subs_exist/sub1/sub2"],
			["/folder_missing_but_subs_exist/sub1/sub2/file.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act
		await syncFolder.Folder("/");

		// Assert: Should skip deletion because subdirectories exist
		Assert.Contains(log =>
			log.Item2!.Contains("Skipping deletion"), logger.TrackedInformation);
	}

	/// <summary>
	///     Edge Case: Very long file paths in subdirectories
	/// </summary>
	[TestMethod]
	public async Task CheckIfFolderExistOnDisk_VeryLongPaths_ShouldSkipDeletion()
	{
		var longFolderName = "/folder_" + new string('a', 100);
		await _query.AddItemAsync(new FileIndexItem(longFolderName) { IsDirectory = true });

		var storage = new FakeIStorage(
			["/", $"{longFolderName}/sub"],
			[$"{longFolderName}/sub/file.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act
		await syncFolder.Folder("/");

		// Assert: Folder should not be deleted
		var folder = await _query.GetObjectByFilePathAsync(longFolderName);
		Assert.IsNotNull(folder);
	}

	/// <summary>
	///     Edge Case: Concurrent modifications with multiple sync operations
	/// </summary>
	[TestMethod]
	public async Task Folder_ConcurrentModificationsWithDifferentFolders_ShouldNotConflict()
	{
		// Setup: Multiple independent folder structures
		await _query.AddItemAsync(new FileIndexItem("/concurrent_folder1") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/concurrent_folder2") { IsDirectory = true });
		await _query.AddItemAsync(new FileIndexItem("/concurrent_folder1/file1.jpg"));
		await _query.AddItemAsync(new FileIndexItem("/concurrent_folder2/file2.jpg"));

		var storage = new FakeIStorage(
			["/", "/concurrent_folder1", "/concurrent_folder2"],
			["/concurrent_folder1/file1.jpg", "/concurrent_folder2/file2.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });

		var appSettings = new AppSettings
		{
			DatabaseType = AppSettings.DatabaseTypeList.InMemoryDatabase,
			MaxDegreesOfParallelism = 2
		};

		var syncFolder = new SyncFolder(appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act: Sync both folders concurrently
		await syncFolder.Folder("/");

		// Assert: Both folders and files should be present
		var folder1 = await _query.GetObjectByFilePathAsync("/concurrent_folder1");
		var folder2 = await _query.GetObjectByFilePathAsync("/concurrent_folder2");
		var file1 = await _query.GetObjectByFilePathAsync("/concurrent_folder1/file1.jpg");
		var file2 = await _query.GetObjectByFilePathAsync("/concurrent_folder2/file2.jpg");

		Assert.IsNotNull(folder1);
		Assert.IsNotNull(folder2);
		Assert.IsNotNull(file1);
		Assert.IsNotNull(file2);
	}

	/// <summary>
	///     Edge Case: RemoveChildItems with mixed statuses in children
	/// </summary>
	[TestMethod]
	public async Task RemoveChildItems_WithChildrenHavingDifferentStatuses_ShouldRemoveAll()
	{
		await _query.AddItemAsync(
			new FileIndexItem("/mixed_status_folder") { IsDirectory = true });

		var child1 = new FileIndexItem("/mixed_status_folder/child1.jpg")
		{
			Status = FileIndexItem.ExifStatus.Ok
		};
		var child2 = new FileIndexItem("/mixed_status_folder/child2.jpg")
		{
			Status = FileIndexItem.ExifStatus.OkAndSame
		};
		var child3 = new FileIndexItem("/mixed_status_folder/child3.jpg")
		{
			Status = FileIndexItem.ExifStatus.Deleted
		};

		await _query.AddItemAsync(child1);
		await _query.AddItemAsync(child2);
		await _query.AddItemAsync(child3);

		var storage = new FakeIStorage(["/"], [],
			new List<byte[]>());

		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var rootItem = await _query.GetObjectByFilePathAsync("/mixed_status_folder");
		Assert.IsNotNull(rootItem);

		var queryFactory = new QueryFactory(new SetupDatabaseTypes(_appSettings),
			_query, new FakeMemoryCache(), _appSettings, null, new FakeIWebLogger());
		var query = queryFactory.Query()!;

		// Act
		var result = await syncFolder.RemoveChildItems(query, rootItem);

		// Assert: All children removed regardless of status
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, result.Status);
		var remaining = await _query.GetAllRecursiveAsync("/mixed_status_folder");
		Assert.IsEmpty(remaining);
	}

	/// <summary>
	///     BUG FIX: Folder with only files (no subdirectories) should not be marked for deletion
	///     This tests the race condition where CheckIfFolderExistOnDisk runs before files
	///     are indexed, causing the folder to appear empty and get deleted incorrectly.
	///     
	///     Issue: Previously only checked GetDirectoryRecursive(), which returns subdirectories
	///     not files. During parallel sync, a folder with only files appeared empty.
	/// </summary>
	[TestMethod]
	public async Task CheckIfFolderExistOnDisk()
	{
		var rootItem = await _query.AddItemAsync(
			new FileIndexItem("/vacation") { IsDirectory = true });
		await _query.AddItemAsync(
			new FileIndexItem("/vacation/day1") { IsDirectory = true });

		var storage = new FakeIStorage(
			["/", "/vacation"], // Only directories, no subdirs of day1
			["/vacation/day1/photo1.jpg", "/vacation/day1/photo2.jpg"], // But files exist!
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });

		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		var items = ( await _query.GetAllRecursiveAsync("/vacation") )
			.Where(p => p.IsDirectory == true).ToList();
		items.Add(rootItem);

		await syncFolder.CheckIfFolderExistOnDisk(items);
	}

	/// <summary>
	/// Test: Folder missing on disk, no subdirectories, no files => should be deleted
	/// </summary>
	[TestMethod]
	public async Task CheckIfFolderExistOnDisk_EmptyMissingFolder_ShouldBeDeleted()
	{
		// Arrange: Add folder to DB, but not on disk
		await _query.AddItemAsync(
			new FileIndexItem("/empty_missing_folder") { IsDirectory = true });

		// Storage: Only root exists, no subdirs or files in /empty_missing_folder
		var storage = new FakeIStorage(["/"], [], new List<byte[]>());
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), new FakeIWebLogger(),
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act: Run folder sync
		await syncFolder.Folder("/");

		// Assert: Folder should be deleted from DB
		var folder = await _query.GetObjectByFilePathAsync("/empty_missing_folder");
		Assert.IsNull(folder, "Empty missing folder should be deleted");
	}

	/// <summary>
	/// Test: Folder missing on disk, but has subdirectories => should skip deletion and log reason
	/// </summary>
	[TestMethod]
	public async Task
		CheckIfFolderExistOnDisk_MissingFolderWithSubdirectories_ShouldSkipDeletionAndLog()
	{
		await _query.AddItemAsync(
			new FileIndexItem("/missing_with_subdirs") { IsDirectory = true });

		// Storage: Only root and subdirectory exist, not the folder itself
		var storage =
			new FakeIStorage(["/", "/missing_with_subdirs/subdir"], [], new List<byte[]>());
		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		await syncFolder.Folder("/");

		// Assert: Folder should NOT be deleted
		var folder = await _query.GetObjectByFilePathAsync("/missing_with_subdirs");
		Assert.IsNotNull(folder, "Folder with subdirectories should not be deleted");
		// Assert: Log contains correct reason
		Assert.Contains(log =>
				log.Item2!.Contains(
					"[SyncFolder] Skipping deletion of /missing_with_subdirs - subdirectories exist on disk"),
			logger.TrackedInformation,
			"Expected log message about skipping deletion due to subdirectories");
	}

	/// <summary>
	/// Test: Folder missing on disk, but has files => should skip deletion and log reason
	/// </summary>
	[TestMethod]
	public async Task CheckIfFolderExistOnDisk_MissingFolderWithFiles_ShouldSkipDeletionAndLog()
	{
		// Add folder to DB, but do NOT add it to storage folders
		await _query.AddItemAsync(new FileIndexItem("/missing_with_files") { IsDirectory = true });

		// Storage: Only root exists, but file is present in the folder (folder itself is missing)
		var storage = new FakeIStorage(
			["/"], // Only root exists
			["/missing_with_files/file.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var logger = new FakeIWebLogger();
		var syncFolder = new SyncFolder(_appSettings, _query, new FakeSelectorStorage(storage),
			new ConsoleWrapper(), logger,
			new FakeMemoryCache(new Dictionary<string, object>()), null);

		// Act: Run folder sync
		await syncFolder.Folder("/");

		// Assert: Should log the skip message for files
		Assert.Contains(log =>
				log.Item2 != null && log.Item2.Contains(
					"[SyncFolder] Skipping deletion of /missing_with_files - files exist on disk"),
			logger.TrackedInformation,
			"Expected log message about skipping deletion due to files");

		// Assert: Folder should still exist in database (not deleted)
		var folder = await _query.GetObjectByFilePathAsync("/missing_with_files");
		Assert.IsNotNull(folder, "Folder should not be deleted when files exist on disk");
	}
}

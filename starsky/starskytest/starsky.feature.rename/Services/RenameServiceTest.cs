using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.rename.Models;
using starsky.feature.rename.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.rename.Services;

[TestClass]
public sealed class RenameServiceTest
{
	private readonly StorageSubPathFilesystem _iStorageSubPath;
	private readonly CreateAnImage _newImage;
	private readonly Query _query;
	private FileIndexItem _fileInExist = new();
	private FileIndexItem _fileInRoot = new();
	private FileIndexItem _folder1Exist = new();

	private FileIndexItem _folderExist = new();
	private FileIndexItem _parentFolder = new();

	public RenameServiceTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		var memoryCache = provider.GetService<IMemoryCache>();

		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase(nameof(RenameServiceTest));
		var options = builder.Options;
		var context = new ApplicationDbContext(options);

		_newImage = new CreateAnImage();

		var appSettings = new AppSettings
		{
			StorageFolder = PathHelper.AddBackslash(_newImage.BasePath),
			ThumbnailTempFolder = _newImage.BasePath
		};
		_query = new Query(context, appSettings, null,
			new FakeIWebLogger(), memoryCache);

		if ( _query.GetAllFilesAsync("/").Result.TrueForAll(p => p.FileName != _newImage.FileName) )
		{
			context.FileIndex.Add(new FileIndexItem
			{
				FileName = _newImage.FileName,
				ParentDirectory = "/",
				AddToDatabase = DateTime.UtcNow
			});
			context.SaveChanges();
		}

		_iStorageSubPath = new StorageSubPathFilesystem(appSettings, new FakeIWebLogger());
	}

	[TestMethod]
	public async Task RenameFsTest_DuplicateFile()
	{
		const string fileAlreadyExistSubPath = "/already_8758.txt";
		_iStorageSubPath.ExistFile(fileAlreadyExistSubPath);

		if ( !_iStorageSubPath.ExistFile(fileAlreadyExistSubPath) )
		{
			await _iStorageSubPath.WriteStreamAsync(
				StringToStreamHelper.StringToStream("test-content-rename-fs"),
				fileAlreadyExistSubPath);
		}

		var renameFs =
			await new RenameService(_query, _iStorageSubPath, new FakeIWebLogger()).Rename(
				_newImage.DbPath,
				fileAlreadyExistSubPath);

		var result = await StreamToStringHelper.StreamToStringAsync(
			_iStorageSubPath.ReadStream(fileAlreadyExistSubPath));

		// it should not overwrite the target file
		Assert.Contains("test", result);
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,
			renameFs.FirstOrDefault()?.Status);

		// and there are no more files with a name that looks like already_8758
		// should only have one file with filename: already_8758
		var count = _iStorageSubPath.GetAllFilesInDirectory("/")
			.Count(p => p.StartsWith(fileAlreadyExistSubPath));
		Assert.AreEqual(1, count);

		// and remove the file afterward
		_iStorageSubPath.FileDelete(fileAlreadyExistSubPath);
	}

	[TestMethod]
	public async Task RenameFsTest_MoveFileWithoutAnyItems()
	{
		var renameFs =
			await new RenameService(_query, _iStorageSubPath, new FakeIWebLogger()).Rename(
				"/non-exist.jpg",
				"/non-exist2.jpg");
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex,
			renameFs.FirstOrDefault()?.Status);
	}

	private async Task CreateFoldersAndFilesInDatabase()
	{
		_folderExist = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "exist",
			ParentDirectory = "/",
			AddToDatabase = DateTime.UtcNow,
			FileHash = "34567898765434567487984785487",
			IsDirectory = true
		});

		_fileInExist = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "file.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			AddToDatabase = new DateTime(2022, 5, 6, 0, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2022, 5, 6, 0, 0, 0, DateTimeKind.Utc)
		});

		_fileInRoot = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "root-file.jpg",
			ParentDirectory = "/",
			IsDirectory = false,
			AddToDatabase = new DateTime(2022, 5, 6, 0, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2022, 5, 6, 0, 0, 0, DateTimeKind.Utc)
		});

		_folder1Exist = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "folder1",
			ParentDirectory = "/",
			IsDirectory = true,
			FileHash = "3497867df894587"
		});

		_parentFolder = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "/", ParentDirectory = "/", IsDirectory = true
		});
	}

	private async Task RemoveFoldersAndFilesInDatabase()
	{
		Assert.IsNotNull(_folderExist.FilePath);
		Assert.IsNotNull(_folder1Exist.FilePath);
		Assert.IsNotNull(_fileInExist.FilePath);
		Assert.IsNotNull(_parentFolder.FilePath);
		Assert.IsNotNull(_fileInRoot.FilePath);

		await _query.RemoveItemAsync(_folderExist);
		await _query.RemoveItemAsync(_folder1Exist);
		await _query.RemoveItemAsync(_fileInExist);
		await _query.RemoveItemAsync(_parentFolder);
		await _query.RemoveItemAsync(_fileInRoot);
	}

	[TestMethod]
	public async Task RenameFsTest_FakeIStorage_RenameOneFile()
	{
		// RenameFsTest_MoveFileToSameFolder_Items

		await CreateFoldersAndFilesInDatabase();
		Assert.IsNotNull(_folderExist.FilePath);
		Assert.IsNotNull(_folder1Exist.FilePath);
		Assert.IsNotNull(_fileInExist.FilePath);
		Assert.IsNotNull(_parentFolder.FilePath);

		var iStorage = new FakeIStorage(new List<string> { _folderExist.FilePath! },
			new List<string> { _fileInExist.FilePath! });

		var renameFs1 = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename(_fileInExist.FilePath!, _folderExist.FilePath + "/test2.jpg");
		var renameFs = renameFs1
			.Where(p => p.Status != FileIndexItem.ExifStatus.NotFoundSourceMissing).ToList();

		// query database
		var all = await _query.GetAllRecursiveAsync();
		Assert.AreEqual("test2.jpg", all.Find(p => p.FileName == "test2.jpg")?.FileName);

		// old item is not in db
		Assert.IsNull(all.Find(p => p.FileName == "test.jpg")?.FileName);

		// use cached view
		var singleItem = _query.SingleItem(_folderExist.FilePath + "/test2.jpg");
		Assert.AreEqual("test2.jpg", singleItem?.FileIndexItem?.FileName);

		Assert.HasCount(1, renameFs);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task RenameFsTest_RenameOneFile_JsonSidecarFile()
	{
		await CreateFoldersAndFilesInDatabase();

		var iStorage = new FakeIStorage(new List<string> { _folderExist.FilePath! },
			new List<string>
			{
				_fileInExist.FilePath!, JsonSidecarLocation.JsonLocation(_fileInExist.FilePath!)
			});

		var renameFs = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename(_fileInExist.FilePath!, _folderExist.FilePath + "/test2.jpg");

		// check if sidecar json are moved (on fake Filesystem)
		var values = iStorage.GetAllFilesInDirectoryRecursive("/").ToList();
		Assert.AreEqual("/exist/.starsky.test2.jpg.json",
			values.Find(p => p == "/exist/.starsky.test2.jpg.json"));
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok,
			renameFs.Find(p => p.FilePath == "/exist/test2.jpg")?.Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
			renameFs.Find(p => p.FilePath == "/exist/file.jpg")?.Status);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task RenameFsTest_FakeIStorage_RenameOneFile_ToWrongNewFileName()
	{
		await CreateFoldersAndFilesInDatabase();

		var iStorage = new FakeIStorage(new List<string> { _folderExist.FilePath! },
			new List<string> { _fileInExist.FilePath! });

		var renameFs = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename(_fileInExist.FilePath!, _folderExist.FilePath + "/test2___");

		// so this operation is not supported

		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,
			renameFs.FirstOrDefault()?.Status);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task RenameFsTest_FakeIStorage_FileToNonExistFolder_Items()
	{
		await CreateFoldersAndFilesInDatabase();

		var initFolderList = new List<string> { "/" };
		var initFileList = new List<string> { _fileInExist.FilePath! };
		var iStorage = new FakeIStorage(initFolderList, initFileList);
		var renameFs1 = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename(initFileList.FirstOrDefault()!, "/nonExist/test5.jpg");
		var renameFs = renameFs1.Where(p =>
				p.Status != FileIndexItem.ExifStatus.Deleted)
			.ToList();

		var all2 = await _query.GetAllRecursiveAsync();
		var selectFile3 = all2.Find(p => p.FileName == "test5.jpg");
		Assert.AreEqual("test5.jpg", selectFile3?.FileName);
		Assert.AreEqual("/nonExist", selectFile3?.ParentDirectory);

		// check if files are moved
		var values = iStorage.GetAllFilesInDirectory("/nonExist").ToList();
		Assert.AreEqual("/nonExist/test5.jpg", values.Find(p => p == "/nonExist/test5.jpg"));

		var initFileListFirst = renameFs.Find(p =>
			p.FilePath == initFileList.FirstOrDefault());
		Assert.AreEqual(initFileList.FirstOrDefault(), initFileListFirst!.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
			initFileListFirst.Status);

		var nonExistTest5 = renameFs.Find(p =>
			p.FilePath == "/nonExist/test5.jpg");
		Assert.AreEqual("/nonExist/test5.jpg", nonExistTest5?.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, nonExistTest5?.Status);

		var nonExist = renameFs.Find(p =>
			p.FilePath == "/nonExist");
		Assert.AreEqual("/nonExist", nonExist?.FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, nonExist?.Status);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task RenameFsTest_FakeIStorage_File_To_ExistFolder_MoveToTheSamePath()
	{
		await CreateFoldersAndFilesInDatabase();

		var initFolderList = new List<string> { "/", "/exist" };
		var initFileList = new List<string> { _fileInExist.FilePath! };
		var iStorage = new FakeIStorage(initFolderList, initFileList);
		var renameFs = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename(initFileList.FirstOrDefault()!, "/exist/");
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,
			renameFs.FirstOrDefault()?.Status);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task
		RenameFsTest_FakeIStorage_File_To_ExistFolder() // there is a separate sidecar json test
	{
		await CreateFoldersAndFilesInDatabase();

		var initFolderList = new List<string> { "/", "/test" };
		var initFileList = new List<string> { _fileInExist.FilePath! };
		var fakeIStorage = new FakeIStorage(initFolderList, initFileList);

		var renameFsResult =
			await new RenameService(_query, fakeIStorage, new FakeIWebLogger()).Rename(
				initFileList.FirstOrDefault()!,
				"/test/");

		var oldItem = await _query.GetObjectByFilePathAsync("/exist/file.jpg");
		Assert.IsNull(oldItem);

		// to file: (in database)
		var all2 = ( await _query.GetAllRecursiveAsync() )
			.Where(p => p.ParentDirectory?.Contains("/test") == true);
		var selectFile3 = all2.FirstOrDefault(p => p.FilePath == "/test/file.jpg");
		Assert.AreEqual("file.jpg", selectFile3?.FileName);
		Assert.AreEqual("/test", selectFile3?.ParentDirectory);

		// check if files are moved (on fake Filesystem)
		var values = fakeIStorage.GetAllFilesInDirectory("/test").ToList();
		Assert.AreEqual("/test/file.jpg", values.Find(p => p == "/test/file.jpg"));

		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
			renameFsResult.FirstOrDefault()?.Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFsResult[1].Status);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task RenameFsTest_FakeIStorage_File_To_ExistFolder_Json_SidecarFile()
	{
		await CreateFoldersAndFilesInDatabase();

		var initFolderList = new List<string> { "/", "/test" };
		var initFileList = new List<string>
		{
			_fileInExist.FilePath!, JsonSidecarLocation.JsonLocation(_fileInExist.FilePath!)
		};

		var iStorage = new FakeIStorage(initFolderList, initFileList);

		// the input is still  FileName = "file.jpg", ParentDirectory = "/exist",
		var renameFs = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename(initFileList.FirstOrDefault()!, "/test/");

		// to file: (in database)
		var all2 = await _query.GetAllRecursiveAsync();
		var selectFile3 = all2.Find(p => p.FileName == "file.jpg");
		Assert.AreEqual("file.jpg", selectFile3?.FileName);
		Assert.AreEqual("/test", selectFile3?.ParentDirectory);

		// check if sidecar Json are moved (on fake Filesystem)
		var values = iStorage.GetAllFilesInDirectoryRecursive("/test").ToList();

		Assert.AreEqual("/test/.starsky.file.jpg.json",
			values.Find(p => p == "/test/.starsky.file.jpg.json"));
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing,
			renameFs.FirstOrDefault()?.Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFs[1].Status);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task RenameFsTest_FakeIStorage_mergeTwoFolders()
	{
		await CreateFoldersAndFilesInDatabase();

		var existSubFolder = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "subfolder",
			ParentDirectory = _folder1Exist.FilePath,
			IsDirectory = true,
			FileHash = "InjectedAsExistSubFolder"
		});

		var existSubFolderChildJpg = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "child.jpg",
			ParentDirectory = _folder1Exist.FilePath + "/subfolder",
			FileHash = "InjectedAsExistSubFolderChildJpg"
		});


		var initFolderList = new List<string>
		{
			"/",
			_folderExist.FilePath + "/subfolder",
			_folder1Exist.FilePath!,
			_folderExist.FilePath!
		};

		var initFileList = new List<string>
		{
			_fileInExist.FilePath!,
			_folder1Exist.FilePath + "/subfolder/child.jpg",
			_folder1Exist.FilePath + "/subfolder/not_synced_item.jpg"
		};
		var iStorage = new FakeIStorage(initFolderList, initFileList);

		// the call
		var renameFs = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename("/exist", "/folder1");

		// First check if fakeDisk is changed
		var folder1Files = iStorage.GetAllFilesInDirectory("/folder1").ToList();
		var folder1Dir = iStorage.GetDirectoryRecursive("/folder1").Select(p => p.Key).ToList();

		Assert.AreEqual("/folder1/file.jpg", folder1Files[0]);
		Assert.AreEqual("/folder1/subfolder", folder1Dir[0]);

		var existDirContent =
			iStorage.GetDirectoryRecursive("/exist").Select(p => p.Key).ToList();
		var existFolder = iStorage.GetAllFilesInDirectory("/exist").ToList();

		Assert.IsEmpty(existDirContent);
		Assert.IsEmpty(existFolder);

		// Now check if FakeDb is changed
		var all2 = await _query.GetAllRecursiveAsync();

		Assert.AreEqual("/folder1/file.jpg",
			all2.Find(p =>
				p.FileName == "file.jpg" &&
				p.Status != FileIndexItem.ExifStatus.NotFoundSourceMissing)?.FilePath);
		Assert.AreEqual("/folder1/subfolder",
			all2.Find(p =>
				p.FileName == "subfolder" &&
				p.Status != FileIndexItem.ExifStatus.NotFoundSourceMissing)?.FilePath);
		Assert.AreEqual("/folder1/subfolder/child.jpg",
			all2.Find(p =>
				p.FileName == "child.jpg" &&
				p.Status != FileIndexItem.ExifStatus.NotFoundSourceMissing)?.FilePath);

		// FileIndexItem.ExifStatus.Ok, /folder1/file.jpg -			
		// FileIndexItem.ExifStatus.Ok, /folder1
		// NotFoundSourceMissing /exist

		var file = renameFs
			.Find(p => p.FilePath == "/folder1/file.jpg");
		var folder1 = renameFs
			.Find(p => p.FilePath == "/folder1");
		var exist = renameFs
			.Find(p => p.FilePath == "/exist");

		Assert.AreEqual("/folder1/file.jpg", file?.FilePath);
		Assert.AreEqual("/folder1", folder1?.FilePath);
		Assert.AreEqual("/exist", exist?.FilePath);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, file?.Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, folder1?.Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, exist?.Status);

		await _query.RemoveItemAsync(existSubFolder);
		await _query.RemoveItemAsync(existSubFolderChildJpg);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task RenameFsTest_TheSameInput()
	{
		var initFolderList = new List<string>();
		var initFileList = new List<string>();
		var fakeIStorage = new FakeIStorage(initFolderList, initFileList);
		var renameFs =
			await new RenameService(_query, fakeIStorage, new FakeIWebLogger()).Rename("/same",
				"/same");
		Assert.HasCount(1, renameFs);
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,
			renameFs.FirstOrDefault()?.Status);
	}

	[TestMethod]
	public void RenameFsTest_FakeIStorage_UnderstandTest()
	{
		// used to test the GetAllFilesInDirectory() fake class
		var initFolderList =
			new List<string> { "/", "/test/subfolder", "/test", "/otherfolder" };
		var initFileList = new List<string>
		{
			"/test/test.jpg", "/test/subfolder/t.jpg", "/test/subfolder/child.jpg"
		};
		var iStorage = new FakeIStorage(initFolderList,
			initFileList).GetAllFilesInDirectory("/test").ToList();
		Assert.HasCount(1, iStorage);
	}

	[TestMethod]
	public async Task RenameFsTest_MoveAFolderIntoAFile()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage();
		var renameFs =
			await new RenameService(_query, iStorage, new FakeIWebLogger()).Rename(
				_folderExist.FilePath!,
				_fileInExist.FilePath!);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, renameFs[0].Status);
	}

	[TestMethod]
	public async Task Rename_MoveFileToRootFolder()
	{
		var itemInChildFolderPath = "/child_folder/test_01.jpg";
		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath));
		await _query.AddParentItemsAsync(itemInChildFolderPath);
		var iStorage = new FakeIStorage(new List<string> { "/", "/child_folder" },
			new List<string> { "/child_folder/test_01.jpg" });

		var renameFs =
			await new RenameService(_query, iStorage, new FakeIWebLogger()).Rename(
				itemInChildFolderPath, "/");

		// where its from
		Assert.AreEqual("/child_folder", renameFs.FirstOrDefault()?.ParentDirectory);
		Assert.AreEqual("/child_folder/test_01.jpg", renameFs.FirstOrDefault()?.FilePath);

		Assert.AreEqual("/", renameFs[1].ParentDirectory);
		Assert.AreEqual("/test_01.jpg", renameFs[1].FilePath);

		Assert.AreEqual("/test_01.jpg",
			_query.SingleItem("/test_01.jpg")?.FileIndexItem?.FilePath);
		Assert.IsNull(_query.SingleItem(itemInChildFolderPath));
	}

	[TestMethod]
	public async Task Rename_Move_FileToFolder_Collections()
	{
		var itemInChildFolderPath = "/child_folder/test_10.jpg";
		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath));
		await _query.AddItemAsync(new FileIndexItem("/child_folder/test_10.png"));
		await _query.AddParentItemsAsync(itemInChildFolderPath);

		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2" },
			new List<string> { "/child_folder/test_10.jpg", "/child_folder/test_10.png" });

		var renameFs = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename(itemInChildFolderPath, "/child_folder2");

		// the first one is the deleted item
		Assert.AreEqual("/child_folder2", renameFs[1].ParentDirectory);
		Assert.AreEqual("/child_folder2/test_10.jpg", renameFs[1].FilePath);

		Assert.AreEqual("/child_folder2/test_10.jpg",
			_query.SingleItem("/child_folder2/test_10.jpg")?.FileIndexItem?.FilePath);
		Assert.AreEqual("/child_folder2/test_10.png",
			_query.SingleItem("/child_folder2/test_10.png")?.FileIndexItem?.FilePath);

		Assert.IsNull(_query.SingleItem(itemInChildFolderPath));
		Assert.IsNull(_query.SingleItem("/child_folder/test_10.png"));
	}

	[TestMethod]
	public async Task Rename_Move_FileToDeleted_Collections()
	{
		var fromItemJpg = "/child_folder/test_21.jpg";
		var fromItemDng = "/child_folder/test_21.dng";
		var toItemJpg = "/child_folder/test_21_edit.jpg";
		var toItemDng = "/child_folder/test_21_edit.dng";

		await _query.AddItemAsync(new FileIndexItem(fromItemJpg));
		await _query.AddItemAsync(new FileIndexItem(fromItemDng));
		await _query.AddParentItemsAsync(fromItemDng);

		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2" },
			new List<string> { fromItemJpg, fromItemDng });

		// only say: fromItemJpg > toItemJpg
		var renameFs1 = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename(fromItemJpg, toItemJpg);
		var renameFs = renameFs1
			.Where(p => p.Status != FileIndexItem.ExifStatus.NotFoundSourceMissing).ToList();

		// it has moved the files
		Assert.IsFalse(iStorage.ExistFile(fromItemJpg));
		Assert.IsFalse(iStorage.ExistFile(fromItemDng));

		Assert.IsTrue(iStorage.ExistFile(toItemJpg));
		Assert.IsTrue(iStorage.ExistFile(toItemDng));

		var toItemJpgItem = renameFs
			.Find(p => p.FilePath == toItemJpg);
		var toItemDngItem = renameFs
			.Find(p => p.FilePath == toItemDng);

		Assert.AreEqual(toItemJpg, toItemJpgItem?.FilePath);
		Assert.AreEqual(toItemDng, toItemDngItem?.FilePath);

		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, toItemJpgItem?.Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, toItemDngItem?.Status);

		// // and the database is ok
		Assert.AreEqual(toItemJpg,
			_query.SingleItem(toItemJpg)?.FileIndexItem?.FilePath);
		Assert.AreEqual(toItemDng,
			_query.SingleItem(toItemDng)?.FileIndexItem?.FilePath);
	}

	[TestMethod]
	public async Task
		InputOutputSubPathsPreflight_FileToDeleted_SingleItem_WithCollectionsEnabled()
	{
		var itemInChildFolderPath1 = "/child_folder/test_22.jpg";
		var collectionItemPath1 = "/child_folder/test_22.dng";

		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath1));
		await _query.AddItemAsync(new FileIndexItem(collectionItemPath1));

		await _query.AddParentItemsAsync(itemInChildFolderPath1);
		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2" },
			new List<string> { itemInChildFolderPath1, collectionItemPath1 });

		var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) =
			new RenameService(_query, iStorage, new FakeIWebLogger())
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1}",
					"/child_folder2/test_22_edit.jpg", true);

		Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
		Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);

		Assert.AreEqual("/child_folder2/test_22_edit.jpg", toFileSubPaths[0]);
		Assert.AreEqual("/child_folder2/test_22_edit.dng", toFileSubPaths[1]);

		Assert.IsEmpty(fileIndexResultsList);

		// this does only preflight

		var item1 = _query.SingleItem(itemInChildFolderPath1)
			?.FileIndexItem;
		Assert.IsNotNull(item1);
		await _query.RemoveItemAsync(item1);

		var item2 = _query.SingleItem(collectionItemPath1)?.FileIndexItem;
		Assert.IsNotNull(item2);
		await _query.RemoveItemAsync(item2);
	}

	[TestMethod]
	public async Task
		InputOutputSubPathsPreflight_FileToDeleted_SingleItem_Change_FileName_And_Extension_WithCollections()
	{
		var itemInChildFolderPath1 = "/child_folder/test_23.jpg";
		var collectionItemPath1 = "/child_folder/test_23.dng";

		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath1));
		await _query.AddItemAsync(new FileIndexItem(collectionItemPath1));

		await _query.AddParentItemsAsync(itemInChildFolderPath1);
		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2" },
			new List<string> { itemInChildFolderPath1, collectionItemPath1 });

		var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) =
			new RenameService(_query, iStorage, new FakeIWebLogger())
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1}",
					// Change to .jpeg
					"/child_folder2/test_23_edit.jpeg", true);

		Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
		Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);

		Assert.AreEqual("/child_folder2/test_23_edit.jpeg", toFileSubPaths[0]);
		Assert.AreEqual("/child_folder2/test_23_edit.dng", toFileSubPaths[1]);

		Assert.IsEmpty(fileIndexResultsList);

		// this does only preflight
		var item1 = _query.SingleItem(itemInChildFolderPath1)
			?.FileIndexItem;
		Assert.IsNotNull(item1);
		await _query.RemoveItemAsync(item1);

		var item2 = _query.SingleItem(collectionItemPath1)?.FileIndexItem;
		Assert.IsNotNull(item2);
		await _query.RemoveItemAsync(item2);
	}

	[TestMethod]
	public async Task
		InputOutputSubPathsPreflight_FileToDeleted_SingleItem_Change_Extension_WithCollections()
	{
		var itemInChildFolderPath1 = "/child_folder/test_24.jpg";
		var collectionItemPath1 = "/child_folder/test_24.dng";

		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath1));
		await _query.AddItemAsync(new FileIndexItem(collectionItemPath1));

		await _query.AddParentItemsAsync(itemInChildFolderPath1).ConfigureAwait(false);
		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2" },
			new List<string> { itemInChildFolderPath1, collectionItemPath1 });

		var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) =
			new RenameService(_query, iStorage, new FakeIWebLogger())
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1}",
					// Change to .jpeg
					"/child_folder2/test_24.jpeg", true);

		Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
		Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);

		Assert.AreEqual("/child_folder2/test_24.jpeg", toFileSubPaths[0]);
		Assert.AreEqual("/child_folder2/test_24.dng", toFileSubPaths[1]);

		Assert.IsEmpty(fileIndexResultsList);

		// this does only preflight
		var item1 = _query.SingleItem(itemInChildFolderPath1)
			?.FileIndexItem;
		Assert.IsNotNull(item1);
		await _query.RemoveItemAsync(item1);

		var item2 = _query.SingleItem(collectionItemPath1)?.FileIndexItem;
		Assert.IsNotNull(item2);
		await _query.RemoveItemAsync(item2);
	}

	[TestMethod]
	public async Task Rename_Move_SidecarFile_ShouldMove_FileToFolder()
	{
		const string item1dng = "/child_folder/test_20.dng";
		const string item1SideCar = "/child_folder/test_20.xmp";

		await _query.AddItemAsync(new FileIndexItem(item1dng));
		await _query.AddParentItemsAsync(item1dng);

		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2" },
			new List<string> { item1dng, item1SideCar }); // item1

		// Move DNG to different folder
		var renameFs = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename(item1dng, "/child_folder2");

		Assert.AreEqual(item1dng, renameFs[0].FilePath);
		Assert.AreEqual(item1dng.Replace("child_folder", "child_folder2"),
			renameFs[1].FilePath);

		// did move the side car file
		Assert.IsTrue(
			iStorage.ExistFile(item1SideCar.Replace("child_folder", "child_folder2")));
	}

	[TestMethod]
	public async Task Rename_Move_SidecarFile_ShouldNotMove_FileToFolder_ItsAJpeg()
	{
		var item1 = "/child_folder/test_20.jpg";
		var item1SideCar = "/child_folder/test_20.xmp";

		await _query.AddItemAsync(new FileIndexItem(item1));
		await _query.AddParentItemsAsync(item1);

		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2" },
			new List<string> { item1, item1SideCar });

		// Move Jpg to different folder but the xmp should be ignored
		var renameFs = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename(item1, "/child_folder2");

		Assert.AreEqual(item1, renameFs.FirstOrDefault()?.FilePath);
		Assert.AreEqual(item1.Replace("child_folder", "child_folder2"),
			renameFs[1].FilePath);

		// it should not move the sidecar file
		Assert.IsFalse(
			iStorage.ExistFile(item1SideCar.Replace("child_folder", "child_folder2")));
	}

	[TestMethod]
	public async Task
		InputOutputSubPathsPreflight_FileToFolder_SingleItemWithCollectionsEnabled()
	{
		var itemInChildFolderPath1 = "/child_folder/test_07.jpg";
		var collectionItemPath1 = "/child_folder/test_07.png";

		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath1));
		await _query.AddItemAsync(new FileIndexItem(collectionItemPath1));

		await _query.AddParentItemsAsync(itemInChildFolderPath1).ConfigureAwait(false);
		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2" },
			new List<string> { itemInChildFolderPath1, collectionItemPath1 });

		var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) =
			new RenameService(_query, iStorage, new FakeIWebLogger())
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1}",
					"/child_folder2", true);

		Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
		Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);

		Assert.AreEqual("/child_folder2", toFileSubPaths[0]);
		Assert.AreEqual("/child_folder2", toFileSubPaths[1]);

		Assert.IsEmpty(fileIndexResultsList);

		// CLEAN
		var item1 = _query.SingleItem(itemInChildFolderPath1)
			?.FileIndexItem;
		Assert.IsNotNull(item1);
		await _query.RemoveItemAsync(item1);

		var item2 = _query.SingleItem(collectionItemPath1)?.FileIndexItem;
		Assert.IsNotNull(item2);
		await _query.RemoveItemAsync(item2);
	}

	[TestMethod]
	public async Task InputOutputSubPathsPreflight_FileToFolder_MultipleFiles_CollectionsTrue()
	{
		// write test that has input /test.jpg;/test2.jpg > /test;/test2 and both has 2 or 3 collection files
		// the other should be ok

		var itemInChildFolderPath1 = "/child_folder/test_01.jpg";
		var collectionItemPath1 = "/child_folder/test_01.png";

		var itemInChildFolderPath2 = "/child_folder/test_02.jpg";
		var collectionItemPath2 = "/child_folder/test_02.png";

		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath1));
		await _query.AddItemAsync(new FileIndexItem(collectionItemPath1));
		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath2));
		await _query.AddItemAsync(new FileIndexItem(collectionItemPath2));

		await _query.AddParentItemsAsync(itemInChildFolderPath1).ConfigureAwait(false);
		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2", "/other" },
			new List<string>
			{
				itemInChildFolderPath1,
				collectionItemPath1,
				itemInChildFolderPath2,
				collectionItemPath2
			});

		var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) =
			new RenameService(_query, iStorage, new FakeIWebLogger())
				.InputOutputSubPathsPreflight(
					$"{itemInChildFolderPath1};{itemInChildFolderPath2}",
					"/child_folder2;/other", true);

		Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
		Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);
		Assert.AreEqual(itemInChildFolderPath2, inputFileSubPaths[2]);
		Assert.AreEqual(collectionItemPath2, inputFileSubPaths[3]);

		Assert.AreEqual("/child_folder2", toFileSubPaths[0]);
		Assert.AreEqual("/child_folder2", toFileSubPaths[1]);
		Assert.AreEqual("/other", toFileSubPaths[2]);
		Assert.AreEqual("/other", toFileSubPaths[3]);

		Assert.IsEmpty(fileIndexResultsList);

		// CLEAN
		var item1 = _query.SingleItem(itemInChildFolderPath1)
			?.FileIndexItem;
		Assert.IsNotNull(item1);
		await _query.RemoveItemAsync(item1);

		var item2 = _query.SingleItem(collectionItemPath1)?.FileIndexItem;
		Assert.IsNotNull(item2);
		await _query.RemoveItemAsync(item2);

		// CLEAN
		var item3 = _query.SingleItem(itemInChildFolderPath2)
			?.FileIndexItem;
		Assert.IsNotNull(item3);
		await _query.RemoveItemAsync(item3);

		var item4 = _query.SingleItem(collectionItemPath2)?.FileIndexItem;
		Assert.IsNotNull(item4);
		await _query.RemoveItemAsync(item4);
	}

	[TestMethod]
	public async Task
		InputOutputSubPathsPreflight_FileToFolder_MultipleFiles_CollectionsFalse_Aka_Disabled()
	{
		// write test that has input /test.jpg;/test2.jpg > /test;/test2 and both has 2 or 3 collection files
		// But this one's are not used
		// the other should be ok

		var itemInChildFolderPath1 = "/child_folder/test_05.jpg";
		var collectionItemPath1 = "/child_folder/test_05.png";

		var itemInChildFolderPath2 = "/child_folder/test_06.jpg";
		var collectionItemPath2 = "/child_folder/test_06.png";

		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath1));
		await _query.AddItemAsync(new FileIndexItem(collectionItemPath1));
		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath2));
		await _query.AddItemAsync(new FileIndexItem(collectionItemPath2));

		await _query.AddParentItemsAsync(itemInChildFolderPath1);
		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2", "/other" },
			new List<string>
			{
				itemInChildFolderPath1,
				collectionItemPath1,
				itemInChildFolderPath2,
				collectionItemPath2
			});

		// Collections disabled!
		var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) =
			new RenameService(_query, iStorage, new FakeIWebLogger())
				.InputOutputSubPathsPreflight(
					$"{itemInChildFolderPath1};{itemInChildFolderPath2}",
					"/child_folder2;/other", false);

		Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
		Assert.AreEqual(itemInChildFolderPath2, inputFileSubPaths[1]);

		Assert.AreEqual("/child_folder2", toFileSubPaths[0]);
		Assert.AreEqual("/other", toFileSubPaths[1]);

		Assert.IsEmpty(fileIndexResultsList);

		// CLEAN
		var item1 = _query.SingleItem(itemInChildFolderPath1)
			?.FileIndexItem;
		Assert.IsNotNull(item1);
		await _query.RemoveItemAsync(item1);

		var item2 = _query.SingleItem(collectionItemPath1)?.FileIndexItem;
		Assert.IsNotNull(item2);
		await _query.RemoveItemAsync(item2);

		// CLEAN
		var item3 = _query.SingleItem(itemInChildFolderPath2)
			?.FileIndexItem;
		Assert.IsNotNull(item3);
		await _query.RemoveItemAsync(item3);

		var item4 = _query.SingleItem(collectionItemPath2)?.FileIndexItem;
		Assert.IsNotNull(item4);
		await _query.RemoveItemAsync(item4);
	}

	[TestMethod]
	public async Task InputOutputSubPathsPreflight_FileToFolder_MultipleFiles_PartlyNotFound()
	{
		var itemInChildFolderPath1 = "/child_folder/test_03.jpg";
		var collectionItemPath1 = "/child_folder/test_03.png";

		var itemInChildFolderPath2 = "/child_folder/test_04.jpg";

		await _query.AddItemAsync(new FileIndexItem(itemInChildFolderPath1));
		await _query.AddItemAsync(new FileIndexItem(collectionItemPath1));

		await _query.AddParentItemsAsync(itemInChildFolderPath1);

		var iStorage = new FakeIStorage(
			new List<string> { "/", "/child_folder", "/child_folder2", "/other" },
			new List<string> { itemInChildFolderPath1, collectionItemPath1 });

		// nr 2 is does not exist in the database
		var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) =
			new RenameService(_query, iStorage, new FakeIWebLogger())
				.InputOutputSubPathsPreflight(
					$"{itemInChildFolderPath1};{itemInChildFolderPath2}",
					"/child_folder2;/other", true);

		Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
		Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);

		Assert.AreEqual("/child_folder2", toFileSubPaths[0]);
		Assert.AreEqual("/child_folder2", toFileSubPaths[1]);

		Assert.HasCount(1, fileIndexResultsList);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex,
			fileIndexResultsList[0].Status);

		await _query.RemoveItemAsync(_query.SingleItem(itemInChildFolderPath1)?.FileIndexItem!);
		await _query.RemoveItemAsync(_query.SingleItem(collectionItemPath1)?.FileIndexItem!);
	}

	[TestMethod]
	public async Task Rename_FolderToExistingFolderInDatabaseButNotOnDisk()
	{
		var iStorage = new FakeIStorage(new List<string> { "/", "/source_folder" });

		await _query.AddItemAsync(
			new FileIndexItem("/source_folder") { IsDirectory = true });
		await _query.AddItemAsync(
			new FileIndexItem("/target_folder_3") { IsDirectory = true });

		// Move Jpg to different folder but the xmp should be ignored
		var renameFs = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename("/source_folder", "/target_folder_3");


		var countTargetFolder = ( await _query.GetAllRecursiveAsync() )
			.Where(p => p.FilePath == "/target_folder_3").ToList();

		Assert.HasCount(1, countTargetFolder);

		Assert.AreEqual("/source_folder", renameFs[1].FilePath);
		Assert.AreEqual("/target_folder_3", renameFs[0].FilePath);

		var sourceFolder = renameFs
			.Find(p => p.FilePath == "/source_folder");
		var targetFolder = renameFs
			.Find(p => p.FilePath == "/target_folder_3");

		Assert.AreEqual("/source_folder", sourceFolder?.FilePath);
		Assert.AreEqual("/target_folder_3", targetFolder?.FilePath);

		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, sourceFolder?.Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, targetFolder?.Status);
	}

	[TestMethod]
	public async Task Rename_FolderToExistingFolder_With_Child_Items_InDatabaseButNotOnDisk()
	{
		var iStorage = new FakeIStorage(
			new List<string> { "/", "/source_folder_2" },
			new List<string> { "/source_folder_2/test.jpg" }
		);

		await _query.AddItemAsync(
			new FileIndexItem("/source_folder_2") { IsDirectory = true });
		await _query.AddItemAsync(
			new FileIndexItem("/source_folder_2/test.jpg"));
		await _query.AddItemAsync(
			new FileIndexItem("/target_folder_4") { IsDirectory = true });
		await _query.AddItemAsync(
			new FileIndexItem("/target_folder_4/test.jpg"));


		var renameFs = await new RenameService(_query, iStorage, new FakeIWebLogger())
			.Rename("/source_folder_2", "/target_folder_4");

		var countTargetChildItem = ( await _query.GetAllRecursiveAsync() )
			.Where(p => p.FilePath == "/target_folder_4/test.jpg").ToList();

		Assert.HasCount(1, countTargetChildItem);

		var countTargetFolder = ( await _query.GetAllRecursiveAsync() )
			.Where(p => p.FilePath == "/target_folder_4").ToList();

		Assert.HasCount(1, countTargetFolder);

		var sourceFolder = renameFs
			.Find(p => p.FilePath == "/source_folder_2");
		var targetFile = renameFs
			.Find(p => p.FilePath == "/target_folder_4/test.jpg");
		var targetFolder = renameFs
			.Find(p => p.FilePath == "/target_folder_4");

		Assert.AreEqual("/source_folder_2", sourceFolder?.FilePath);
		Assert.AreEqual("/target_folder_4/test.jpg", targetFile?.FilePath);
		Assert.AreEqual("/target_folder_4", targetFolder?.FilePath);

		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundSourceMissing, sourceFolder?.Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, targetFile?.Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, targetFolder?.Status);
	}

	[TestMethod]
	public async Task FromFolderToFolder_Null_exception()
	{
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
			await new RenameService(null!, null!, null!).FromFolderToFolder(null!,
				null!, null!, null!));
		// expect exception
	}

	[TestMethod]
	public async Task RenameFsTest_MergeToLowerPath()
	{
		// At the moment there is no check for case sensitive file or filenames

		const string beforePath = "/test/case_sensitive.jpg";
		const string afterPath = "/test/Case_Sensitive.jpg";

		var storage = new FakeIStorage(new List<string> { "/exist", beforePath });
		var query =
			new FakeIQuery(
				new List<FileIndexItem> { new(beforePath) });

		var renameService = new RenameService(query, storage, new FakeIWebLogger());

		await renameService.Rename(beforePath, afterPath);

		var beforeItem = await query.GetObjectByFilePathAsync(beforePath);
		Assert.IsNull(beforeItem);

		var after = await query.GetObjectByFilePathAsync(afterPath);
		Assert.AreEqual(afterPath, after!.FilePath);
	}

	[TestMethod]
	public async Task Rename_ShouldResetCacheForFileHash()
	{
		// Arrange
		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase(nameof(Rename_ShouldResetCacheForFileHash));
		var context = new ApplicationDbContext(builder.Options);

		var appSettings = new AppSettings { StorageFolder = "/", ThumbnailTempFolder = "/" };
		var memoryCache = new MemoryCache(new MemoryCacheOptions());
		var query = new Query(context, appSettings, null, new FakeIWebLogger(), memoryCache);

		var fileIndexItem = new FileIndexItem { FileHash = "test-hash", FilePath = "/test.jpg" };
		await query.AddItemAsync(fileIndexItem);

		var setCacheResult = await query.GetSubPathByHashAsync(fileIndexItem.FileHash);
		Assert.AreEqual("/test.jpg", setCacheResult,
			"Cache should contain the item before rename.");

		var fakeStorage = new FakeIStorage(["/"], ["/test.jpg"]);
		var renameService = new RenameService(query, fakeStorage, new FakeIWebLogger());

		// Act
		await renameService.Rename("/test.jpg", "/new-test.jpg");

		// Assert
		var cacheResult = await query.GetSubPathByHashAsync(fileIndexItem.FileHash);
		Assert.AreEqual("/new-test.jpg", cacheResult,
			"Cache should be reset after rename");
	}

	[TestMethod]
	public async Task PreviewBatchRename_FileInExistsFolder_ReturnsExpectedMappings_ForValidFiles()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([_folderExist.FilePath!],
			[_fileInExist.FilePath!]);
		var service = new RenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string> { _fileInExist.FilePath! };
		const string tokenPattern = "{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}";
		var result = service.PreviewBatchRename(filePaths,
			tokenPattern);
		CollectionAssert.AreEqual(new List<string> { _fileInExist.FilePath! },
			result.Select(x => x.SourceFilePath).ToList());
		Assert.IsFalse(result[0].HasError);
		Assert.EndsWith(".jpg", result[0].TargetFilePath);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task PreviewBatchRename_FileInRoot_ReturnsExpectedMappings_ForValidFiles()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([],
			[_fileInRoot.FilePath!]);
		var service = new RenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string> { _fileInRoot.FilePath! };
		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}_{filenamebase}.{ext}";
		var result = service.PreviewBatchRename(filePaths,
			tokenPattern);
		CollectionAssert.AreEqual(new List<string> { _fileInRoot.FilePath! },
			result.Select(x => x.SourceFilePath).ToList());
		Assert.IsFalse(result[0].HasError);
		Assert.EndsWith(".jpg", result[0].TargetFilePath);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task
		PreviewBatchRename_FileInRoot_ParentNull_ReturnsExpectedMappings_ForValidFiles()
	{
		await CreateFoldersAndFilesInDatabase();
		_fileInRoot.ParentDirectory = null;

		var iStorage = new FakeIStorage([],
			[_fileInRoot.FilePath!]);
		var service = new RenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string> { _fileInRoot.FilePath! };
		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}_{filenamebase}.{ext}";
		var result = service.PreviewBatchRename(filePaths,
			tokenPattern);
		CollectionAssert.AreEqual(new List<string> { _fileInRoot.FilePath! },
			result.Select(x => x.SourceFilePath).ToList());
		Assert.IsFalse(result[0].HasError);
		Assert.EndsWith(".jpg", result[0].TargetFilePath);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task PreviewBatchRenameAsync_ReturnsError_ForInvalidPattern()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([_folderExist.FilePath!],
			[_fileInExist.FilePath!]);
		var service = new RenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string> { _fileInExist.FilePath! };
		const string tokenPattern = "{invalidtoken}";
		var result = service.PreviewBatchRename(filePaths, tokenPattern);
		CollectionAssert.AllItemsAreNotNull(result);
		Assert.IsTrue(result.Any(x => x.HasError));
		Assert.Contains("Invalid pattern", result[0].ErrorMessage!);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public void PreviewBatchRenameAsync_ReturnsError_WhenFileNotFound()
	{
		var iStorage =
			new FakeIStorage([_folderExist.FilePath!], new List<string>());
		var service = new RenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string> { "/notfound.jpg" };
		const string tokenPattern = "{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}";
		var result = service.PreviewBatchRename(filePaths, tokenPattern);
		CollectionAssert.AllItemsAreNotNull(result);
		Assert.IsTrue(result.Any(x => x.HasError));
		Assert.AreEqual("File not found in database", result[0].ErrorMessage);
	}

	[TestMethod]
	public void PreviewBatchRenameAsync_ReturnsEmptyList_WhenNoFiles()
	{
		var iStorage = new FakeIStorage([], []);
		var service = new RenameService(_query, iStorage, new FakeIWebLogger());
		var filePaths = new List<string>();
		const string tokenPattern = "{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}";
		var result = service.PreviewBatchRename(filePaths, tokenPattern);
		CollectionAssert.AllItemsAreNotNull(result);
		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_IsEmpty()
	{
		var iStorage = new FakeIStorage([], []);
		var service =
			new RenameService(new FakeIQuery([new FileIndexItem("/test.jpg")]),
				iStorage, new FakeIWebLogger());
		var result = await service.ExecuteBatchRenameAsync([]);

		Assert.IsEmpty(result);
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_NotFound()
	{
		var iStorage = new FakeIStorage([], []);
		var service =
			new RenameService(new FakeIQuery([new FileIndexItem("/test.jpg")]),
				iStorage, new FakeIWebLogger());
		var result = await service.ExecuteBatchRenameAsync([
			new BatchRenameMapping
			{
				SourceFilePath = "/notfound.jpg",
				TargetFilePath = "/newname.jpg",
				HasError = false,
				RelatedFilePaths = []
			}
		]);

		Assert.HasCount(1, result);
		Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, result[0].Status);
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_Null()
	{
		var iStorage = new FakeIStorage([], []);
		var service =
			new RenameService(new FakeIQueryException(new AccessViolationException("test")),
				iStorage, new FakeIWebLogger());
		var result = await service.ExecuteBatchRenameAsync([
			new BatchRenameMapping
			{
				SourceFilePath = null!,
				TargetFilePath = null!,
				HasError = false,
				RelatedFilePaths = []
			}
		]);

		Assert.HasCount(1, result);
		Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, result[0].Status);
	}

	[TestMethod]
	public void PreviewBatchRename_Null()
	{
		var iStorage = new FakeIStorage([], []);
		var service =
			new RenameService(new FakeIQueryException(new AccessViolationException("test")),
				iStorage, new FakeIWebLogger());
		var result = service.PreviewBatchRename(
			[null!], "{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}");

		Assert.IsEmpty(result);
	}

	[TestMethod]
	public void PreviewBatchRename_InvalidFileName()
	{
		var iStorage = new FakeIStorage([], ["/test.jpg"]);
		var service =
			new RenameService(new FakeIQuery([new FileIndexItem("/test.jpg")]),
				iStorage, new FakeIWebLogger());
		var result = service.PreviewBatchRename(
			["/test.jpg"], "{yyyy}{MM}{dd}_{filenamebase}{seqn}__{ext}");

		Assert.HasCount(1, result);
		Assert.IsTrue(result[0].HasError);
		Assert.AreEqual(
			"Failed to generate filename: Generated filename is invalid: 00010101_test__jpg",
			result[0].ErrorMessage);
	}


	[TestMethod]
	public async Task ExecuteBatchRenameAsync_BatchRename_SimpleFiles()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([_parentFolder.FilePath!, _folderExist.FilePath!],
			[_fileInExist.FilePath!, _fileInRoot.FilePath!]);
		var service = new RenameService(_query, iStorage, new FakeIWebLogger());

		var mappings = new List<BatchRenameMapping>
		{
			new()
			{
				SourceFilePath = _fileInExist.FilePath!,
				TargetFilePath = "/exist/20220506_000000.jpg",
				HasError = false,
				RelatedFilePaths = []
			},
			new()
			{
				SourceFilePath = _fileInRoot.FilePath!,
				TargetFilePath = "/20220506_000000.jpg",
				HasError = false,
				RelatedFilePaths = []
			}
		};

		var result = await service.ExecuteBatchRenameAsync(mappings, false);

		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).ToList();

		Assert.HasCount(2, filteredResults);
		Assert.AreEqual("/exist/20220506_000000.jpg", filteredResults[0].FilePath);
		Assert.AreEqual("/20220506_000000.jpg", filteredResults[1].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[0].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[1].Status);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_WithPreview_SimpleFiles()
	{
		await CreateFoldersAndFilesInDatabase();
		var iStorage = new FakeIStorage([_parentFolder.FilePath!, _folderExist.FilePath!],
			[_fileInExist.FilePath!, _fileInRoot.FilePath!]);
		var service = new RenameService(_query, iStorage, new FakeIWebLogger());

		const string tokenPattern = "{yyyy}-{MM}-{dd}_{HH}-{mm}-{ss}.{ext}";
		var mappings = service.PreviewBatchRename(
			[_fileInExist.FilePath!, _fileInRoot.FilePath!],
			tokenPattern);

		var result = await service.ExecuteBatchRenameAsync(mappings, false);

		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).ToList();

		Assert.HasCount(2, filteredResults);
		Assert.AreEqual("/exist/2022-05-06_00-00-00.jpg", filteredResults[0].FilePath);
		Assert.AreEqual("/2022-05-06_00-00-00-1.jpg", filteredResults[1].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[0].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[1].Status);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_Sequence_AppendsSequenceSuffix()
	{
		await CreateFoldersAndFilesInDatabase();
		// Simulate two files with the same datetime, requiring sequence handling
		var file1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});
		var file2 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});
		var iStorage = new FakeIStorage([_folderExist.FilePath!],
		[
			new FileIndexItem(file1.FilePath!).FilePath!,
			new FileIndexItem(file2.FilePath!).FilePath!
		]);
		var service = new RenameService(_query, iStorage, new FakeIWebLogger());

		var mappings = new List<BatchRenameMapping>
		{
			new()
			{
				SourceFilePath = file1.FilePath!,
				TargetFilePath = "/exist/20260101_180000.jpg",
				HasError = false,
				RelatedFilePaths = []
			},
			new()
			{
				SourceFilePath = file2.FilePath!,
				TargetFilePath = "/exist/20260101_180000-1.jpg",
				HasError = false,
				RelatedFilePaths = []
			}
		};

		var result = await service.ExecuteBatchRenameAsync(mappings, false);
		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).ToList();

		Assert.HasCount(2, filteredResults);
		Assert.AreEqual("/exist/20260101_180000.jpg", filteredResults[0].FilePath);
		Assert.AreEqual("/exist/20260101_180000-1.jpg", filteredResults[1].FilePath);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[0].Status);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, filteredResults[1].Status);

		await _query.RemoveItemAsync(file1);
		await _query.RemoveItemAsync(file2);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_Sequence_Raw_AppendsSequenceSuffix()
	{
		await CreateFoldersAndFilesInDatabase();

		var file1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.jpg",
			ParentDirectory = "/exist",
			FileHash = "DSC0001.jpg",
			IsDirectory = false,
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file1Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0001.arw",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.jpg",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.arw",
			AddToDatabase = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc)
		});

		var iStorage = new FakeIStorage([_folderExist.FilePath!],
		[
			new FileIndexItem(file1.FilePath!).FilePath!,
			new FileIndexItem(file2.FilePath!).FilePath!,
			new FileIndexItem(file1Raw.FilePath!).FilePath!,
			new FileIndexItem(file2Raw.FilePath!).FilePath!
		]);

		var service = new RenameService(_query, iStorage, new FakeIWebLogger());

		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}.{ext}";

		var mappings = service.PreviewBatchRename(
			[file1.FilePath!, file2.FilePath!, file2Raw.FilePath!, file1Raw.FilePath!],
			tokenPattern);

		var result = await service.ExecuteBatchRenameAsync(mappings);

		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).OrderBy(p => p.FileName).ToList();

		Assert.HasCount(4, filteredResults);

		Assert.AreEqual("/exist/20260101_180000-1.arw", filteredResults[0].FilePath);
		Assert.AreEqual("DSC0002.arw", filteredResults[0].FileHash);

		Assert.AreEqual("/exist/20260101_180000-1.jpg", filteredResults[1].FilePath);
		Assert.AreEqual("DSC0002.jpg", filteredResults[1].FileHash);

		Assert.AreEqual("/exist/20260101_180000.arw", filteredResults[2].FilePath);
		Assert.AreEqual("DSC0001.arw", filteredResults[2].FileHash);

		Assert.AreEqual("/exist/20260101_180000.jpg", filteredResults[3].FilePath);
		Assert.AreEqual("DSC0001.jpg", filteredResults[3].FileHash);

		await _query.RemoveItemAsync(file1);
		await _query.RemoveItemAsync(file2);
		await _query.RemoveItemAsync(file1Raw);
		await _query.RemoveItemAsync(file2Raw);
		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public async Task ExecuteBatchRenameAsync_Sequence_RawXmp_Explicit_AppendsSequenceSuffix()
	{
		await CreateFoldersAndFilesInDatabase();

		var file1 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.jpg",
			ParentDirectory = "/exist",
			FileHash = "DSC0001.jpg",
			IsDirectory = false,
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file1Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0001.arw",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file1Xmp = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0001.xmp",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0001.xmp",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2 = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.jpg",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.jpg",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var file2Raw = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.arw",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.arw",
			AddToDatabase = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1,
				18, 0, 0, DateTimeKind.Utc)
		});

		var file2Xmp = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "DSC0002.xmp",
			ParentDirectory = "/exist",
			IsDirectory = false,
			FileHash = "DSC0002.xmp",
			AddToDatabase = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc),
			DateTime = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc)
		});

		var iStorage = new FakeIStorage([_folderExist.FilePath!],
		[
			new FileIndexItem(file1.FilePath!).FilePath!,
			new FileIndexItem(file2.FilePath!).FilePath!,
			new FileIndexItem(file1Raw.FilePath!).FilePath!,
			new FileIndexItem(file2Raw.FilePath!).FilePath!,
			new FileIndexItem(file1Xmp.FilePath!).FilePath!,
			new FileIndexItem(file2Xmp.FilePath!).FilePath!
		]);

		var service = new RenameService(_query, iStorage, new FakeIWebLogger());

		const string tokenPattern = "{yyyy}{MM}{dd}_{HH}{mm}{ss}.{ext}";

		var mappings = service.PreviewBatchRename(
			[
				file1.FilePath!, file2.FilePath!,
				file2Raw.FilePath!, file1Raw.FilePath!,
				file1Xmp.FilePath!, file2Xmp.FilePath!
			],
			tokenPattern);

		var result = await service.ExecuteBatchRenameAsync(mappings);

		var filteredResults = result.Where(p => p is
		{
			Status: FileIndexItem.ExifStatus.Ok,
			IsDirectory: false
		}).OrderBy(p => p.FileName).ToList();

		Assert.HasCount(6, filteredResults);

		Assert.AreEqual("/exist/20260101_180000-1.arw", filteredResults[0].FilePath);
		Assert.AreEqual("DSC0002.arw", filteredResults[0].FileHash);

		Assert.AreEqual("/exist/20260101_180000-1.jpg", filteredResults[1].FilePath);
		Assert.AreEqual("DSC0002.jpg", filteredResults[1].FileHash);

		Assert.AreEqual("/exist/20260101_180000-1.xmp", filteredResults[2].FilePath);
		Assert.AreEqual("DSC0002.xmp", filteredResults[2].FileHash);

		Assert.AreEqual("/exist/20260101_180000.arw", filteredResults[3].FilePath);
		Assert.AreEqual("DSC0001.arw", filteredResults[3].FileHash);

		Assert.AreEqual("/exist/20260101_180000.jpg", filteredResults[4].FilePath);
		Assert.AreEqual("DSC0001.jpg", filteredResults[4].FileHash);

		Assert.AreEqual("/exist/20260101_180000.xmp", filteredResults[5].FilePath);
		Assert.AreEqual("DSC0001.xmp", filteredResults[5].FileHash);

		await _query.RemoveItemAsync(file1);
		await _query.RemoveItemAsync(file2);
		await _query.RemoveItemAsync(file1Raw);
		await _query.RemoveItemAsync(file2Raw);
		await _query.RemoveItemAsync(file1Xmp);
		await _query.RemoveItemAsync(file2Xmp);

		await RemoveFoldersAndFilesInDatabase();
	}

	[TestMethod]
	public void PreviewBatchRename_WithJsonSidecarFile_IncludesSidecarInMapping()
	{
		// Arrange
		const string sourceFilePath = "/folder/testfile.jpg";
		var sidecarFilePath = JsonSidecarLocation.JsonLocation(sourceFilePath);
		var filePaths = new List<string> { sourceFilePath, sidecarFilePath };
		var iStorage = new FakeIStorage(["/folder"], [sourceFilePath, sidecarFilePath]);
		var fakeQuery = new FakeIQuery([
			new FileIndexItem(sourceFilePath)
			{
				FileName = "testfile.jpg",
				ParentDirectory = "/folder",
				DateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc)
			},
			new FileIndexItem(sidecarFilePath)
			{
				FileName = ".starsky.testfile.jpg.json",
				ParentDirectory = "/folder",
				DateTime = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc)
			}
		]);
		var service = new RenameService(fakeQuery, iStorage, new FakeIWebLogger());
		const string tokenPattern = "{yyyy}{MM}{dd}_{filenamebase}{seqn}.{ext}";

		// Act
		var result = service.PreviewBatchRename(filePaths, tokenPattern);

		// Assert
		Assert.HasCount(2, result);
		Assert.IsTrue(result.Any(x => x.SourceFilePath == sourceFilePath));
		Assert.IsTrue(result.Any(x => x.SourceFilePath == sidecarFilePath));
		var mainFile = result.First(x => x.SourceFilePath == sourceFilePath);
		var sidecar = result.First(x => x.SourceFilePath == sidecarFilePath);
		Assert.IsFalse(mainFile.HasError);
		Assert.IsFalse(sidecar.HasError);
		Assert.EndsWith(".jpg", mainFile.TargetFilePath);
		Assert.EndsWith(".json", sidecar.TargetFilePath);
		Assert.Contains(".starsky.", sidecar.TargetFilePath);
	}
}

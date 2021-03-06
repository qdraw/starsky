using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.rename.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.Services;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using SyncService = starskycore.Services.SyncService;

namespace starskytest.starsky.feature.rename.Services
{
	[TestClass]
	public class RenameServiceTest
	{
		private readonly Query _query;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _newImage;
		private readonly SyncService _sync;
		private readonly StorageSubPathFilesystem _iStorageSubPath;

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

			_appSettings = new AppSettings
			{
				StorageFolder = PathHelper.AddBackslash(_newImage.BasePath),
				ThumbnailTempFolder = _newImage.BasePath
			};
			_query = new Query(context,memoryCache, _appSettings);

			if ( _query.GetAllFiles("/").All(p => p.FileName != _newImage.FileName) )
			{
				_query.AddItem(new FileIndexItem
				{
					FileName = _newImage.FileName,
					ParentDirectory = "/",
					AddToDatabase = DateTime.UtcNow,
				});
			}
			
			var iStorage = new StorageSubPathFilesystem(_appSettings);

			var readMeta = new ReadMeta(iStorage,_appSettings,memoryCache);
			
			_iStorageSubPath = new StorageSubPathFilesystem(_appSettings);
			
			var services = new ServiceCollection();
			var selectorStorage = new FakeSelectorStorage(iStorage);

			_sync = new SyncService(_query,_appSettings,selectorStorage);
		}

		[TestMethod]
		public void RenameFsTest_DuplicateFile()
		{
			var fileAlreadyExistSubPath = "/already_8758.txt";
			_iStorageSubPath.ExistFile(fileAlreadyExistSubPath);
			
			if ( !_iStorageSubPath.ExistFile(fileAlreadyExistSubPath) )
			{
				_iStorageSubPath.WriteStream(new PlainTextFileHelper().StringToStream("test"),
					fileAlreadyExistSubPath);
			}
			
			var renameFs = new RenameService( _query,_iStorageSubPath).Rename(_newImage.DbPath,
				fileAlreadyExistSubPath);
			
			var result = new PlainTextFileHelper().StreamToString(
				_iStorageSubPath.ReadStream(fileAlreadyExistSubPath));
			
			// it should not overwrite the target file
			Assert.AreEqual(result.Contains("test"), true);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, renameFs.FirstOrDefault().Status );

			// and there are no more files with a name that looks like already_8758
			// should only have one file with filename: already_8758
			var count = _iStorageSubPath.GetAllFilesInDirectory("/")
				.Count(p => p.StartsWith(fileAlreadyExistSubPath));
			Assert.AreEqual(1, count);
			
			// and remove the file afterwards
			_iStorageSubPath.FileDelete(fileAlreadyExistSubPath);
		}

		[TestMethod]
		public void RenameFsTest_MoveFileWithoutAnyItems()
		{
			var renameFs = new RenameService(_query,_iStorageSubPath).Rename("/non-exist.jpg", "/non-exist2.jpg");
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, renameFs.FirstOrDefault().Status );
		}
		
		[TestMethod]
		public void RenameFsTest_MoveFileToExistFolder_Items()
		{
			CreateFoldersAndFilesInDatabase();

			// remove file if already exist; we are not testing duplicate support here
			var existFullPath = Path.Combine(_newImage.BasePath, "exist");
			if ( File.Exists(Path.Combine(existFullPath, "test2.jpg")) )
			{
				File.Delete(Path.Combine(existFullPath, "test2.jpg"));
			}
			
			// check if dir exist
			if (!Directory.Exists(existFullPath) )
			{
				Directory.CreateDirectory(existFullPath);
			}
			
			var renameFs = new RenameService(_query,_iStorageSubPath)
				.Rename(_newImage.DbPath, "/exist/test2.jpg")
				.Where( p => p.Status != FileIndexItem.ExifStatus.Deleted).ToList();

			Assert.AreEqual(1,renameFs.Count);
			
			// query database
			var all = _query.GetAllRecursive();
			Assert.AreEqual(all.FirstOrDefault(
				p => p.FileName == "test2.jpg").FileName, "test2.jpg");
			
			
			// use cached view
			var singleItem = _query.SingleItem("/exist/test2.jpg");
			Assert.AreEqual("test2.jpg",singleItem.FileIndexItem.FileName);		
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFs.FirstOrDefault().Status );

			new StorageHostFullPathFilesystem().FolderDelete(Path.Combine(_newImage.BasePath, "exist"));

			RemoveFoldersAndFilesInDatabase();
		}
	
		[TestMethod]
		public void RenameFsTest_MoveDirWithItemsTest()
		{
			var existFullDirPath = Path.Combine(_newImage.BasePath, "dir1");
			Directory.CreateDirectory(existFullDirPath);
			// move an item to this directory	
			
			if (! File.Exists(_appSettings.DatabasePathToFilePath("/dir1/test3.jpg")) )
			{
				File.Move(_newImage.FullFilePath,
					_appSettings.DatabasePathToFilePath("/dir1/test3.jpg",false));
			}
			_sync.SingleFile("/dir1/test3.jpg");
			
			// query database
			var all = _query.GetAllRecursive();
			Assert.AreEqual(all.FirstOrDefault(
				p => p.FileName == "test3.jpg").FileName, "test3.jpg");
			
			
			var renameFs = new RenameService(_query,_iStorageSubPath).Rename("/dir1", "/dir2");
			// check if files are moved in the database

			var all2 = _query.GetAllRecursive();

			var selectFile3 = all2.FirstOrDefault(p => p.FileName == "test3.jpg");
			Assert.AreEqual("test3.jpg",selectFile3.FileName);
			Assert.AreEqual("/dir2",selectFile3.ParentDirectory);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFs[1].Status );
			
			var dir2FullDirPath = Path.Combine(_newImage.BasePath, "dir2");

			new StorageHostFullPathFilesystem().FolderDelete(dir2FullDirPath);
		}

		private FileIndexItem _folderExist;
		private FileIndexItem _folder1Exist;
		private FileIndexItem _fileInExist;
		private FileIndexItem _parentFolder;

		private void CreateFoldersAndFilesInDatabase()
		{
			_folderExist = _query.AddItem(new FileIndexItem
			{
				FileName = "exist",
				ParentDirectory = "/",
				AddToDatabase = DateTime.UtcNow,
				FileHash = "34567898765434567487984785487",
				IsDirectory = true
			});
			
			_fileInExist = _query.AddItem(new FileIndexItem
			{
				FileName = "file.jpg",
				ParentDirectory = "/exist",
				IsDirectory = false
			});

			_folder1Exist = _query.AddItem(new FileIndexItem
			{
				FileName = "folder1",
				ParentDirectory = "/",
				IsDirectory = true,
				FileHash = "3497867df894587",
			});
			
			_parentFolder = _query.AddItem(new FileIndexItem
			{
				FileName = "/",
				ParentDirectory = "/",
				IsDirectory = true,
			});
		}

		private void RemoveFoldersAndFilesInDatabase()
		{
			_query.RemoveItem(_folderExist);
			_query.RemoveItem(_folder1Exist);
			_query.RemoveItem(_fileInExist);
			_query.RemoveItem(_parentFolder);
		}

		[TestMethod]
		public void RenameFsTest_FakeIStorage_RenameOneFile()
		{
			// RenameFsTest_MoveFileToSameFolder_Items
			
			CreateFoldersAndFilesInDatabase();

			var iStorage = new FakeIStorage(new List<string>{_folderExist.FilePath},
				new List<string>{_fileInExist.FilePath});
			
			var renameFs = new RenameService(_query, iStorage)
				.Rename( _fileInExist.FilePath, _folderExist.FilePath+ "/test2.jpg")
				.Where(p => p.Status != FileIndexItem.ExifStatus.Deleted).ToList();
			
			// query database
			var all = _query.GetAllRecursive();
			Assert.AreEqual("test2.jpg", all.FirstOrDefault(p => p.FileName == "test2.jpg")?.FileName );

			// old item is not in db
			Assert.AreEqual(null, all.FirstOrDefault(p => p.FileName == "test.jpg")?.FileName);

			// use cached view
			var singleItem = _query.SingleItem( _folderExist.FilePath+ "/test2.jpg");
			Assert.AreEqual("test2.jpg",singleItem.FileIndexItem.FileName);			

			Assert.AreEqual(1,renameFs.Count);

			RemoveFoldersAndFilesInDatabase();
		}

		[TestMethod]
		public void RenameFsTest_RenameOneFile_JsonSidecarFile()
		{
			CreateFoldersAndFilesInDatabase();

			var iStorage = new FakeIStorage(new List<string>{_folderExist.FilePath},
				new List<string>{_fileInExist.FilePath,JsonSidecarLocation.JsonLocation(_fileInExist.FilePath)});
			
			var renameFs = new RenameService(_query, iStorage)
				.Rename( _fileInExist.FilePath, _folderExist.FilePath + "/test2.jpg");
			
			// check if sidecar json are moved (on fake Filesystem)
			var values = iStorage.GetAllFilesInDirectoryRecursive("/").ToList();
			Assert.AreEqual("/exist/.starsky.test2.jpg.json", 
				values.FirstOrDefault(p => p == "/exist/.starsky.test2.jpg.json"));
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFs[1].Status );
			
			RemoveFoldersAndFilesInDatabase();
		}

		[TestMethod]
		public void RenameFsTest_FakeIStorage_RenameOneFile_ToWrongNewFileName()
		{
			CreateFoldersAndFilesInDatabase();

			var iStorage = new FakeIStorage(new List<string>
			{
				_folderExist.FilePath
			},new List<string>
			{
				_fileInExist.FilePath
			});
			
			var renameFs = new RenameService(_query, iStorage)
				.Rename( _fileInExist.FilePath, _folderExist.FilePath + "/test2___");
			// so this operation is not supported
			
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported,renameFs.FirstOrDefault().Status );
			
			RemoveFoldersAndFilesInDatabase();
		}

		[TestMethod]
		public void RenameFsTest_FakeIStorage_FileToNonExistFolder_Items()
		{
			CreateFoldersAndFilesInDatabase();

			var initFolderList =  new List<string> { "/" };
			var initFileList = new List<string> { _fileInExist.FilePath };
			var istorage = new FakeIStorage(initFolderList,initFileList);
			var renameFs = new RenameService(_query, istorage)
				.Rename(initFileList.FirstOrDefault(), "/nonExist/test5.jpg", true)
				.Where(p => p.Status != FileIndexItem.ExifStatus.Deleted).ToList();
			
			var all2 = _query.GetAllRecursive();
			var selectFile3 = all2.FirstOrDefault(p => p.FileName == "test5.jpg");
			Assert.AreEqual("test5.jpg",selectFile3.FileName);
			Assert.AreEqual("/nonExist",selectFile3.ParentDirectory);

			// check if files are moved
			var values = istorage.GetAllFilesInDirectory("/nonExist").ToList();
			Assert.AreEqual("/nonExist/test5.jpg", values.FirstOrDefault(p => p == "/nonExist/test5.jpg"));
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFs.FirstOrDefault().Status );

			RemoveFoldersAndFilesInDatabase();
		}
		
		[TestMethod]
		public void RenameFsTest_FakeIStorage_File_To_ExistFolder_MoveToTheSamePath()
		{
			CreateFoldersAndFilesInDatabase();

			var initFolderList =  new List<string> { "/", "/exist" };
			var initFileList = new List<string> { _fileInExist.FilePath };
			var istorage = new FakeIStorage(initFolderList,initFileList);
			var renameFs = new RenameService(_query, istorage)
				.Rename(initFileList.FirstOrDefault(), "/exist/", true);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, renameFs.FirstOrDefault().Status );

			RemoveFoldersAndFilesInDatabase();
		}
		
		[TestMethod]
		public void RenameFsTest_FakeIStorage_File_To_ExistFolder() // there is a separate sidecar json test
		{
			CreateFoldersAndFilesInDatabase();

			var initFolderList =  new List<string> { "/", "/test" };
			var initFileList = new List<string> { _fileInExist.FilePath };
			var fakeIStorage = new FakeIStorage(initFolderList,initFileList);
			
			var renameFsResult = new RenameService(_query, fakeIStorage).
				Rename(initFileList.FirstOrDefault(), "/test/", true);

			var oldItem = _query.GetObjectByFilePath("/exist/file.jpg");
			Assert.IsNull(oldItem);
			
			// to file: (in database)
			var all2 = _query.GetAllRecursive().Where(p => p.ParentDirectory.Contains("/test"));
			var selectFile3 = all2.FirstOrDefault(p => p.FilePath == "/test/file.jpg");
			Assert.AreEqual("file.jpg",selectFile3.FileName);
			Assert.AreEqual("/test",selectFile3.ParentDirectory);

			// check if files are moved (on fake Filesystem)
			var values = fakeIStorage.GetAllFilesInDirectory("/test").ToList();
			Assert.AreEqual("/test/file.jpg", values.FirstOrDefault(p => p == "/test/file.jpg"));
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFsResult.FirstOrDefault().Status );

			RemoveFoldersAndFilesInDatabase();
		}
		
		[TestMethod]
		public void RenameFsTest_FakeIStorage_File_To_ExistFolder_Json_SidecarFile()
		{
			CreateFoldersAndFilesInDatabase();

			var initFolderList =  new List<string> { "/", "/test" };
			var initFileList = new List<string> { _fileInExist.FilePath, JsonSidecarLocation.JsonLocation(_fileInExist.FilePath) };
			
			var iStorage = new FakeIStorage(initFolderList,initFileList);
			
			// the input is still  FileName = "file.jpg", ParentDirectory = "/exist",
			var renameFs = new RenameService(_query, iStorage)
				.Rename(initFileList.FirstOrDefault(), "/test/", true);
			
			// to file: (in database)
			var all2 = _query.GetAllRecursive();
			var selectFile3 = all2.FirstOrDefault(p => p.FileName == "file.jpg");
			Assert.AreEqual("file.jpg",selectFile3.FileName);
			Assert.AreEqual("/test",selectFile3.ParentDirectory);

			// check if sidecar json are moved (on fake Filesystem)
			var values = iStorage.GetAllFilesInDirectoryRecursive("/test").ToList();
			Assert.AreEqual("/test/.starsky.file.jpg.json", 
				values.FirstOrDefault(p => p == "/test/.starsky.file.jpg.json"));
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFs.FirstOrDefault().Status );

			RemoveFoldersAndFilesInDatabase();
		}
		
		[TestMethod]
		public void RenameFsTest_FakeIStorage_mergeTwoFolders()
		{
			CreateFoldersAndFilesInDatabase();
			
			var existSubFolder = _query.AddItem(new FileIndexItem
			{
				FileName = "subfolder",
				ParentDirectory = _folder1Exist.FilePath,
				IsDirectory = true,
				FileHash = "InjectedAsExistSubFolder"
			});
			
			var existSubFolderChildJpg = _query.AddItem(new FileIndexItem
			{
				FileName = "child.jpg",
				ParentDirectory = _folder1Exist.FilePath + "/subfolder",
				FileHash = "InjectedAsExistSubFolderChildJpg"
			});
			

			var initFolderList =  new List<string> { "/", _folderExist.FilePath + "/subfolder", _folder1Exist.FilePath,
				_folderExist.FilePath };
			var initFileList = new List<string> { _fileInExist.FilePath, _folder1Exist.FilePath + "/subfolder/child.jpg",
				_folder1Exist.FilePath + "/subfolder/not_synced_item.jpg" };
			var istorage = new FakeIStorage(initFolderList,initFileList);
			
			// the call
			var renameFs = new RenameService(_query, istorage).Rename("/exist", "/folder1", true);
			
			// First check if fakeDisk is changed
			var folder1Files = istorage.GetAllFilesInDirectory("/folder1").ToList();
			var folder1Dir = istorage.GetDirectoryRecursive("/folder1").ToList();
			
			Assert.AreEqual("/folder1/file.jpg", folder1Files[0]);
			Assert.AreEqual("/folder1/subfolder", folder1Dir[0]);

			var existDirContent = istorage.GetDirectoryRecursive("/exist").ToList();
			var existFolder = istorage.GetAllFilesInDirectory("/exist").ToList();
			
			Assert.AreEqual(0,existDirContent.Count);
			Assert.AreEqual(0,existFolder.Count);
			
			// Now check if FakeDb is changed
			var all2 = _query.GetAllRecursive();

			Assert.AreEqual("/folder1/file.jpg",
				all2.FirstOrDefault(p => p.FileName == "file.jpg" && p.Status != FileIndexItem.ExifStatus.Deleted).FilePath);
			Assert.AreEqual("/folder1/subfolder",
				all2.FirstOrDefault(p => p.FileName == "subfolder" && p.Status != FileIndexItem.ExifStatus.Deleted).FilePath);
			Assert.AreEqual("/folder1/subfolder/child.jpg",
				all2.FirstOrDefault(p => p.FileName == "child.jpg" &&  p.Status != FileIndexItem.ExifStatus.Deleted).FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Deleted, renameFs[0].Status );
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFs[1].Status );

			_query.RemoveItem(existSubFolder);
			_query.RemoveItem(existSubFolderChildJpg);

			RemoveFoldersAndFilesInDatabase();
		}

		[TestMethod]
		public void RenameFsTest_TheSameInput()
		{
			var initFolderList =  new List<string> {};
			var initFileList = new List<string> {};
			var istorage = new FakeIStorage(initFolderList,initFileList);
			var renameFs = new RenameService(_query, istorage).Rename("/same", "/same");
			Assert.AreEqual(1,renameFs.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, renameFs.FirstOrDefault().Status);
		}

		[TestMethod]
		public void RenameFsTest_FakeIStorage_UnderstandTest()
		{
			// used to test the GetAllFilesInDirectory() fake class
			var initFolderList =  new List<string> {"/", "/test/subfolder", "/test", "/otherfolder" };
			var initFileList = new List<string> { "/test/test.jpg", "/test/subfolder/t.jpg", "/test/subfolder/child.jpg" };
			var iStorage = new FakeIStorage(initFolderList,
				initFileList).GetAllFilesInDirectory("/test").ToList();
			Assert.AreEqual(1,iStorage.Count);
		}

		[TestMethod]
		public void RenameFsTest_MoveAFolderIntoAFile()
		{
			CreateFoldersAndFilesInDatabase();
			var iStorage = new FakeIStorage();
			var renameFs = new RenameService(_query, iStorage).Rename(_folderExist.FilePath, _fileInExist.FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, renameFs[0].Status);
		}

		[TestMethod]
		public void Rename_MoveFileToRootFolder()
		{
			var itemInChildFolderPath = "/child_folder/test_01.jpg";
			_query.AddItem(new FileIndexItem(itemInChildFolderPath));
			_query.AddParentItemsAsync(itemInChildFolderPath).ConfigureAwait(false);
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder"}, 
				new List<string>{"/child_folder/test_01.jpg"});

			var renameFs = new RenameService(_query, iStorage).Rename(itemInChildFolderPath, "/");
			
			Assert.AreEqual("/",renameFs.FirstOrDefault().ParentDirectory);
			Assert.AreEqual("/test_01.jpg",renameFs.FirstOrDefault().FilePath);

			Assert.AreEqual("/test_01.jpg", _query.SingleItem("/test_01.jpg").FileIndexItem.FilePath);
			Assert.AreEqual(null, _query.SingleItem(itemInChildFolderPath));
		}
		
		[TestMethod]
		public void Rename_Move_FileToFolder_Collections()
		{
			var itemInChildFolderPath = "/child_folder/test_10.jpg";
			_query.AddItem(new FileIndexItem(itemInChildFolderPath));
			_query.AddItem(new FileIndexItem("/child_folder/test_10.png"));
			_query.AddParentItemsAsync(itemInChildFolderPath).ConfigureAwait(false);
			
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2"}, 
				new List<string>{"/child_folder/test_10.jpg", "/child_folder/test_10.png"});

			var renameFs = new RenameService(_query, iStorage)
				.Rename(itemInChildFolderPath, "/child_folder2");
			
			Assert.AreEqual("/child_folder2",renameFs.FirstOrDefault().ParentDirectory);
			Assert.AreEqual("/child_folder2/test_10.jpg",renameFs.FirstOrDefault().FilePath);

			Assert.AreEqual("/child_folder2/test_10.jpg", 
				_query.SingleItem("/child_folder2/test_10.jpg").FileIndexItem.FilePath);
			Assert.AreEqual("/child_folder2/test_10.png", 
				_query.SingleItem("/child_folder2/test_10.png").FileIndexItem.FilePath);	
			
			Assert.AreEqual(null, _query.SingleItem(itemInChildFolderPath));
			Assert.AreEqual(null, _query.SingleItem("/child_folder/test_10.png"));
		}
		
		[TestMethod]
		public void Rename_Move_FileToDeleted_Collections()
		{
			var fromItemJpg = "/child_folder/test_21.jpg";
			var fromItemDng = "/child_folder/test_21.dng";
			var toItemJpg = "/child_folder/test_21_edit.jpg";
			var toItemDng = "/child_folder/test_21_edit.dng";

			_query.AddItem(new FileIndexItem(fromItemJpg));
			_query.AddItem(new FileIndexItem(fromItemDng));
			_query.AddParentItemsAsync(fromItemDng).ConfigureAwait(false);
			
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2"}, 
				new List<string>{fromItemJpg, fromItemDng});

			// only say: fromItemJpg > toItemJpg
			var renameFs = new RenameService(_query, iStorage)
				.Rename(fromItemJpg, toItemJpg)
				.Where(p => p.Status != FileIndexItem.ExifStatus.Deleted).ToList();

			// it has moved the files
			Assert.IsFalse(iStorage.ExistFile(fromItemJpg));
			Assert.IsFalse(iStorage.ExistFile(fromItemDng));
			
			Assert.IsTrue(iStorage.ExistFile(toItemJpg));
			Assert.IsTrue(iStorage.ExistFile(toItemDng));
			
			// and the result is ok
			Assert.AreEqual(toItemJpg, renameFs[0].FilePath);
			Assert.AreEqual(toItemDng, renameFs[1].FilePath);

			// and the database is ok
			Assert.AreEqual(toItemJpg, 
				_query.SingleItem(toItemJpg).FileIndexItem.FilePath);
			Assert.AreEqual(toItemDng, 
				_query.SingleItem(toItemDng).FileIndexItem.FilePath);	
		}
		
		[TestMethod]
		public void InputOutputSubPathsPreflight_FileToDeleted_SingleItem_WithCollectionsEnabled()
		{
			var itemInChildFolderPath1 = "/child_folder/test_22.jpg";
			var collectionItemPath1 = "/child_folder/test_22.dng";

			_query.AddItem(new FileIndexItem(itemInChildFolderPath1));
			_query.AddItem(new FileIndexItem(collectionItemPath1));

			_query.AddParentItemsAsync(itemInChildFolderPath1).ConfigureAwait(false);
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2"}, 
				new List<string>{itemInChildFolderPath1, collectionItemPath1});

			var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) = new RenameService(_query, iStorage)
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1}", 
					"/child_folder2/test_22_edit.jpg", true);
			
			Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
			Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);

			Assert.AreEqual("/child_folder2/test_22_edit.jpg", toFileSubPaths[0]);
			Assert.AreEqual("/child_folder2/test_22_edit.dng", toFileSubPaths[1]);
			
			Assert.AreEqual(0, fileIndexResultsList.Count );

			// this does only preflight
			
			_query.RemoveItem(_query.SingleItem(itemInChildFolderPath1).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(collectionItemPath1).FileIndexItem);
		}
		
		[TestMethod]
		public void InputOutputSubPathsPreflight_FileToDeleted_SingleItem_Change_FileName_And_Extension_WithCollections()
		{
			var itemInChildFolderPath1 = "/child_folder/test_23.jpg";
			var collectionItemPath1 = "/child_folder/test_23.dng";

			_query.AddItem(new FileIndexItem(itemInChildFolderPath1));
			_query.AddItem(new FileIndexItem(collectionItemPath1));

			_query.AddParentItemsAsync(itemInChildFolderPath1).ConfigureAwait(false);
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2"}, 
				new List<string>{itemInChildFolderPath1, collectionItemPath1});

			var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) = new RenameService(_query, iStorage)
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1}", 
					// Change to .jpeg
					"/child_folder2/test_23_edit.jpeg", true);
			
			Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
			Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);

			Assert.AreEqual("/child_folder2/test_23_edit.jpeg", toFileSubPaths[0]);
			Assert.AreEqual("/child_folder2/test_23_edit.dng", toFileSubPaths[1]);
			
			Assert.AreEqual(0, fileIndexResultsList.Count );

			// this does only preflight
			_query.RemoveItem(_query.SingleItem(itemInChildFolderPath1).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(collectionItemPath1).FileIndexItem);
		}
		
		[TestMethod]
		public void InputOutputSubPathsPreflight_FileToDeleted_SingleItem_Change_Extension_WithCollections()
		{
			var itemInChildFolderPath1 = "/child_folder/test_24.jpg";
			var collectionItemPath1 = "/child_folder/test_24.dng";

			_query.AddItem(new FileIndexItem(itemInChildFolderPath1));
			_query.AddItem(new FileIndexItem(collectionItemPath1));

			_query.AddParentItemsAsync(itemInChildFolderPath1).ConfigureAwait(false);
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2"}, 
				new List<string>{itemInChildFolderPath1, collectionItemPath1});

			var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) = new RenameService(_query, iStorage)
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1}", 
					// Change to .jpeg
					"/child_folder2/test_24.jpeg", true);
			
			Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
			Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);

			Assert.AreEqual("/child_folder2/test_24.jpeg", toFileSubPaths[0]);
			Assert.AreEqual("/child_folder2/test_24.dng", toFileSubPaths[1]);
			
			Assert.AreEqual(0, fileIndexResultsList.Count );

			// this does only preflight
			_query.RemoveItem(_query.SingleItem(itemInChildFolderPath1).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(collectionItemPath1).FileIndexItem);
		}
		
		[TestMethod]
		public void Rename_Move_SidecarFile_ShouldMove_FileToFolder()
		{
			// var item1 = "/child_folder/test_20.jpg";
			var item1dng = "/child_folder/test_20.dng";
			var item1SideCar = "/child_folder/test_20.xmp";

			// _query.AddItem(new FileIndexItem(item1));
			_query.AddItem(new FileIndexItem(item1dng));
			_query.AddParentItemsAsync(item1dng).ConfigureAwait(false);
			
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2"}, 
				new List<string>{ item1dng, item1SideCar}); // item1

			// Move DNG to different folder
			var renameFs = new RenameService(_query, iStorage)
				.Rename(item1dng, "/child_folder2");
			
			Assert.AreEqual(item1dng.Replace("child_folder","child_folder2"),
				renameFs.FirstOrDefault().FilePath);

			// did move the side car file
			Assert.IsTrue(iStorage.ExistFile(item1SideCar.Replace("child_folder","child_folder2")));
		}
		
		[TestMethod]
		public void Rename_Move_SidecarFile_ShouldNotMove_FileToFolder_ItsAJpeg()
		{
			var item1 = "/child_folder/test_20.jpg";
			var item1SideCar = "/child_folder/test_20.xmp";

			_query.AddItem(new FileIndexItem(item1));
			_query.AddParentItemsAsync(item1).ConfigureAwait(false);
			
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2"}, 
				new List<string>{ item1, item1SideCar});

			// Move Jpg to different folder but the xmp should be ignored
			var renameFs = new RenameService(_query, iStorage)
				.Rename(item1, "/child_folder2");
			
			Assert.AreEqual(item1.Replace("child_folder","child_folder2"),
				renameFs.FirstOrDefault().FilePath);

			// it should not move the sidecar file
			Assert.IsFalse(iStorage.ExistFile(item1SideCar.Replace("child_folder","child_folder2")));
		}

		[TestMethod]
		public void InputOutputSubPathsPreflight_FileToFolder_SingleItemWithCollectionsEnabled()
		{
			var itemInChildFolderPath1 = "/child_folder/test_07.jpg";
			var collectionItemPath1 = "/child_folder/test_07.png";

			_query.AddItem(new FileIndexItem(itemInChildFolderPath1));
			_query.AddItem(new FileIndexItem(collectionItemPath1));

			_query.AddParentItemsAsync(itemInChildFolderPath1).ConfigureAwait(false);
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2"}, 
				new List<string>{itemInChildFolderPath1, collectionItemPath1});

			var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) = new RenameService(_query, iStorage)
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1}", 
					"/child_folder2", true);
			
			Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
			Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);

			Assert.AreEqual("/child_folder2", toFileSubPaths[0]);
			Assert.AreEqual("/child_folder2", toFileSubPaths[1]);
			
			Assert.AreEqual(0, fileIndexResultsList.Count );
			
			_query.RemoveItem(_query.SingleItem(itemInChildFolderPath1).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(collectionItemPath1).FileIndexItem);
		}
		
		[TestMethod]
		public void InputOutputSubPathsPreflight_FileToFolder_MultipleFiles_CollectionsTrue()
		{
			// write test that has input /test.jpg;/test2.jpg > /test;/test2 and both has 2 or 3 collection files
			// the other should be ok
			
			var itemInChildFolderPath1 = "/child_folder/test_01.jpg";
			var collectionItemPath1 = "/child_folder/test_01.png";
			
			var itemInChildFolderPath2 = "/child_folder/test_02.jpg";
			var collectionItemPath2 = "/child_folder/test_02.png";
			
			_query.AddItem(new FileIndexItem(itemInChildFolderPath1));
			_query.AddItem(new FileIndexItem(collectionItemPath1));
			_query.AddItem(new FileIndexItem(itemInChildFolderPath2));
			_query.AddItem(new FileIndexItem(collectionItemPath2));
			
			_query.AddParentItemsAsync(itemInChildFolderPath1).ConfigureAwait(false);
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2","/other"}, 
				new List<string>{itemInChildFolderPath1, collectionItemPath1, 
					itemInChildFolderPath2, collectionItemPath2});

			var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) = new RenameService(_query, iStorage)
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1};{itemInChildFolderPath2}", 
					"/child_folder2;/other", true);

			Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
			Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);
			Assert.AreEqual(itemInChildFolderPath2, inputFileSubPaths[2]);
			Assert.AreEqual(collectionItemPath2, inputFileSubPaths[3]);

			Assert.AreEqual("/child_folder2", toFileSubPaths[0]);
			Assert.AreEqual("/child_folder2", toFileSubPaths[1]);
			Assert.AreEqual("/other", toFileSubPaths[2]);
			Assert.AreEqual("/other", toFileSubPaths[3]);
			
			Assert.AreEqual(0, fileIndexResultsList.Count );

			_query.RemoveItem(_query.SingleItem(itemInChildFolderPath1).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(collectionItemPath1).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(itemInChildFolderPath2).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(collectionItemPath2).FileIndexItem);
		}
			
		[TestMethod]
		public void InputOutputSubPathsPreflight_FileToFolder_MultipleFiles_CollectionsFalse_Aka_Disabled()
		{
			// write test that has input /test.jpg;/test2.jpg > /test;/test2 and both has 2 or 3 collection files
			// But this one's are not used
			// the other should be ok
			
			var itemInChildFolderPath1 = "/child_folder/test_05.jpg";
			var collectionItemPath1 = "/child_folder/test_05.png";
			
			var itemInChildFolderPath2 = "/child_folder/test_06.jpg";
			var collectionItemPath2 = "/child_folder/test_06.png";
			
			_query.AddItem(new FileIndexItem(itemInChildFolderPath1));
			_query.AddItem(new FileIndexItem(collectionItemPath1));
			_query.AddItem(new FileIndexItem(itemInChildFolderPath2));
			_query.AddItem(new FileIndexItem(collectionItemPath2));
			
			_query.AddParentItemsAsync(itemInChildFolderPath1).ConfigureAwait(false);
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2","/other"}, 
				new List<string>{itemInChildFolderPath1, collectionItemPath1, 
					itemInChildFolderPath2, collectionItemPath2});

			// Collections disabled!
			var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) = new RenameService(_query, iStorage)
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1};{itemInChildFolderPath2}", 
					"/child_folder2;/other", false);

			Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
			Assert.AreEqual(itemInChildFolderPath2, inputFileSubPaths[1]);

			Assert.AreEqual("/child_folder2", toFileSubPaths[0]);
			Assert.AreEqual("/other", toFileSubPaths[1]);
			
			Assert.AreEqual(0, fileIndexResultsList.Count );

			_query.RemoveItem(_query.SingleItem(itemInChildFolderPath1).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(collectionItemPath1).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(itemInChildFolderPath2).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(collectionItemPath2).FileIndexItem);
		}
		
		[TestMethod]
		public void InputOutputSubPathsPreflight_FileToFolder_MultipleFiles_PartlyNotFound()
		{
			var itemInChildFolderPath1 = "/child_folder/test_03.jpg";
			var collectionItemPath1 = "/child_folder/test_03.png";
			
			var itemInChildFolderPath2 = "/child_folder/test_04.jpg";
			
			_query.AddItem(new FileIndexItem(itemInChildFolderPath1));
			_query.AddItem(new FileIndexItem(collectionItemPath1));
			
			_query.AddParentItemsAsync(itemInChildFolderPath1).ConfigureAwait(false);
			
			var iStorage = new FakeIStorage(new List<string>{"/","/child_folder","/child_folder2","/other"}, 
				new List<string>{itemInChildFolderPath1, collectionItemPath1});

			// nr 2 is does not exist in the database
			var ((inputFileSubPaths, toFileSubPaths), fileIndexResultsList) = new RenameService(_query, iStorage)
				.InputOutputSubPathsPreflight($"{itemInChildFolderPath1};{itemInChildFolderPath2}", 
					"/child_folder2;/other", true);

			Assert.AreEqual(itemInChildFolderPath1, inputFileSubPaths[0]);
			Assert.AreEqual(collectionItemPath1, inputFileSubPaths[1]);

			Assert.AreEqual("/child_folder2", toFileSubPaths[0]);
			Assert.AreEqual("/child_folder2", toFileSubPaths[1]);
			
			Assert.AreEqual(1, fileIndexResultsList.Count );
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, fileIndexResultsList[0].Status );

			_query.RemoveItem(_query.SingleItem(itemInChildFolderPath1).FileIndexItem);
			_query.RemoveItem(_query.SingleItem(collectionItemPath1).FileIndexItem);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void FromFolderToFolder_Null_exception()
		{
			new RenameService(null, null).FromFolderToFolder(null, null, null);
			// expect exception
		}

		// todo: add this RenameFsTest_MergeToLowerPath
	}
}

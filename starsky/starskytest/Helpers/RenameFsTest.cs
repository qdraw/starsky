using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
using starskycore.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;
using Query = starskycore.Services.Query;
using SyncService = starskycore.Services.SyncService;

namespace starskytest.Helpers
{
	[TestClass]
	public class RenameFsTest
	{
		private readonly Query _query;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _newImage;
		private readonly SyncService _sync;
		private readonly StorageSubPathFilesystem _iStorageSubPath;

		public RenameFsTest()
		{
			var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();
			
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("test");
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
			var serviceProvider = services.BuildServiceProvider();
			var selectorStorage = new FakeSelectorStorage(iStorage);

			_sync = new SyncService(_query,_appSettings,selectorStorage);
		}

		[TestMethod]
		public void RenameFsTest_DuplicateFile()
		{
			// Default is skip 
			var fileAlreadyExist = Path.Join(_newImage.BasePath, "already.txt");
			if(!File.Exists(fileAlreadyExist)) new PlainTextFileHelper().WriteFile(fileAlreadyExist,"test");
			var renameFs = new RenameFs( _query,_sync,_iStorageSubPath).Rename(_newImage.DbPath, "/already.txt");
			Assert.AreEqual(new PlainTextFileHelper().ReadFile(fileAlreadyExist).Contains("test"), true);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, renameFs.FirstOrDefault().Status );

			// test with newline at the end
			FilesHelper.DeleteFile(fileAlreadyExist);
		}

		[TestMethod]
		public void RenameFsTest_MoveFileWithoutAnyItems()
		{
			var renameFs = new RenameFs(_query,_sync,_iStorageSubPath).Rename("/non-exist.jpg", "/non-exist2.jpg");
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, renameFs.FirstOrDefault().Status );
		}
		

		[TestMethod]
		public void RenameFsTest_MoveFileToExistFolder_Items()
		{
			// remove file if already exist; we are not testing duplicate support here
			var existFullPath = Path.Combine(_newImage.BasePath, "exist");
			if ( File.Exists(Path.Combine(existFullPath, "test2.jpg")) )
			{
				File.Delete(Path.Combine(existFullPath, "test2.jpg"));
			}
			
			// check if dir exist
			if (!System.IO.Directory.Exists(existFullPath) )
			{
				System.IO.Directory.CreateDirectory(existFullPath);
			}
			
			
			var renameFs = new RenameFs(_query,_sync,_iStorageSubPath).Rename(_newImage.DbPath, "/exist/test2.jpg");

			Assert.AreEqual(1,renameFs.Count);
			
			// query database
			var all = _query.GetAllRecursive();
			Assert.AreEqual(all.FirstOrDefault(p => p.FileName == "test2.jpg").FileName, "test2.jpg");
			
			
			// use cached view
			var singleItem = _query.SingleItem("/exist/test2.jpg");
			Assert.AreEqual("test2.jpg",singleItem.FileIndexItem.FileName);		
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFs.FirstOrDefault().Status );

			new StorageHostFullPathFilesystem().FolderDelete(Path.Combine(_newImage.BasePath, "exist"));
		}

	
		[TestMethod]
		public void RenameFsTest_MoveDirWithItemsTest()
		{
			var existFullDirPath = Path.Combine(_newImage.BasePath, "dir1");
			System.IO.Directory.CreateDirectory(existFullDirPath);
			// move an item to this directory	
			
			if (! File.Exists(_appSettings.DatabasePathToFilePath("/dir1/test3.jpg")) )
			{
				File.Move(_newImage.FullFilePath,_appSettings.DatabasePathToFilePath("/dir1/test3.jpg",false));
			}
			_sync.SingleFile("/dir1/test3.jpg");
			
			// query database
			var all = _query.GetAllRecursive();
			Assert.AreEqual(all.FirstOrDefault(p => p.FileName == "test3.jpg").FileName, "test3.jpg");
			
			
			var renameFs = new RenameFs(_query,_sync,_iStorageSubPath).Rename("/dir1", "/dir2");
			// check if files are moved in the database

			var all2 = _query.GetAllRecursive();

			var selectFile3 = all2.FirstOrDefault(p => p.FileName == "test3.jpg");
			Assert.AreEqual("test3.jpg",selectFile3.FileName);
			Assert.AreEqual("/dir2",selectFile3.ParentDirectory);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFs.FirstOrDefault().Status );
			
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

			var iStorage = new FakeIStorage(new List<string>{_folderExist.FilePath},new List<string>{_fileInExist.FilePath});
			
			var renameFs = new RenameFs(_query,_sync,iStorage).Rename( _fileInExist.FilePath, _folderExist.FilePath+ "/test2.jpg");
			
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
			
			var renameFs = new RenameFs(_query,_sync,iStorage).Rename( _fileInExist.FilePath, _folderExist.FilePath + "/test2___");
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
			var renameFs = new RenameFs(_query, _sync, istorage).Rename(initFileList.FirstOrDefault(), "/nonExist/test5.jpg", true);
			
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
			var renameFs = new RenameFs(_query, _sync, istorage).Rename(initFileList.FirstOrDefault(), "/exist/", true);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, renameFs.FirstOrDefault().Status );

			RemoveFoldersAndFilesInDatabase();
		}
		
		
		[TestMethod]
		public void RenameFsTest_FakeIStorage_File_To_ExistFolder()
		{
			CreateFoldersAndFilesInDatabase();

			var initFolderList =  new List<string> { "/", "/test" };
			var initFileList = new List<string> { _fileInExist.FilePath };
			var istorage = new FakeIStorage(initFolderList,initFileList);
			var renameFs = new RenameFs(_query, _sync, istorage).Rename(initFileList.FirstOrDefault(), "/test/", true);
			
			// to file: (in database)
			var all2 = _query.GetAllRecursive();
			var selectFile3 = all2.FirstOrDefault(p => p.FileName == "file.jpg");
			Assert.AreEqual("file.jpg",selectFile3.FileName);
			Assert.AreEqual("/test",selectFile3.ParentDirectory);

			// check if files are moved (on fake Filesystem)
			var values = istorage.GetAllFilesInDirectory("/test").ToList();
			Assert.AreEqual("/test/file.jpg", values.FirstOrDefault(p => p == "/test/file.jpg"));
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
			var renameFs = new RenameFs(_query, _sync, istorage).Rename("/exist", "/folder1", true);
			
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

			Assert.AreEqual("/folder1/file.jpg",all2.FirstOrDefault(p => p.FileName == "file.jpg").FilePath);
			Assert.AreEqual("/folder1/subfolder",all2.FirstOrDefault(p => p.FileName == "subfolder").FilePath);
			Assert.AreEqual("/folder1/subfolder/child.jpg",all2.FirstOrDefault(p => p.FileName == "child.jpg").FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.Ok, renameFs.FirstOrDefault().Status );

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
			var renameFs = new RenameFs(_query, _sync, istorage).Rename("/same", "/same");
			Assert.AreEqual(1,renameFs.Count);
			Assert.AreEqual(FileIndexItem.ExifStatus.OperationNotSupported, renameFs.FirstOrDefault().Status);
		}

		[TestMethod]
		public void RenameFsTest_FakeIStorage_UnderstandTest()
		{
			// used to test the GetAllFilesInDirectory() fake class
			var initFolderList =  new List<string> {"/", "/test/subfolder", "/test", "/otherfolder" };
			var initFileList = new List<string> { "/test/test.jpg", "/test/subfolder/t.jpg", "/test/subfolder/child.jpg" };
			var istorage = new FakeIStorage(initFolderList,initFileList).GetAllFilesInDirectory("/test").ToList();
			Assert.AreEqual(1,istorage.Count);
		}

		[TestMethod]
		public void RenameFsTest_MoveAFolderIntoAFile()
		{
			CreateFoldersAndFilesInDatabase();
			var istorage = new FakeIStorage();
			var renameFs = new RenameFs(_query, _sync, istorage).Rename(_folderExist.FilePath, _fileInExist.FilePath);
			Assert.AreEqual(FileIndexItem.ExifStatus.NotFoundNotInIndex, renameFs[0].Status);
		}

		[TestMethod]
		public void RenameFsTest_MergeToLowerPath()
		{
			// todo: add this
		}
	}
}

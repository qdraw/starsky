using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Data;
using starskycore.Helpers;
using starskycore.Models;
using starskycore.Services;
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
		private StorageFilesystem _iStorage;

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
			
			var readMeta = new ReadMeta(_appSettings,memoryCache);
			
			_iStorage = new StorageFilesystem(_appSettings);

			_sync = new SyncService(context,_query,_appSettings,readMeta,_iStorage);
			

		}

		[TestMethod]
		public void RenameFsTest_DuplicateFile()
		{
			// Default is skip 
			var fileAlreadyExist = Path.Join(_newImage.BasePath, "already.txt");
			if(!File.Exists(fileAlreadyExist)) new PlainTextFileHelper().WriteFile(fileAlreadyExist,"test");
			var renameFs = new RenameFs(_appSettings, _query,_sync,_iStorage).Rename(_newImage.DbPath, "/already.txt");
			Assert.AreEqual(new PlainTextFileHelper().ReadFile(fileAlreadyExist).Contains("test"), true);
			// test with newline at the end
			FilesHelper.DeleteFile(fileAlreadyExist);
		}

		[TestMethod]
		public void RenameFsTest_MoveFileWithoutAnyItems()
		{
			var renameFs = new RenameFs(_appSettings, _query,_sync,_iStorage).Rename("/non-exist.jpg", "/non-exist2.jpg");
			Assert.AreEqual(renameFs.FirstOrDefault().Status,FileIndexItem.ExifStatus.NotFoundNotInIndex);
		}
		
		[TestMethod]
		public void RenameFsTest_MoveFileToSameFolder_Items()
		{
			// remove file if already exist; we are not testing duplicate support here
			if ( File.Exists(Path.Combine(_newImage.BasePath, "test2.jpg")) )
			{
				File.Delete(Path.Combine(_newImage.BasePath, "test2.jpg"));
			}
			
			var renameFs = new RenameFs(_appSettings, _query,_sync,_iStorage).Rename(_newImage.DbPath, "/test2.jpg");
			
			// query database
			var all = _query.GetAllRecursive();
			Assert.AreEqual(all.FirstOrDefault(p => p.FileName == "test2.jpg").FileName, "test2.jpg");

			// use cached view
			var singleItem = _query.SingleItem("/test2.jpg");
			Assert.AreEqual("test2.jpg",singleItem.FileIndexItem.FileName);			
			
			File.Delete(Path.Combine(_newImage.BasePath, "test2.jpg"));

			Assert.AreEqual(1,renameFs.Count);
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
			
			
			var renameFs = new RenameFs(_appSettings, _query,_sync,_iStorage).Rename(_newImage.DbPath, "/exist/test2.jpg");

			Assert.AreEqual(1,renameFs.Count);
			
			// query database
			var all = _query.GetAllRecursive();
			Assert.AreEqual(all.FirstOrDefault(p => p.FileName == "test2.jpg").FileName, "test2.jpg");
			
			
			// use cached view
			var singleItem = _query.SingleItem("/exist/test2.jpg");
			Assert.AreEqual("test2.jpg",singleItem.FileIndexItem.FileName);		
			
			FilesHelper.DeleteDirectory(Path.Combine(_newImage.BasePath, "exist"));
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
			
			
			var renameFs = new RenameFs(_appSettings, _query,_sync,_iStorage).Rename("/dir1", "/dir2");
			// check if files are moved in the database

			var all2 = _query.GetAllRecursive();

			var selectFile3 = all2.FirstOrDefault(p => p.FileName == "test3.jpg");
			Assert.AreEqual("test3.jpg",selectFile3.FileName);
			Assert.AreEqual("/dir2",selectFile3.ParentDirectory);
			
			var dir2FullDirPath = Path.Combine(_newImage.BasePath, "dir2");

			FilesHelper.DeleteDirectory(dir2FullDirPath);
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
		public void RenameFsTest_ToNonExistFolder_Items()
		{
			CreateFoldersAndFilesInDatabase();

			var initFolderList =  new List<string> { "/" };
			var initFileList = new List<string> { _fileInExist.FilePath };
			var istorage = new FakeIStorage(initFolderList,initFileList);
			var renameFs = new RenameFs(_appSettings, _query, _sync, istorage).Rename(initFileList.FirstOrDefault(), "/nonExist/test5.jpg", true);
			
			var all2 = _query.GetAllRecursive();
			var selectFile3 = all2.FirstOrDefault(p => p.FileName == "test5.jpg");
			Assert.AreEqual("test5.jpg",selectFile3.FileName);
			Assert.AreEqual("/nonExist",selectFile3.ParentDirectory);

			// check if files are moved
			var values = istorage.GetAllFilesInDirectory("/nonExist").ToList();
			Assert.AreEqual("/nonExist/test5.jpg", values.FirstOrDefault(p => p == "/nonExist/test5.jpg"));
			
			
			RemoveFoldersAndFilesInDatabase();
		}
		
		[TestMethod]
		public void RenameFsTest_mergeTwoFolders()
		{

			
			CreateFoldersAndFilesInDatabase();

			var initFolderList =  new List<string> { "/", _folder1Exist.FilePath + "/subfolder", _folder1Exist.FilePath, _folderExist.FilePath };
			var initFileList = new List<string> { _fileInExist.FilePath, _folder1Exist.FilePath + "/subfolder/child.jpg" };
			var istorage = new FakeIStorage(initFolderList,initFileList);
			var renameFs = new RenameFs(_appSettings, _query, _sync, istorage).Rename("/folder1", "/exist", true);
			// todo: incomplete!!!!!!

			var t = istorage.GetDirectoryRecursive("/").ToList();
			RemoveFoldersAndFilesInDatabase();


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

	}
}

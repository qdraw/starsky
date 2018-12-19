using System;
using System.IO;
using System.Linq;
using MetadataExtractor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;
using starsky.Helpers;
using starsky.Interfaces;
using starsky.Models;
using starsky.Services;

namespace starskytests.Helpers
{
	[TestClass]
	public class RenameFsTest
	{
		private readonly Query _query;
		private readonly AppSettings _appSettings;
		private readonly CreateAnImage _newImage;
		private readonly SyncService _sync;

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
				StorageFolder = _newImage.BasePath,
				ThumbnailTempFolder = _newImage.BasePath
			};
			_query = new Query(context,memoryCache, _appSettings);
			_query.AddItem(new FileIndexItem
			{
				FileName = _newImage.FileName,
				ParentDirectory = "/",
				AddToDatabase = DateTime.UtcNow
			});
			
			var readMeta = new ReadMeta(_appSettings,memoryCache);
			
			_sync = new SyncService(context,_query,_appSettings,readMeta);

		}

		[TestMethod]
		public void RenameFsTest_DuplicateFile()
		{
			Assert.AreEqual(null,1);
		}

		[TestMethod]
		public void RenameFsTest_MoveFileWithoutAnyItems()
		{
			var renameFs = new RenameFs(_appSettings, _query,_sync).Rename("/non-exist.jpg", "/non-exist2.jpg");
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
			
			var renameFs = new RenameFs(_appSettings, _query,_sync).Rename(_newImage.DbPath, "/test2.jpg");
			
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
			
			
			var renameFs = new RenameFs(_appSettings, _query,_sync).Rename(_newImage.DbPath, "/exist/test2.jpg");

			Assert.AreEqual(1,renameFs.Count);
			
			// query database
			var all = _query.GetAllRecursive();
			Assert.AreEqual(all.FirstOrDefault(p => p.FileName == "test2.jpg").FileName, "test2.jpg");
			
			
			// use cached view
			var singleItem = _query.SingleItem("/exist/test2.jpg");
			Assert.AreEqual("test2.jpg",singleItem.FileIndexItem.FileName);		
			
			Files.DeleteDirectory(Path.Combine(_newImage.BasePath, "exist"));
		}

		[TestMethod]
		[ExpectedException(typeof(DirectoryNotFoundException))]
		public void RenameFsTest_ToNonExistFolder_Items_DirectoryNotFoundException()
		{
			var renameFs = new RenameFs(_appSettings, _query,_sync).Rename(_newImage.DbPath, "/nonExist/test2.jpg",true,false);
		}
		
		
		[TestMethod]
		public void RenameFsTest_MoveDirWithItemsTest()
		{
			var existFullDirPath = Path.Combine(_newImage.BasePath, "dir1");
			System.IO.Directory.CreateDirectory(existFullDirPath);
			// move an item to this directory			
			var renameFs = new RenameFs(_appSettings, _query,_sync).Rename(_newImage.DbPath, "/dir1/test2.jpg");
			// there is one file moved
			Assert.AreEqual(1,renameFs.Count);
			

			
			renameFs = new RenameFs(_appSettings, _query,_sync).Rename("/dir1", "/dir2");
			// check if files are moved in the database

			var all = _query.GetAllRecursive();
			
			
			Files.DeleteDirectory(existFullDirPath);
		}
		
		
		

//		[TestMethod]
//		public void RenameFsTest_ToNonExistFolder_Items()
//		{
//			var renameFs = new RenameFs(_appSettings, _query).Rename(_newImage.DbPath, "/nonExist/test2.jpg", true, false);
//		}
	}
}

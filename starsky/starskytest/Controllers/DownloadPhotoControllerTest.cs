using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starskycore.Data;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskycore.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class DownloadPhotoControllerTest
	{
		private readonly IQuery _query;

		public DownloadPhotoControllerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			var memoryCache = provider.GetService<IMemoryCache>();

			var builderDb = new DbContextOptionsBuilder<ApplicationDbContext>();
			builderDb.UseInMemoryDatabase(nameof(DownloadPhotoControllerTest));
			var options = builderDb.Options;
			var context = new ApplicationDbContext(options);
			_query = new Query(context, memoryCache);
		}

		private FileIndexItem InsertSearchData()
		{
			var item = new FileIndexItem
			{
				FileName = "test.jpg",
				ParentDirectory = "/",
				FileHash = "home0012304590",
				ColorClass = FileIndexItem.Color.Winner // 1
			};

			if ( string.IsNullOrEmpty(_query.GetSubPathByHash("home0012304590")) )
			{
				_query.AddItem(item);
			}
			return item;
		}
		
		[TestMethod]
		public async Task DownloadPhoto_isThumbnailTrue_CreateThumb_ReturnFileStream_Test()
		{
			// Arange
			var fileIndexItem = InsertSearchData();
			var folderPaths = new List<string>{"/"};
			var inputSubPaths = new List<string>{"/test.jpg"};
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, 
					new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			var selectorStorage = new FakeSelectorStorage(storage);

			// Act
			var controller = new DownloadPhotoController(_query,selectorStorage);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var actionResult = await controller.DownloadPhoto(fileIndexItem.FilePath) as FileStreamResult;
			Assert.AreNotEqual(null,actionResult);

			actionResult.FileStream.Dispose();
		}
		
		// [TestMethod]
  //       public void ApiController_DownloadPhoto_isThumbnailFalse_ReturnFileStream_Test()
  //       {
  //           // Arange
  //           var fileIndexItem = InsertSearchData();
  //
  //           // Remove thumb if exist
  //           var thumbPath = new CreateAnImage().BasePath + 
  //                           Path.DirectorySeparatorChar + 
  //                           fileIndexItem.FileHash + ".jpg";
  //           
  //
  //           if (File.Exists(thumbPath))
  //           {
  //               File.Delete(thumbPath);
  //           }
  //           var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings));
  //
  //           // Act
  //           var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,selectorStorage,null);
  //           controller.ControllerContext.HttpContext = new DefaultHttpContext();
  //           var actionResult =  controller.DownloadPhoto(fileIndexItem.FilePath,false)  as FileStreamResult;
  //           Assert.AreNotEqual(null,actionResult);
  //
  //           actionResult.FileStream.Dispose();
  //
  //
  //           if (File.Exists(thumbPath))
  //           {
  //               File.Delete(thumbPath);
  //           }
  //       }
  //       
  //       [TestMethod]
  //       public void ApiController_DownloadPhoto_isThumbnailTrue_ReturnAThumb_ReturnFileStream_Test()
  //       {
  //           // Arange
  //           var fileIndexItem = InsertSearchData();
  //
  //           // Remove thumb if exist
  //           var thumbPath = new CreateAnImage().BasePath + 
  //                           Path.DirectorySeparatorChar + fileIndexItem.FileHash +
  //                           ".jpg";
  //           
  //           if (File.Exists(thumbPath))
  //           {
  //               File.Delete(thumbPath);
  //           }
  //           var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings));
  //
  //           // Act
  //           var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,selectorStorage,null);
  //           controller.ControllerContext.HttpContext = new DefaultHttpContext();
  //
  //           // Run once
  //           var actionResult1 = controller.DownloadPhoto(fileIndexItem.FilePath) as FileStreamResult;
  //           actionResult1.FileStream.Dispose();
  //
  //
  //           // Run twice
  //           var actionResult2 =  controller.DownloadPhoto(fileIndexItem.FilePath)  as FileStreamResult;
  //           Assert.AreNotEqual(null,actionResult2);
  //
  //           actionResult2.FileStream.Dispose();
  //
  //           // Clean
  //           if (File.Exists(thumbPath))
  //           {
  //               File.Delete(thumbPath);
  //           }
  //       }
  //
  //       [TestMethod]
  //       public void ApiController_DownloadPhoto_SourceImageIsMissing_Test()
  //       {
  //           var thumbHash = "ApiController_Thumbnail_CorruptImage_Test";
  //           var path = _createAnImage.BasePath + Path.DirectorySeparatorChar + thumbHash + ".jpg";
  //          
  //
  //           _query.AddItem(new FileIndexItem
  //           {
  //               FileName = "ApiController_Thumbnail_CorruptImage_Test.jpg",
  //               ParentDirectory = "/",
  //               FileHash = thumbHash
  //           });
  //           var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings));
  //
  //           // Act
  //           var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,selectorStorage,null);
  //           var actionResult =  controller.DownloadPhoto("/" + thumbHash)  as NotFoundObjectResult;
  //           Assert.AreNotEqual(null,actionResult);
  //           Assert.AreEqual(404,actionResult.StatusCode);
  //
  //           
  //           // remove files + database item
  //           _query.RemoveItem(_query.GetObjectByFilePath("/" + thumbHash + ".jpg"));
  //           if (File.Exists(path))
  //           {
  //               File.Delete(path);
  //           }
  //           
  //       }
  //       
  //       
  //       [TestMethod]
  //       public void ApiController_DownloadPhoto_Thumb_base_folder_not_found_Test()
  //       {
  //           // this works
  //           
  //           // Arange
  //           var fileIndexItem = InsertSearchData();
  //           
  //           var appSettingsthumbtest = _appSettings;
  //           appSettingsthumbtest.ThumbnailTempFolder = null;
  //           var selectorStorage = new FakeSelectorStorage(new StorageSubPathFilesystem(_appSettings));
  //
  //           // Act
  //           var controller = new ApiController(_query,_exifTool,_appSettings,_bgTaskQueue,selectorStorage,null);
  //           controller.ControllerContext.HttpContext = new DefaultHttpContext();
  //           
  //           // Run once
  //           var actionResult = controller.DownloadPhoto(fileIndexItem.FilePath) as NotFoundObjectResult;
  //           Assert.AreNotEqual(null,actionResult);
  //           Assert.AreEqual(404,actionResult.StatusCode);
  //
  //       }

	}
}

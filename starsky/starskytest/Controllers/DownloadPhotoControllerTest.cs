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

		private IStorage ArrangeStorage()
		{
			var folderPaths = new List<string>{"/"};
			var inputSubPaths = new List<string>{"/test.jpg"};
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, 
					new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			return storage;
		}
		
		[TestMethod]
		public async Task DownloadPhoto_isThumbnailTrue_CreateThumb_ReturnFileStream_Test()
		{
			// Arrange
			var fileIndexItem = InsertSearchData();
			var selectorStorage = new FakeSelectorStorage(ArrangeStorage());
			
			// Act
			var controller = new DownloadPhotoController(_query,selectorStorage);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var actionResult = await controller.DownloadPhoto(fileIndexItem.FilePath) as FileStreamResult;
			Assert.AreNotEqual(null,actionResult);

			actionResult.FileStream.Dispose();
		}
		
		[TestMethod]
        public async Task DownloadPhoto_isThumbnailFalse_ReturnFileStream_Test()
        {
	        // Arrange
	        var fileIndexItem = InsertSearchData();
	        var selectorStorage = new FakeSelectorStorage(ArrangeStorage());
	        
            // Act
            var controller = new DownloadPhotoController(_query,selectorStorage);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            var actionResult =  await controller.DownloadPhoto(fileIndexItem.FilePath,false)  as FileStreamResult;
            Assert.AreNotEqual(null,actionResult);
  
            actionResult.FileStream.Dispose();
        }

		[TestMethod]
		public async Task DownloadPhoto_isThumbnailTrue_ReturnAThumb_ReturnFileStream_Test()
		{
			// Arrange
			var fileIndexItem = InsertSearchData();
			var selectorStorage = new FakeSelectorStorage(ArrangeStorage());

			// Act
			var controller = new DownloadPhotoController(_query,selectorStorage);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			// Run once
			var actionResult1 = await controller.DownloadPhoto(fileIndexItem.FilePath) as FileStreamResult;
			actionResult1.FileStream.Dispose();

			// Run twice
			var actionResult2 =  await controller.DownloadPhoto(fileIndexItem.FilePath)  as FileStreamResult;
			Assert.AreNotEqual(null,actionResult2);

			actionResult2.FileStream.Dispose();
		}
		
		[TestMethod]
		public async Task ApiController_DownloadPhoto_SourceImageIsMissing_Test()
		{
			// Arrange
			var fileIndexItem = InsertSearchData();
			
			// so the item does not exist on disk
			var storage = ArrangeStorage();
			var selectorStorage = new FakeSelectorStorage();
			
			// Act
			var controller = new DownloadPhotoController(_query,selectorStorage);
			var actionResult =  await controller.DownloadPhoto(fileIndexItem.FilePath)  as NotFoundObjectResult;
			Assert.AreNotEqual(null,actionResult);
			Assert.AreEqual(404,actionResult.StatusCode);
			Assert.AreEqual("source image missing /test.jpg",actionResult.Value);
		}

		[TestMethod]
		public async Task DownloadPhoto_Thumb_base_folder_not_found_Test()
		{
			// Arrange
			var fileIndexItem = InsertSearchData();
			var storage =
				new FakeIStorage(null, new List<string>{"/test.jpg"}, 
					new List<byte[]>{FakeCreateAn.CreateAnImage.Bytes});
			var selectorStorage = new FakeSelectorStorage(storage);
			

			// Act
			var controller = new DownloadPhotoController(_query,selectorStorage);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var actionResult =  await controller.DownloadPhoto(fileIndexItem.FilePath)  as NotFoundObjectResult;
		
			Assert.AreNotEqual(null,actionResult);
			Assert.AreEqual(404,actionResult.StatusCode);
			Assert.AreEqual("ThumbnailTempFolder not found",actionResult.Value);
		}
	}
}

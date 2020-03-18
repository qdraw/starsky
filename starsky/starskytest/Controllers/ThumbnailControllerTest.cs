using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.query.Interfaces;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class ThumbnailControllerTest
	{
		private readonly IQuery _query;

		public ThumbnailControllerTest()
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
				ColorClass = ColorClassParser.Color.Winner // 1
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
			var inputSubPaths = new List<string>{"/test.jpg","/test2.jpg"};
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, 
					new List<byte[]>{CreateAnImage.Bytes,CreateAnImage.Bytes});
			return storage;
		}
		
		[TestMethod]
		public void Thumbnail_InputBadRequest()
		{
			var storageSelector = new FakeSelectorStorage(ArrangeStorage());
			
			var controller = new ThumbnailController(_query,storageSelector);;
			var actionResult = controller.Thumbnail("../") as BadRequestResult;
			Assert.AreEqual(400,actionResult.StatusCode);
		}

		[TestMethod]
		public void Thumbnail_CorruptImage_NoContentResult_Test()
		{
			// Arrange
			var storage = ArrangeStorage();
			var plainTextStream = new PlainTextFileHelper().StringToStream("CorruptImage");
			storage.WriteStream(plainTextStream, "/hash-corrupt-image");

			_query.AddItem(new FileIndexItem("/test2.jpg"){FileHash= "hash-corrupt-image"});

			// Act
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
  
			var actionResult = controller.Thumbnail("hash-corrupt-image", false, true) as NoContentResult;
			Assert.AreEqual(204,actionResult.StatusCode);
               
			// remove files + database item
			_query.RemoveItem(_query.GetObjectByFilePath("/test2.jpg"));
		}
		
		[TestMethod]
		public void Thumbnail_NonExistingFile_API_Test()
		{
			var controller = new ThumbnailController(_query,new FakeSelectorStorage());;
			var actionResult = controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
			var thumbnailAnswer = actionResult.StatusCode;
			Assert.AreEqual(404,thumbnailAnswer);
		}
		
		[TestMethod]
		public void Thumbnail_HappyFlowDisplayJson_API_Test()
		{
			// Arrange
			var storage = ArrangeStorage();
			var createAnImage = InsertSearchData();

			// Act
			// Create thumbnail in fake storage
			new Thumbnail(storage,storage).CreateThumb(createAnImage.FilePath, createAnImage.FileHash);
			
			// Check if exist
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			var actionResult = controller.Thumbnail(createAnImage.FileHash,true,true) as JsonResult;
			
			// Thumbnail exist
			Assert.AreNotEqual(actionResult,null);
			var thumbnailAnswer = actionResult.Value as string;
			Assert.AreEqual("OK",thumbnailAnswer);
		}
	  
		[TestMethod]
		public void Thumbnail_HappyFlowFileStreamResult_API_Test()
		{
			// Arrange
			var storage = ArrangeStorage();
			var createAnImage = InsertSearchData();

			// Act
			// Create thumbnail in fake storage
			new Thumbnail(storage,storage).CreateThumb(createAnImage.FilePath, createAnImage.FileHash);
			
			// Check if exist
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			var actionResult = controller.Thumbnail(createAnImage.FileHash, true) as FileStreamResult;
			
			// Thumbnail exist
			Assert.AreNotEqual(actionResult,null);
			var thumbnailAnswer = actionResult.ContentType;
			
			Assert.AreEqual(createAnImage.FileHash + ".jpg", actionResult.FileDownloadName);
			Assert.AreEqual("image/jpeg",thumbnailAnswer);
			actionResult.FileStream.Dispose(); // for windows
		}
		
		[TestMethod]
		public void Thumbnail_ShowOriginalImage_API_Test()
		{
			var createAnImage = InsertSearchData();
			var storage = ArrangeStorage();

			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.Thumbnail(createAnImage.FileHash, true) as FileStreamResult;
			var thumbnailAnswer = actionResult.ContentType;
			
			Assert.AreEqual("test.jpg", actionResult.FileDownloadName);
			Assert.AreEqual("image/jpeg",thumbnailAnswer);
			
			actionResult.FileStream.Dispose(); // for windows
		}

		[TestMethod]
		public void Thumbnail_IsMissing_ButOriginalExist_butNoIsSingleItemFlag_API_Test()
		{
			// Photo exist in database but " + "isSingleItem flag is Missing
			var createAnImage = InsertSearchData();
			var storage = ArrangeStorage();

			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.Thumbnail(createAnImage.FileHash, false, true) as JsonResult;
			var thumbnailAnswer = actionResult.StatusCode; // always null for some reason ?!
			Assert.AreEqual("Thumbnail is not ready yet",actionResult.Value);
		}

        [TestMethod]
        public void ApiController_FloatingDatabaseFileTest_API_Test()
        {
            var item = _query.AddItem(new FileIndexItem
            {
                ParentDirectory = "/fakeImage/",
                FileName = "fake.jpg",
                FileHash = "0986524678765456786543",
				Id= 788,
            });
            
            var storage = ArrangeStorage();
            
            var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
            var actionResult = controller.Thumbnail(item.FileHash, false, true) as NotFoundObjectResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreEqual(404,thumbnailAnswer);
            _query.RemoveItem(item);
        }

		[TestMethod]
		public void ApiController_Thumbnail1_NonExistingFile_API_Test()
		{
			var storage = ArrangeStorage();
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			var actionResult = controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
			var thumbnailAnswer = actionResult.StatusCode;
			Assert.AreEqual(404,thumbnailAnswer);
		}
	}
}

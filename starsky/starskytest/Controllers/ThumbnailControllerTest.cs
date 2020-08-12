using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
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
			var fileHash = "home0012304590";
			var item = new FileIndexItem
			{
				FileName = "test.jpg",
				ParentDirectory = "/",
				FileHash = fileHash,
				ColorClass = ColorClassParser.Color.Winner // 1
			};

			if ( string.IsNullOrEmpty(_query.GetSubPathByHash(fileHash)) )
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
		public async Task Thumbnail_InputBadRequest()
		{
			var storageSelector = new FakeSelectorStorage(ArrangeStorage());
			
			var controller = new ThumbnailController(_query,storageSelector);;
			var actionResult = await controller.Thumbnail("../") as BadRequestResult;
			Assert.AreEqual(400,actionResult.StatusCode);
		}

		[TestMethod]
		public async Task Thumbnail_CorruptImage_NoContentResult_Test()
		{
			// Arrange
			var storage = ArrangeStorage();
			var plainTextStream = new PlainTextFileHelper().StringToStream("CorruptImage");
			await storage.WriteStreamAsync(plainTextStream, "hash-corrupt-image");

			await _query.AddItemAsync(new FileIndexItem("/test2.jpg"){FileHash= "hash-corrupt-image"});

			// Act
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
  
			var actionResult = await controller.Thumbnail("hash-corrupt-image", false, true) as NoContentResult;
			Assert.AreEqual(204,actionResult.StatusCode);
               
			// remove files + database item
			_query.RemoveItem(await _query.GetObjectByFilePathAsync("/test2.jpg"));
		}
		
		[TestMethod]
		public async Task Thumbnail_NonExistingFile_API_Test()
		{
			var controller = new ThumbnailController(_query,new FakeSelectorStorage());;
			var actionResult = await controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
			var thumbnailAnswer = actionResult.StatusCode;
			Assert.AreEqual(404,thumbnailAnswer);
		}
		
		[TestMethod]
		public async Task Thumbnail_HappyFlowDisplayJson_API_Test()
		{
			// Arrange
			var storage = ArrangeStorage();
			var createAnImage = InsertSearchData();

			// Act
			// Create thumbnail in fake storage
			new Thumbnail(storage,storage).CreateThumb(createAnImage.FilePath, createAnImage.FileHash);
			
			// Check if exist
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = await controller.Thumbnail(createAnImage.FileHash,true,true) as JsonResult;
			
			// Thumbnail exist
			Assert.AreNotEqual(actionResult,null);
			var thumbnailAnswer = actionResult.Value as string;
			Assert.AreEqual("OK",thumbnailAnswer);
		}
	  
		[TestMethod]
		public async Task Thumbnail_HappyFlowFileStreamResult_API_Test()
		{
			// Arrange
			var storage = ArrangeStorage();
			var createAnImage = InsertSearchData();

			// Act
			// Create thumbnail in fake storage
			new Thumbnail(storage,storage).CreateThumb(createAnImage.FilePath, createAnImage.FileHash);
			
			// Check if exist
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = await controller.Thumbnail(createAnImage.FileHash, true) as FileStreamResult;
			
			// Thumbnail exist
			Assert.AreNotEqual(actionResult,null);
			var thumbnailAnswer = actionResult.ContentType;
			
			controller.Response.Headers.TryGetValue("x-filename", out var value ); 
			Assert.AreEqual(createAnImage.FileHash + ".jpg", value.ToString());

			Assert.AreEqual("image/jpeg",thumbnailAnswer);
			actionResult.FileStream.Dispose(); // for windows
		}
		
		[TestMethod]
		public async Task Thumbnail_ShowOriginalImage_API_Test()
		{
			var createAnImage = InsertSearchData();
			var storage = ArrangeStorage();

			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = await controller.Thumbnail(createAnImage.FileHash, true) as FileStreamResult;
			var thumbnailAnswer = actionResult.ContentType;
			
			controller.Response.Headers.TryGetValue("x-filename", out var value ); 
			Assert.AreEqual("test.jpg", value.ToString());
			
			Assert.AreEqual("image/jpeg",thumbnailAnswer);
			
			actionResult.FileStream.Dispose(); // for windows
		}

		[TestMethod]
		public async Task Thumbnail_IsMissing_ButOriginalExist_butNoIsSingleItemFlag_API_Test()
		{
			// Photo exist in database but " + "isSingleItem flag is Missing
			var createAnImage = InsertSearchData();
			var storage = ArrangeStorage();

			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = await controller.Thumbnail(createAnImage.FileHash, false, true) as JsonResult;
			var thumbnailAnswer = actionResult.StatusCode; // always null for some reason ?!
			Assert.AreEqual("Thumbnail is not ready yet",actionResult.Value);
		}

        [TestMethod]
        public async Task ApiController_FloatingDatabaseFileTest_API_Test()
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
            var actionResult = await controller.Thumbnail(item.FileHash, false, true) as NotFoundObjectResult;
            var thumbnailAnswer = actionResult.StatusCode;
            Assert.AreEqual(404,thumbnailAnswer);
            _query.RemoveItem(item);
        }

		[TestMethod]
		public async Task ApiController_Thumbnail1_NonExistingFile_API_Test()
		{
			var storage = ArrangeStorage();
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			var actionResult = await controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
			var thumbnailAnswer = actionResult.StatusCode;
			Assert.AreEqual(404,thumbnailAnswer);
		}
	}
}

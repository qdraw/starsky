using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Data;
using starsky.foundation.database.Interfaces;
using starsky.foundation.database.Models;
using starsky.foundation.database.Query;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.Helpers;
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
			_query = new Query(context, new AppSettings(), 
				null, new FakeIWebLogger(), memoryCache);
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

		private static IStorage ArrangeStorage()
		{
			var folderPaths = new List<string>{"/"};
			var inputSubPaths = new List<string>{"/test.jpg","/test2.jpg", "/test.dng"};
			var storage =
				new FakeIStorage(folderPaths, inputSubPaths, 
					new List<byte[]>{CreateAnImage.Bytes,CreateAnImage.Bytes,CreateAnImage.Bytes});
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
		public async Task Thumbnail_CorruptImage_NoContentResult_Test()
		{
			// Arrange
			var storage = ArrangeStorage();
			var plainTextStream = PlainTextFileHelper.StringToStream("CorruptImage");
			await storage.WriteStreamAsync(plainTextStream, ThumbnailNameHelper.Combine(
				"hash-corrupt-image", ThumbnailSize.ExtraLarge));

			await _query.AddItemAsync(new FileIndexItem("/test2.jpg"){FileHash= "hash-corrupt-image"});

			// Act
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
  
			var actionResult = controller.Thumbnail("hash-corrupt-image", 
				false, true) as NoContentResult;
			Assert.AreEqual(204,actionResult.StatusCode);
               
			// remove files + database item
			await _query.RemoveItemAsync(await _query.GetObjectByFilePathAsync("/test2.jpg"));
		}
		
		[TestMethod]
		public void Thumbnail_NonExistingFile_API_Test()
		{
			var controller = new ThumbnailController(_query,new FakeSelectorStorage());
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var actionResult = controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
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
			await new Thumbnail(storage,storage, 
				new FakeIWebLogger()).CreateThumb(createAnImage.FilePath, createAnImage.FileHash);
			
			// Check if exist
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.Thumbnail(createAnImage.FileHash,true,true) as JsonResult;
			
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
			await new Thumbnail(storage,storage, new FakeIWebLogger()
			).CreateThumb(createAnImage.FilePath, createAnImage.FileHash);
			
			// Check if exist
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.Thumbnail(createAnImage.FileHash, true) as FileStreamResult;
			
			// Thumbnail exist
			Assert.AreNotEqual(actionResult,null);
			var thumbnailAnswer = actionResult.ContentType;
			
			controller.Response.Headers.TryGetValue("x-filename", out var value ); 
			Assert.AreEqual(createAnImage.FileHash + ".jpg", value.ToString());

			Assert.AreEqual("image/jpeg",thumbnailAnswer);
			actionResult.FileStream.Dispose(); // for windows
		}
		
		[TestMethod]
		public void Thumbnail_IgnoreRawFile()
		{
			var storageSelector = new FakeSelectorStorage(ArrangeStorage());
			
			var controller = new ThumbnailController(new FakeIQuery(
				new List<FileIndexItem>
				{
					new FileIndexItem("/test.dng"){ FileHash = "hash1"}
				}),storageSelector);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			controller.Thumbnail("hash1", true);
			
			Assert.AreEqual(210,controller.Response.StatusCode);
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
			
			controller.Response.Headers.TryGetValue("x-filename", out var value ); 
			Assert.AreEqual("test.jpg", value.ToString());
			
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
		public async Task ApiController_FloatingDatabaseFileTest_API_Test()
		{
			var item = await _query.AddItemAsync(new FileIndexItem
			{
				ParentDirectory = "/fakeImage/",
				FileName = "fake.jpg",
				FileHash = "0986524678765456786543",
				Id= 788,
			});
            
			var storage = ArrangeStorage();
            
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
            
			var actionResult = controller.Thumbnail(item.FileHash, false, true) as NotFoundObjectResult;
			var thumbnailAnswer = actionResult.StatusCode;
			Assert.AreEqual(404,thumbnailAnswer);
			await _query.RemoveItemAsync(item);
		}

		[TestMethod]
		public void Thumbnail1_NonExistingFile_API_Test()
		{
			var storage = ArrangeStorage();
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var actionResult = controller.Thumbnail("404filehash", false, true) as NotFoundObjectResult;
			var thumbnailAnswer = actionResult.StatusCode;
			Assert.AreEqual(404,thumbnailAnswer);
		}
		
				
		[TestMethod]
		public void Thumbnail_GetLargeFirstChoiceResult()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Small),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Large)
			});
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			controller.Thumbnail("test", true, false, false);
			
			controller.Response.Headers.TryGetValue("x-image-size", out var value ); 
			Assert.AreEqual(ThumbnailSize.Large.ToString(), value.ToString());
		}
		
		[TestMethod]
		public void Thumbnail_IgnoreAtInInF()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Small),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Large)
			});
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			controller.Thumbnail("test@2000", true, false, false);
			
			controller.Response.Headers.TryGetValue("x-image-size", out var value ); 
			Assert.AreEqual(ThumbnailSize.Large.ToString(), value.ToString());
		}
		
		[TestMethod]
		public void Thumbnail_GetExtraLargeSecondChoiceResult()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.ExtraLarge)
			});
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			controller.Thumbnail("test", true, false, false);
			
			controller.Response.Headers.TryGetValue("x-image-size", out var value ); 
			Assert.AreEqual(ThumbnailSize.ExtraLarge.ToString(), value.ToString());
		}
				
		[TestMethod]
		public async Task ByZoomFactor_NonExistingFile_API_Test()
		{
			var controller = new ThumbnailController(_query,new FakeSelectorStorage());
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var actionResult = await controller.ByZoomFactor("404filehash", 1) as NotFoundObjectResult;
			var thumbnailAnswer = actionResult.StatusCode;
			Assert.AreEqual(404,thumbnailAnswer);
		}

		[TestMethod]
		public async Task ByZoomFactor_InputBadRequest()
		{
			var storageSelector = new FakeSelectorStorage(ArrangeStorage());
			
			var controller = new ThumbnailController(_query,storageSelector);;
			var actionResult = await controller.ByZoomFactor("../") as BadRequestResult;
			Assert.AreEqual(400,actionResult.StatusCode);
		}
		
		[TestMethod]
		public void ByZoomFactor_IgnoreRawFile()
		{
			var storageSelector = new FakeSelectorStorage(ArrangeStorage());
			
			var controller = new ThumbnailController(new FakeIQuery(
				new List<FileIndexItem>
				{
					new FileIndexItem("/test.dng"){ FileHash = "hash1"}
				}),storageSelector);
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			controller.ByZoomFactor("hash1");
			
			Assert.AreEqual(210,controller.Response.StatusCode);
		}
		
		
		[TestMethod]
		public async Task ByZoomFactor_ShowOriginalImage_API_Test()
		{
			var createAnImage = InsertSearchData();
			var storage = ArrangeStorage();

			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = await controller.ByZoomFactor(createAnImage.FileHash, 1) as FileStreamResult;
			var thumbnailAnswer = actionResult.ContentType;
			
			controller.Response.Headers.TryGetValue("x-filename", out var value ); 
			Assert.AreEqual("test.jpg", value.ToString());
			
			Assert.AreEqual("image/jpeg",thumbnailAnswer);
			
			await actionResult.FileStream.DisposeAsync(); // for windows
		}
		
		[TestMethod]
		public async Task ByZoomFactor_ShowOriginalImage_NoFileHash_API_Test()
		{
			InsertSearchData();
			var storage = ArrangeStorage();

			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = await controller.ByZoomFactor("____", 1, "/test.jpg") as FileStreamResult;
			var thumbnailAnswer = actionResult.ContentType;
			
			controller.Response.Headers.TryGetValue("x-filename", out var value ); 
			Assert.AreEqual("test.jpg", value.ToString());
			
			Assert.AreEqual("image/jpeg",thumbnailAnswer);
			
			await actionResult.FileStream.DisposeAsync(); // for windows
		}
		
		[TestMethod]
		public void ThumbnailSmallOrTinyMeta_InputBadRequest()
		{
			var storageSelector = new FakeSelectorStorage(ArrangeStorage());
			
			var controller = new ThumbnailController(_query,storageSelector);;
			var actionResult = controller.ThumbnailSmallOrTinyMeta("../") as BadRequestResult;
			Assert.AreEqual(400,actionResult.StatusCode);
		}

		[TestMethod]
		public void ThumbnailSmallOrTinyMeta_NotFound()
		{
			var storage = new FakeIStorage();
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			
			var actionResult = controller.ThumbnailSmallOrTinyMeta("404filehash") as NotFoundObjectResult;
			var thumbnailAnswer = actionResult.StatusCode;
			Assert.AreEqual(404,thumbnailAnswer);
		}
		
		[TestMethod]
		public void ThumbnailSmallOrTinyMeta_GetTinyResult_WhenSmallDoesNotExist()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Large)
			});
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			controller.ThumbnailSmallOrTinyMeta("test");
			
			controller.Response.Headers.TryGetValue("x-image-size", out var value ); 
			Assert.AreEqual(ThumbnailSize.TinyMeta.ToString(), value.ToString());
		}
		
		[TestMethod]
		public void ThumbnailSmallOrTinyMeta_GetSmallResult()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Small),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Large)
			});
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			controller.ThumbnailSmallOrTinyMeta("test");
			
			controller.Response.Headers.TryGetValue("x-image-size", out var value ); 
			Assert.AreEqual(ThumbnailSize.Small.ToString(), value.ToString());
		}
				
		[TestMethod]
		public void ThumbnailSmallOrTinyMeta_GetLargeResultWhenAllAreMissing()
		{
			var storage = new FakeIStorage(new List<string>{"/"}, new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Large)
			});
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			controller.ThumbnailSmallOrTinyMeta("test");
			
			controller.Response.Headers.TryGetValue("x-image-size", out var value ); 
			Assert.AreEqual(ThumbnailSize.Large.ToString(), value.ToString());
		}
		
		[TestMethod]
		public void ListSizesByHash_NotFound()
		{
			// Arrange
			var storage = new FakeIStorage(new List<string>(),
				new List<string> {"01234567890123456789123456"});

			// Check if exist
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.ListSizesByHash("01234567890123456789123456") as NotFoundObjectResult;
			
			Assert.AreNotEqual(null, actionResult);
			Assert.AreEqual(404, actionResult.StatusCode);
		}
		
		[TestMethod]
		public void ListSizesByHash_ExpectLarge()
		{
			var item = _query.AddItem(new FileIndexItem("/test123.jpg")
			{
				FileHash = "01234567890123456789123456"
			});
			
			// Arrange
			var storage = new FakeIStorage(new List<string>(),
				new List<string> {"01234567890123456789123456"});

			// Check if exist
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.ListSizesByHash("01234567890123456789123456") as JsonResult;
			
			// Thumbnail exist
			Assert.AreNotEqual(actionResult,null);
			var thumbnailAnswer = actionResult.Value as ThumbnailSizesExistStatusModel;

			Assert.AreEqual(202, controller.Response.StatusCode);
			Assert.AreEqual(true,thumbnailAnswer.Large);
			Assert.AreEqual(false,thumbnailAnswer.ExtraLarge);
			Assert.AreEqual(false,thumbnailAnswer.TinyMeta);

			_query.RemoveItem(item);
		}
		
		[TestMethod]
		public void ListSizesByHash_AllExist_exceptTinyMeta()
		{
			var hash = "01234567890123456789123456";
			var item = _query.AddItem(new FileIndexItem("/test123.jpg")
			{
				FileHash = hash
			});
			
			// Arrange
			var storage = new FakeIStorage(new List<string>(),
				new List<string> {
					ThumbnailNameHelper.Combine(hash, ThumbnailSize.Large),
					ThumbnailNameHelper.Combine(hash, ThumbnailSize.Small),
					ThumbnailNameHelper.Combine(hash, ThumbnailSize.ExtraLarge),
				});

			// Check if exist
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.ListSizesByHash(hash) as JsonResult;
			
			// Thumbnail exist
			Assert.AreNotEqual(null, actionResult);
			var thumbnailAnswer = actionResult.Value as ThumbnailSizesExistStatusModel;

			Assert.AreEqual(200, controller.Response.StatusCode);
			Assert.AreEqual(true,thumbnailAnswer.Large);
			Assert.AreEqual(true,thumbnailAnswer.ExtraLarge);
			Assert.AreEqual(true,thumbnailAnswer.Small);
			// > TinyMeta is optional and not needed
			Assert.AreEqual(false,thumbnailAnswer.TinyMeta);

			_query.RemoveItem(item);
		}
		
		[TestMethod]
		public void ListSizesByHash_InputBadRequest()
		{
			var storageSelector = new FakeSelectorStorage(ArrangeStorage());
			
			var controller = new ThumbnailController(_query,storageSelector);;
			var actionResult = controller.ListSizesByHash("../") as BadRequestResult;
			Assert.AreEqual(400,actionResult.StatusCode);
		}
		
		[TestMethod]
		public void ListSizesByHash_IgnoreRaw()
		{
			var item = _query.AddItem(new FileIndexItem("/test123.arw")
			{
				FileHash = "91234567890123456789123451"
			});
			
			// Arrange
			var storage = new FakeIStorage(new List<string>(),
				new List<string> {"91234567890123456789123451"});

			// Check if exist
			var controller = new ThumbnailController(_query,new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();

			var actionResult = controller.ListSizesByHash("91234567890123456789123451") as JsonResult;
			
			// Thumbnail exist
			Assert.AreNotEqual(null,actionResult);

			Assert.AreEqual(210, controller.Response.StatusCode);

			_query.RemoveItem(item);
		}
	}
}

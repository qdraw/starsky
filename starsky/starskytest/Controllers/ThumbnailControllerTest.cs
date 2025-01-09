using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class ThumbnailControllerTest
{
	private readonly Query _query;

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

	private async Task<FileIndexItem> InsertSearchData()
	{
		const string fileHash = "home0012304590";
		var item = new FileIndexItem
		{
			FileName = "test.jpg",
			ParentDirectory = "/",
			FileHash = fileHash,
			ColorClass = ColorClassParser.Color.Winner // 1
		};

		if ( string.IsNullOrEmpty(await _query.GetSubPathByHashAsync(fileHash)) )
		{
			await _query.AddItemAsync(item);
		}

		return item;
	}

	private static FakeIStorage ArrangeStorage()
	{
		var folderPaths = new List<string> { "/" };
		var inputSubPaths = new List<string> { "/test.jpg", "/test2.jpg", "/test.dng" };
		var storage =
			new FakeIStorage(folderPaths, inputSubPaths,
				new List<byte[]>
				{
					CreateAnImage.Bytes.ToArray(),
					CreateAnImage.Bytes.ToArray(),
					CreateAnImage.Bytes.ToArray()
				});
		return storage;
	}

	[TestMethod]
	public async Task Thumbnail_InputBadRequest()
	{
		var storageSelector = new FakeSelectorStorage(ArrangeStorage());

		var controller = new ThumbnailController(_query, storageSelector, new AppSettings());
		var actionResult = await controller.Thumbnail("../") as BadRequestResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task Thumbnail_CorruptImage_NoContentResult_Test()
	{
		// Arrange
		var storage = ArrangeStorage();
		var plainTextStream = StringToStreamHelper.StringToStream("CorruptImage");
		await storage.WriteStreamAsync(plainTextStream, ThumbnailNameHelper.Combine(
			"hash-corrupt-image", ThumbnailSize.ExtraLarge,
			new AppSettings().ThumbnailImageFormat));

		await _query.AddItemAsync(
			new FileIndexItem("/test2.jpg") { FileHash = "hash-corrupt-image" });

		// Act
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult = await controller.Thumbnail("hash-corrupt-image", "/test2.jpg",
			false, true) as NoContentResult;
		Assert.AreEqual(204, actionResult?.StatusCode);

		// remove files + database item
		var resultItem = await _query.GetObjectByFilePathAsync("/test2.jpg");
		if ( resultItem == null )
		{
			throw new WebException("object is null");
		}

		await _query.RemoveItemAsync(resultItem);
	}

	[TestMethod]
	public async Task Thumbnail_NonExistingFile_API_Test()
	{
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult =
			await controller.Thumbnail("404filehash", null, false,
				true) as NotFoundObjectResult;
		var thumbnailAnswer = actionResult?.StatusCode;
		Assert.AreEqual(404, thumbnailAnswer);
	}

	[TestMethod]
	public async Task Thumbnail_HappyFlowDisplayJson_API_Test()
	{
		// Arrange
		var storage = ArrangeStorage();
		var createAnImage = await InsertSearchData();

		// Act
		// Create thumbnail in fake storage
		var service = new ThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings(),
			new FakeIUpdateStatusGeneratedThumbnailService());
		await service.GenerateThumbnail(createAnImage.FilePath!, createAnImage.FileHash!);

		// Check if exist
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			await controller.Thumbnail(createAnImage.FileHash!, null,
				true, true) as JsonResult;

		// Thumbnail exist
		Assert.AreNotEqual(null, actionResult);
		var thumbnailAnswer = actionResult?.Value as string;
		Assert.AreEqual("OK", thumbnailAnswer);
	}

	[TestMethod]
	public async Task Thumbnail_InvalidModel()
	{
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		var result = await controller.Thumbnail("Invalid");
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task Thumbnail_HappyFlowFileStreamResult_API_Test()
	{
		// Arrange
		var storage = ArrangeStorage();
		var createAnImage = await InsertSearchData();

		// Act
		// Create thumbnail in fake storage
		var thumbnailService = new ThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings(),
			new FakeIUpdateStatusGeneratedThumbnailService());

		await thumbnailService.GenerateThumbnail(createAnImage.FilePath!, createAnImage.FileHash!);

		// Check if exist
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			await controller.Thumbnail(createAnImage.FileHash!, null, true) as FileStreamResult;

		// Thumbnail exist
		Assert.AreNotEqual(null, actionResult);
		var thumbnailAnswer = actionResult?.ContentType;

		controller.Response.Headers.TryGetValue("x-filename", out var value);
		Assert.AreEqual(createAnImage.FileHash + ".jpg", value.ToString());

		Assert.AreEqual("image/jpeg", thumbnailAnswer);
		await actionResult!.FileStream.DisposeAsync(); // for windows
	}

	[TestMethod]
	public async Task Thumbnail_HashNotFound_ButFilePathValid()
	{
		await InsertSearchData();
		var storage = ArrangeStorage();
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult =
			await controller.Thumbnail("any", "/test.jpg", true) as FileStreamResult;

		controller.Response.Headers.TryGetValue("x-filename", out var value);
		Assert.AreEqual("test.jpg", value.ToString());
		var thumbnailAnswer = actionResult?.ContentType;
		Assert.AreEqual("image/jpeg", thumbnailAnswer);

		await actionResult!.FileStream.DisposeAsync(); // for windows
	}

	[TestMethod]
	public async Task Thumbnail_HashNotFound_ButFilePath_Not_Valid()
	{
		await InsertSearchData();
		var storage = ArrangeStorage();
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult =
			await controller.Thumbnail("any", "/not_found.jpg", true) as NotFoundObjectResult;

		Assert.AreEqual(404, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task Thumbnail_In_db_but_not_on_disk()
	{
		await InsertSearchData();
		var storage = ArrangeStorage();
		var controller = new ThumbnailController(
			new FakeIQuery(new List<FileIndexItem>
			{
				new("/not_on_disk.jpg") { FileHash = "not_on_disk_hash" }
			}), new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult =
			await controller.Thumbnail("not_on_disk_hash", "/not_on_disk.jpg", true) as
				NotFoundObjectResult;

		Assert.AreEqual(404, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task Thumbnail_IgnoreRawFile()
	{
		var storageSelector = new FakeSelectorStorage(ArrangeStorage());

		var controller = new ThumbnailController(new FakeIQuery(
				new List<FileIndexItem> { new("/test.dng") { FileHash = "hash1" } }),
			storageSelector, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		await controller.Thumbnail("hash1", null, true);

		Assert.AreEqual(210, controller.Response.StatusCode);
	}

	[TestMethod]
	public async Task Thumbnail_ShowOriginalImage_API_Test()
	{
		var createAnImage = await InsertSearchData();
		var storage = ArrangeStorage();

		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			await controller.Thumbnail(createAnImage.FileHash!, null, true) as FileStreamResult;
		Assert.IsNotNull(actionResult);
		Assert.IsNotNull(actionResult.FileStream);

		var thumbnailAnswer = actionResult.ContentType;

		controller.Response.Headers.TryGetValue("x-filename", out var value);
		Assert.AreEqual("test.jpg", value.ToString());

		Assert.AreEqual("image/jpeg", thumbnailAnswer);

		await actionResult.FileStream.DisposeAsync(); // for windows
	}

	[TestMethod]
	public async Task Thumbnail_IsMissing_ButOriginalExist_butNoIsSingleItemFlag_API_Test()
	{
		// Photo exist in database but " + "isSingleItem flag is Missing
		var createAnImage = await InsertSearchData();
		var storage = ArrangeStorage();

		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			await controller.Thumbnail(createAnImage.FileHash!, null, false,
				true) as JsonResult;
		Assert.AreEqual("Thumbnail is not ready yet", actionResult?.Value);
	}

	[TestMethod]
	public async Task ApiController_FloatingDatabaseFileTest_API_Test()
	{
		var item = await _query.AddItemAsync(new FileIndexItem
		{
			ParentDirectory = "/fakeImage/",
			FileName = "fake.jpg",
			FileHash = "0986524678765456786543",
			Id = 788
		});

		var storage = ArrangeStorage();

		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			await controller.Thumbnail(item.FileHash!, null, false, true) as
				NotFoundObjectResult;
		var thumbnailAnswer = actionResult?.StatusCode;
		Assert.AreEqual(404, thumbnailAnswer);
		await _query.RemoveItemAsync(item);
	}

	[TestMethod]
	public async Task Thumbnail1_NonExistingFile_API_Test()
	{
		var storage = ArrangeStorage();
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult =
			await controller.Thumbnail("404filehash", null, false,
				true) as NotFoundObjectResult;
		var thumbnailAnswer = actionResult?.StatusCode;
		Assert.AreEqual(404, thumbnailAnswer);
	}


	[TestMethod]
	public async Task Thumbnail_GetLargeFirstChoiceResult()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta,
					new AppSettings().ThumbnailImageFormat),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Small,
					new AppSettings().ThumbnailImageFormat),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Large,
					new AppSettings().ThumbnailImageFormat)
			});
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		await controller.Thumbnail("test", null, true, false, false);

		controller.Response.Headers.TryGetValue("x-image-size", out var value);
		Assert.AreEqual(ThumbnailSize.Large.ToString(), value.ToString());
	}

	[TestMethod]
	public async Task Thumbnail_IgnoreAtInInF()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta,
					new AppSettings().ThumbnailImageFormat),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Small,
					new AppSettings().ThumbnailImageFormat),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Large,
					new AppSettings().ThumbnailImageFormat)
			});
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		await controller.Thumbnail("test@2000", null, true, false, false);

		controller.Response.Headers.TryGetValue("x-image-size", out var value);
		Assert.AreEqual(ThumbnailSize.Large.ToString(), value.ToString());
	}

	[TestMethod]
	public async Task Thumbnail_GetExtraLargeSecondChoiceResult()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.ExtraLarge,
					new AppSettings().ThumbnailImageFormat)
			});
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		await controller.Thumbnail("test", null, true, false, false);

		controller.Response.Headers.TryGetValue("x-image-size", out var value);
		Assert.AreEqual(ThumbnailSize.ExtraLarge.ToString(), value.ToString());
	}

	[TestMethod]
	public async Task ByZoomFactor_NonExistingFile_API_Test()
	{
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult =
			await controller.ByZoomFactorAsync("404filehash", 1) as NotFoundObjectResult;
		var thumbnailAnswer = actionResult?.StatusCode;
		Assert.AreEqual(404, thumbnailAnswer);
	}

	[TestMethod]
	public async Task ByZoomFactor_ModelState()
	{
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		var result =
			await controller.ByZoomFactorAsync("InValid", 1);
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task ByZoomFactor_InputBadRequest()
	{
		var storageSelector = new FakeSelectorStorage(ArrangeStorage());

		var controller = new ThumbnailController(_query, storageSelector, new AppSettings());
		var actionResult = await controller.ByZoomFactorAsync("../") as BadRequestResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task ByZoomFactor_IgnoreRawFile()
	{
		var storageSelector = new FakeSelectorStorage(ArrangeStorage());

		var controller = new ThumbnailController(new FakeIQuery(
				new List<FileIndexItem> { new("/test.dng") { FileHash = "hash1" } }),
			storageSelector, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		await controller.ByZoomFactorAsync("hash1");

		Assert.AreEqual(210, controller.Response.StatusCode);
	}


	[TestMethod]
	public async Task ByZoomFactor_ShowOriginalImage_API_Test()
	{
		var createAnImage = await InsertSearchData();
		var storage = ArrangeStorage();

		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			await controller.ByZoomFactorAsync(createAnImage.FileHash!, 1) as FileStreamResult;
		var thumbnailAnswer = actionResult?.ContentType;

		controller.Response.Headers.TryGetValue("x-filename", out var value);
		Assert.AreEqual("test.jpg", value.ToString());

		Assert.AreEqual("image/jpeg", thumbnailAnswer);

		Assert.IsNotNull(actionResult?.FileStream);

		await actionResult.FileStream.DisposeAsync(); // for windows
	}

	[TestMethod]
	public async Task ByZoomFactor_ShowOriginalImage_NoFileHash_API_Test()
	{
		await InsertSearchData();
		var storage = ArrangeStorage();

		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			await controller.ByZoomFactorAsync("____", 1, "/test.jpg") as FileStreamResult;
		Assert.IsNotNull(actionResult);
		var thumbnailAnswer = actionResult.ContentType;

		controller.Response.Headers.TryGetValue("x-filename", out var value);
		Assert.AreEqual("test.jpg", value.ToString());

		Assert.AreEqual("image/jpeg", thumbnailAnswer);

		await actionResult.FileStream.DisposeAsync(); // for windows
	}

	[TestMethod]
	public void ThumbnailSmallOrTinyMeta_InputBadRequest()
	{
		var storageSelector = new FakeSelectorStorage(ArrangeStorage());

		var controller = new ThumbnailController(_query, storageSelector, new AppSettings());
		var actionResult = controller.ThumbnailSmallOrTinyMeta("../") as BadRequestResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public void ThumbnailSmallOrTinyMeta_NotFound()
	{
		var storage = new FakeIStorage();
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			controller.ThumbnailSmallOrTinyMeta("404filehash") as NotFoundObjectResult;
		var thumbnailAnswer = actionResult?.StatusCode;
		Assert.AreEqual(404, thumbnailAnswer);
	}

	[TestMethod]
	public void ThumbnailSmallOrTinyMeta_GetTinyResult_WhenSmallDoesNotExist()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta,
					new AppSettings().ThumbnailImageFormat),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Large,
					new AppSettings().ThumbnailImageFormat)
			});
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		controller.ThumbnailSmallOrTinyMeta("test");

		controller.Response.Headers.TryGetValue("x-image-size", out var value);
		Assert.AreEqual(ThumbnailSize.TinyMeta.ToString(), value.ToString());
	}

	[TestMethod]
	public void ThumbnailSmallOrTinyMeta_GetSmallResult()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta,
					new AppSettings().ThumbnailImageFormat),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Small,
					new AppSettings().ThumbnailImageFormat),
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Large,
					new AppSettings().ThumbnailImageFormat)
			});
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		controller.ThumbnailSmallOrTinyMeta("test");

		controller.Response.Headers.TryGetValue("x-image-size", out var value);
		Assert.AreEqual(ThumbnailSize.Small.ToString(), value.ToString());
	}

	[TestMethod]
	public void ThumbnailSmallOrTinyMeta_GetLargeResultWhenAllAreMissing()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string>
			{
				ThumbnailNameHelper.Combine("test", ThumbnailSize.Large,
					new AppSettings().ThumbnailImageFormat)
			});
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		controller.ThumbnailSmallOrTinyMeta("test");

		controller.Response.Headers.TryGetValue("x-image-size", out var value);
		Assert.AreEqual(ThumbnailSize.Large.ToString(), value.ToString());
	}

	[TestMethod]
	public void ThumbnailSmallOrTinyMeta_InvalidModel()
	{
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		var result = controller.ThumbnailSmallOrTinyMeta("Invalid");
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task ListSizesByHash_NotFound()
	{
		// Arrange
		var storage = new FakeIStorage(new List<string>(),
			new List<string> { "01234567890123456789123456" });

		// Check if exist
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			await controller.ListSizesByHash("01234567890123456789123456") as
				NotFoundObjectResult;

		Assert.AreNotEqual(null, actionResult);
		Assert.AreEqual(404, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task ListSizesByHash_ExpectLarge()
	{
		var item = await _query.AddItemAsync(new FileIndexItem("/test123.jpg")
		{
			FileHash = "01234567890123456789123456"
		});

		// Arrange
		var storage = new FakeIStorage(new List<string>(),
			new List<string> { "01234567890123456789123456.jpg" });

		// Check if exist
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			await controller.ListSizesByHash("01234567890123456789123456") as JsonResult;

		// Thumbnail exist
		Assert.AreNotEqual(null, actionResult);
		var thumbnailAnswer = actionResult?.Value as ThumbnailSizesExistStatusModel;

		Assert.IsNotNull(thumbnailAnswer);
		Assert.AreEqual(202, controller.Response.StatusCode);
		Assert.IsTrue(thumbnailAnswer.Large);
		Assert.IsFalse(thumbnailAnswer.ExtraLarge);
		Assert.IsFalse(thumbnailAnswer.TinyMeta);

		await _query.RemoveItemAsync(item);
	}

	[TestMethod]
	public async Task ListSizesByHash_AllExist_exceptTinyMeta()
	{
		const string hash = "01234567890123456789123456";
		var item = await _query.AddItemAsync(new FileIndexItem("/test123.jpg") { FileHash = hash });

		// Arrange
		var storage = new FakeIStorage(new List<string>(),
			new List<string>
			{
				ThumbnailNameHelper.Combine(hash, ThumbnailSize.Large,
					new AppSettings().ThumbnailImageFormat),
				ThumbnailNameHelper.Combine(hash, ThumbnailSize.Small,
					new AppSettings().ThumbnailImageFormat),
				ThumbnailNameHelper.Combine(hash, ThumbnailSize.ExtraLarge,
					new AppSettings().ThumbnailImageFormat)
			});

		// Check if exist
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult = await controller.ListSizesByHash(hash) as JsonResult;

		// Thumbnail exist
		Assert.IsNotNull(actionResult);
		var thumbnailAnswer = actionResult.Value as ThumbnailSizesExistStatusModel;
		Assert.IsNotNull(thumbnailAnswer);

		Assert.AreEqual(200, controller.Response.StatusCode);
		Assert.IsTrue(thumbnailAnswer.Large);
		Assert.IsTrue(thumbnailAnswer.ExtraLarge);
		Assert.IsTrue(thumbnailAnswer.Small);
		// > TinyMeta is optional and not needed
		Assert.IsFalse(thumbnailAnswer.TinyMeta);

		await _query.RemoveItemAsync(item);
	}

	[TestMethod]
	public async Task ListSizesByHash_InputBadRequest()
	{
		var storageSelector = new FakeSelectorStorage(ArrangeStorage());

		var controller = new ThumbnailController(_query, storageSelector, new AppSettings());
		var actionResult = await controller.ListSizesByHash("../") as BadRequestResult;
		Assert.AreEqual(400, actionResult?.StatusCode);
	}

	[TestMethod]
	public async Task ListSizesByHash_InvalidModel()
	{
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ModelState.AddModelError("Key", "ErrorMessage");
		var result = await controller.ListSizesByHash("Invalid");
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}

	[TestMethod]
	public async Task ListSizesByHash_IgnoreRaw()
	{
		var item = await _query.AddItemAsync(new FileIndexItem("/test123.arw")
		{
			FileHash = "91234567890123456789123451"
		});

		// Arrange
		var storage = new FakeIStorage(new List<string>(),
			new List<string> { "91234567890123456789123451" });

		// Check if exist
		var controller =
			new ThumbnailController(_query, new FakeSelectorStorage(storage), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult =
			await controller.ListSizesByHash("91234567890123456789123451") as JsonResult;

		// Thumbnail exist
		Assert.AreNotEqual(null, actionResult);

		Assert.AreEqual(210, controller.Response.StatusCode);

		await _query.RemoveItemAsync(item);
	}
}

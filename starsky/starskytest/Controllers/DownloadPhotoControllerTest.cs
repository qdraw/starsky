using System;
using System.Collections.Generic;
using System.Linq;
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
using starsky.foundation.storage.Services;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class DownloadPhotoControllerTest
{
	private readonly Query _query;

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
		var scopeFactory = provider.GetService<IServiceScopeFactory>();
		_query = new Query(context, new AppSettings(), scopeFactory, new FakeIWebLogger(),
			memoryCache);
	}

	private async Task<FileIndexItem> InsertSearchData()
	{
		var item = new FileIndexItem
		{
			FileName = "test.jpg",
			ParentDirectory = "/",
			FileHash = "/home0012304590",
			ColorClass = ColorClassParser.Color.Winner // 1
		};

		if ( string.IsNullOrEmpty(await _query.GetSubPathByHashAsync("home0012304590")) )
		{
			await _query.AddItemAsync(item);
		}

		return item;
	}

	private static FakeIStorage ArrangeStorage()
	{
		var folderPaths = new List<string> { "/" };
		var inputSubPaths = new List<string> { "/test.jpg", "/test.xmp", "/corrupt.jpg" };
		var storage =
			new FakeIStorage(folderPaths, inputSubPaths,
				new List<byte[]>
				{
					CreateAnImage.Bytes.ToArray(),
					CreateAnXmp.Bytes.ToArray(),
					Array.Empty<byte>()
				});
		return storage;
	}

	[TestMethod]
	public void DownloadSidecar_Ok()
	{
		// Arrange
		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());

		// Act
		var controller = new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
			new FakeIThumbnailService(), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult = controller.DownloadSidecar("/test.xmp") as FileStreamResult;
		Assert.IsNotNull(actionResult);

		actionResult.FileStream.Dispose();
	}

	[TestMethod]
	public void DownloadSidecar_ReturnsBadRequest()
	{
		// Arrange
		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());

		// Act
		var controller = new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
			new FakeIThumbnailService(), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var actionResult = controller.DownloadSidecar(null!);

		// Assert
		Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
	}

	[TestMethod]
	public void DownloadSidecar_TryToGetJpeg()
	{
		// Arrange
		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());

		// Act
		var controller =
			new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
				new FakeIThumbnailService(), new AppSettings())
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		var actionResult = controller.DownloadSidecar("/test.jpg") as NotFoundObjectResult;

		Assert.IsNotNull(actionResult);
		Assert.AreEqual(404, actionResult.StatusCode);
	}

	[TestMethod]
	public void DownloadSidecar_NotFound()
	{
		// Arrange
		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());

		// Act
		var controller =
			new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
				new FakeIThumbnailService(), new AppSettings())
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		var actionResult = controller.DownloadSidecar("/not-found.xmp") as NotFoundObjectResult;

		Assert.IsNotNull(actionResult);
		Assert.AreEqual(404, actionResult.StatusCode);
	}

	[TestMethod]
	public async Task DownloadPhoto_isThumbnailTrue_CreateThumb_ReturnFileStream_Test()
	{
		// Arrange
		var fileIndexItem = await InsertSearchData();
		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());
		var thumbnailService = new ThumbnailService(selectorStorage, new FakeIWebLogger(),
			new AppSettings(), new FakeIUpdateStatusGeneratedThumbnailService(),
			new FakeIVideoProcess(selectorStorage),
			new FileHashSubPathStorage(selectorStorage, new FakeIWebLogger()),
			new FakeINativePreviewThumbnailGenerator());

		// Act
		var controller = new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
			thumbnailService, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult =
			await controller.DownloadPhoto(fileIndexItem.FilePath!) as FileStreamResult;
		Assert.IsNotNull(actionResult);

		await actionResult.FileStream.DisposeAsync();
	}

	[TestMethod]
	public async Task DownloadPhoto_WrongInputNotFound()
	{
		// Arrange
		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());
		var thumbnailService = new ThumbnailService(selectorStorage, new FakeIWebLogger(),
			new AppSettings(), new FakeIUpdateStatusGeneratedThumbnailService(),
			new FakeIVideoProcess(selectorStorage),
			new FileHashSubPathStorage(selectorStorage, new FakeIWebLogger()),
			new FakeINativePreviewThumbnailGenerator());

		// Act
		var controller =
			new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
				thumbnailService, new AppSettings())
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		var actionResult = await controller.DownloadPhoto("?isthumbnail") as NotFoundObjectResult;

		Assert.IsNotNull(actionResult);
		Assert.AreEqual(404, actionResult.StatusCode);
	}

	[TestMethod]
	public async Task DownloadPhoto_ReturnsBadRequest()
	{
		// Arrange
		var controller =
			new DownloadPhotoController(_query, new FakeSelectorStorage(), new FakeIWebLogger(),
				new FakeIThumbnailService(), new AppSettings())
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		controller.ModelState.AddModelError("Key", "ErrorMessage");

		var actionResult = await controller.DownloadPhoto(null!);

		// Assert
		Assert.IsInstanceOfType(actionResult, typeof(BadRequestObjectResult));
	}

	[TestMethod]
	public async Task DownloadPhotoCorrupt()
	{
		// Arrange
		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());

		var item = await _query.AddItemAsync(new FileIndexItem
		{
			FileName = "corrupt.jpg", ParentDirectory = "/", FileHash = "hash"
		});

		// Act
		var controller =
			new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
				new FakeIThumbnailService(), new AppSettings())
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};

		var actionResult = await controller.DownloadPhoto("/corrupt.jpg") as JsonResult;
		Assert.IsNotNull(actionResult);

		Assert.AreEqual(500, controller.Response.StatusCode);

		await _query.RemoveItemAsync(item);
	}


	[TestMethod]
	public async Task DownloadPhoto_NotFound()
	{
		// Arrange
		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());

		// Act
		var controller =
			new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
				new FakeIThumbnailService(), new AppSettings())
			{
				ControllerContext = { HttpContext = new DefaultHttpContext() }
			};
		var actionResult = await controller.DownloadPhoto("/not-found.jpg") as NotFoundObjectResult;

		Assert.IsNotNull(actionResult);
		Assert.AreEqual(404, actionResult.StatusCode);
	}

	[TestMethod]
	public async Task DownloadPhoto_isThumbnailFalse_ReturnFileStream_Test()
	{
		// Arrange
		var fileIndexItem = await InsertSearchData();
		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());

		// Act
		var controller = new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
			new FakeIThumbnailService(), new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult =
			await controller.DownloadPhoto(fileIndexItem.FilePath!, false) as FileStreamResult;
		Assert.IsNotNull(actionResult);

		await actionResult.FileStream.DisposeAsync();
	}

	[TestMethod]
	public async Task DownloadPhoto_isThumbnailTrue_ReturnAThumb_ReturnFileStream_Test()
	{
		// Arrange
		var fileIndexItem = await InsertSearchData();
		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());
		var thumbnailService = new ThumbnailService(selectorStorage, new FakeIWebLogger(),
			new AppSettings(), new FakeIUpdateStatusGeneratedThumbnailService(),
			new FakeIVideoProcess(selectorStorage),
			new FileHashSubPathStorage(selectorStorage, new FakeIWebLogger()),
			new FakeINativePreviewThumbnailGenerator());

		// Act
		var controller = new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
			thumbnailService, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		// Run once
		var actionResult1 =
			await controller.DownloadPhoto(fileIndexItem.FilePath!) as FileStreamResult;
		await actionResult1!.FileStream.DisposeAsync();

		// Run twice
		var actionResult2 =
			await controller.DownloadPhoto(fileIndexItem.FilePath!) as FileStreamResult;
		Assert.IsNotNull(actionResult2);

		await actionResult2.FileStream.DisposeAsync();
	}

	[TestMethod]
	public async Task ApiController_DownloadPhoto_SourceImageIsMissing_Test()
	{
		// Arrange
		var fileIndexItem = await InsertSearchData();

		// so the item does not exist on disk
		var selectorStorage = new FakeSelectorStorage();

		// Act
		var controller = new DownloadPhotoController(_query, selectorStorage, new FakeIWebLogger(),
			new FakeIThumbnailService(), new AppSettings());
		var actionResult =
			await controller.DownloadPhoto(fileIndexItem.FilePath!) as NotFoundObjectResult;
		Assert.IsNotNull(actionResult);
		Assert.AreEqual(404, actionResult.StatusCode);
		Assert.AreEqual("source image missing /test.jpg", actionResult.Value);
	}

	[TestMethod]
	public async Task DownloadPhoto_Thumb_base_folder_not_found_Test()
	{
		// Arrange
		var fileIndexItem = await InsertSearchData();
		var storage =
			new FakeIStorage(null!, new List<string> { "/test.jpg" },
				new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var selectorStorage = new FakeSelectorStorage(storage);


		// Act
		var controller = new DownloadPhotoController(_query, selectorStorage,
			new FakeIWebLogger(), new FakeIThumbnailService(), new AppSettings());

		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		var actionResult =
			await controller.DownloadPhoto(fileIndexItem.FilePath!) as NotFoundObjectResult;

		Assert.IsNotNull(actionResult);
		Assert.AreEqual(404, actionResult.StatusCode);
		Assert.AreEqual("ThumbnailTempFolder not found", actionResult.Value);
	}
}

using System;
using System.Collections.Generic;
using System.IO;
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
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Interfaces;
using starsky.foundation.thumbnailgeneration.Models;
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
			new FakeIStorage(null!,
				["/test.jpg"],
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

	[TestMethod]
	[DataRow("")]
	[DataRow(null)]
	public async Task DownloadPhoto_Thumbnail_MissingFileHash_Returns500(string? fileHash)
	{
		// Arrange
		var fileIndexItem = new FileIndexItem
		{
			FileName = "test.jpg",
			ParentDirectory = "/",
			FileHash = fileHash, // Missing hash
			ColorClass = ColorClassParser.Color.Winner
		};

		if ( string.IsNullOrEmpty(
			    await _query.GetSubPathByHashAsync("home0012304590_nomissinghash")) )
		{
			await _query.AddItemAsync(fileIndexItem);
		}

		var selectorStorage = new FakeSelectorStorage(ArrangeStorage());
		var controller = new DownloadPhotoController(_query, selectorStorage,
			new FakeIWebLogger(),
			new FakeIThumbnailService(), new AppSettings());

		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		// Act
		var actionResult = await controller.DownloadPhoto(fileIndexItem.FilePath!)
			as JsonResult;

		// Assert
		Assert.IsNotNull(actionResult);
		Assert.AreEqual(500, controller.Response.StatusCode);
		Assert.IsTrue(actionResult.Value?.ToString()?
			.Contains("Thumbnail generation failed"));
	}

	[TestMethod]
	public async Task
		DownloadPhoto_GenerationMarkedSuccess_ButLargeMissing_Returns500_AndLogsError()
	{
		// Arrange
		var filePath = "/test-gen.jpg";
		var fileHash = "FAKEHASH123";

		// subpath storage has the source file
		var subPathStorage = new FakeIStorage(new List<string> { "/" },
			new List<string> { filePath },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		// thumbnail storage only has small and meta but NOT large
		var thumbFiles = new List<string>
		{
			ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.Small,
				ThumbnailImageFormat.webp),
			ThumbnailNameHelper.Combine(fileHash, ThumbnailSize.TinyMeta, ThumbnailImageFormat.webp)
		};
		var thumbnailStorage = new FakeIStorage(["/"], thumbFiles,
			new List<byte[]> { new byte[10], new byte[10] });

		var selectorStorage = new FakeSelectorStorageByType(subPathStorage, thumbnailStorage,
			new FakeIStorage(), new FakeIStorage());

		// Query contains the FileIndexItem with a FileHash
		var fakeQuery = new FakeIQuery(new List<FileIndexItem>
		{
			new(filePath) { FileHash = fileHash, IsDirectory = false }
		});

		var fakeLogger = new FakeIWebLogger();

		// Fake thumbnail service that returns success results but does NOT write the large thumbnail
		var fakeThumbnailService = new FakeIThumbnailServiceNoWrite();

		var controller = new DownloadPhotoController(fakeQuery, selectorStorage, fakeLogger,
			fakeThumbnailService, new AppSettings());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		// Act
		var actionResult = await controller.DownloadPhoto(filePath) as JsonResult;

		// Assert - controller should respond with 500 and the specific JSON message
		Assert.IsNotNull(actionResult);
		Assert.AreEqual(500, controller.Response.StatusCode);
		Assert.AreEqual("Thumbnail generation failed: file not persisted after generation",
			actionResult.Value);

		// Logger should have recorded the missing thumbnail error message
		Assert.IsTrue(fakeLogger.TrackedExceptions.Exists(t => t.Item2 != null &&
		                                                       t.Item2.Contains(
			                                                       "Thumbnail file not found after generation (marked success)")));
	}

	// Fake thumbnail service that returns success generation results but doesn't write files
	private class FakeIThumbnailServiceNoWrite : IThumbnailService
	{
		public Task<List<GenerationResultModel>> GenerateThumbnail(string fileOrFolderPath,
			ThumbnailGenerationType type = ThumbnailGenerationType.All)
		{
			return Task.FromResult(new List<GenerationResultModel>());
		}

		public Task<List<GenerationResultModel>> GenerateThumbnail(string subPath, string fileHash,
			ThumbnailGenerationType type = ThumbnailGenerationType.All)
		{
			return Task.FromResult(new List<GenerationResultModel>());
		}

		public Task<(Stream?, GenerationResultModel)> GenerateThumbnail(string subPath,
			string fileHash, ThumbnailImageFormat imageFormat,
			ThumbnailSize size)
		{
			return Task.FromResult(new ValueTuple<Stream?, GenerationResultModel>(null,
				new GenerationResultModel
				{
					FileHash = fileHash, Size = size, ImageFormat = imageFormat, Success = true
				}));
		}

		public Task<bool> RotateThumbnail(string fileHash, int orientation, int width = 1000,
			int height = 0)
		{
			return Task.FromResult(false);
		}
	}
}

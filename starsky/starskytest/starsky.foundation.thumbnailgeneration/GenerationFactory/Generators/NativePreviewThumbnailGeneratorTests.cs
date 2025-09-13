using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

[TestClass]
public class NativePreviewThumbnailGeneratorTests
{
	private readonly NativePreviewThumbnailGenerator _generator = CreateSut();

	private static NativePreviewThumbnailGenerator CreateSut(bool isSupported = true)
	{
		var storage = new FakeIStorage(["/"],
			["/test.jpg", "/corrupted.jpg"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), Array.Empty<byte>() });

		var selectorStorage = new FakeSelectorStorage(storage);
		var imageNativeService = new FakeIPreviewImageNativeService(storage, isSupported);
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var readMeta = new FakeReadMetaSubPathStorage();
		var existsService = new FakeIFullFilePathExistsService();

		return new NativePreviewThumbnailGenerator(
			selectorStorage,
			imageNativeService,
			logger,
			appSettings,
			readMeta,
			existsService
		);
	}

	[TestMethod]
	public async Task GenerateThumbnail_ShouldReturnResults_InvalidExtension()
	{
		// Arrange
		const string singleSubPath = "test-path";
		const string fileHash = "test-hash";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Small, ThumbnailSize.Large };

		// Act
		var results =
			await _generator.GenerateThumbnail(singleSubPath, fileHash, imageFormat,
				thumbnailSizes);

		// Assert
		Assert.IsNotNull(results);
		foreach ( var result in results )
		{
			Assert.IsFalse(result.Success);
		}
	}

	[TestMethod]
	public async Task GenerateThumbnail_ShouldReturnResults_HappyFlow()
	{
		// Arrange
		const string singleSubPath = "/test.jpg";
		const string fileHash = "/test-hash";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Small, ThumbnailSize.Large };

		// Act
		var results =
			await _generator.GenerateThumbnail(singleSubPath, fileHash, imageFormat,
				thumbnailSizes);

		// Assert
		Assert.IsNotNull(results);
		foreach ( var result in results )
		{
			Assert.IsTrue(result.Success);
		}
	}

	[TestMethod]
	public async Task GenerateThumbnail_ShouldReturnResults_Corrupted()
	{
		// Arrange
		const string singleSubPath = "/corrupted.jpg";
		const string fileHash = "/test-hash";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Small, ThumbnailSize.Large };

		// Act
		var results =
			( await _generator.GenerateThumbnail(singleSubPath, fileHash, imageFormat,
				thumbnailSizes) ).ToList();

		// Assert
		Assert.IsNotNull(results);
		Assert.HasCount(2, results);
		foreach ( var result in results )
		{
			Assert.IsFalse(result.Success);
			Assert.AreEqual("Image cannot be loaded", result.ErrorMessage);
			Assert.AreEqual(ThumbnailImageFormat.jpg, result.ImageFormat);
			Assert.AreEqual(fileHash, result.FileHash);
		}
	}

	[TestMethod]
	public async Task GenerateThumbnail_ServiceNotSupported()
	{
		// Arrange
		const string singleSubPath = "/test.jpg";
		const string fileHash = "/test-hash";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Small, ThumbnailSize.Large };

		// Act
		var results =
			( await CreateSut(false).GenerateThumbnail(singleSubPath, fileHash, imageFormat,
				thumbnailSizes) ).ToList();

		// Assert
		Assert.HasCount(2, results);
		Assert.AreEqual("Native service not supported", results[0].ErrorMessage);
		Assert.IsFalse(results[0].ErrorLog);
		Assert.AreEqual("Native service not supported", results[1].ErrorMessage);
		Assert.IsFalse(results[1].ErrorLog);
	}
}

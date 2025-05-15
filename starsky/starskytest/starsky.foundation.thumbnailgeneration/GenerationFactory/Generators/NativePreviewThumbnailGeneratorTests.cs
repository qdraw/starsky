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
	private readonly NativePreviewThumbnailGenerator _generator;

	public NativePreviewThumbnailGeneratorTests()
	{
		var storage = new FakeIStorage(new List<string> { "/" },
			new List<string> { "/test.jpg" },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var selectorStorage = new FakeSelectorStorage(storage);
		var imageNativeService = new FakeIPreviewImageNativeService(storage);
		var logger = new FakeIWebLogger();
		var appSettings = new AppSettings();
		var readMeta = new FakeReadMetaSubPathStorage();
		var existsService = new FakeIFullFilePathExistsService();

		_generator = new NativePreviewThumbnailGenerator(
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
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.native.PreviewImageNative;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.NativePreview;

[TestClass]
public class NativePreviewHelperTests
{
	private readonly NativePreviewHelper _helper;

	public NativePreviewHelperTests()
	{
		var previewService = new FakeIPreviewImageNativeService();
		var storage = new FakeIStorage(["/"], [
				"/valid-path.jpg"
			],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var tempStorage = new FakeIStorage(["/"], [
				"/temp/test.preview.jpg"
			],
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });
		var appSettings = new AppSettings { TempFolder = "/temp" };
		var readMeta = new FakeReadMetaSubPathStorage();
		var existsService = new FakeIFullFilePathExistsService();
		var logger = new FakeIWebLogger();

		_helper = new NativePreviewHelper(previewService, storage,
			tempStorage, appSettings, readMeta, existsService, logger);
	}

	[TestMethod]
	public async Task NativePreviewImage_ShouldReturnSuccess_WhenValidInput()
	{
		// Arrange
		const string singleSubPath = "/valid-path.jpg";
		const string fileHash = "test-hash";
		const ThumbnailSize thumbnailSize = ThumbnailSize.Small;

		// Act
		var result = await _helper.NativePreviewImage(thumbnailSize, singleSubPath, fileHash);

		// Assert
		Assert.IsTrue(result.IsSuccess);
		
		var extension = new PreviewImageNativeService(new FakeIWebLogger()).FileExtension();
		Assert.AreEqual($"test-hash.preview.{extension}", result.ResultPath);
	}

	[TestMethod]
	public async Task NativePreviewImage_ShouldReturnError_WhenFileDoesNotExist()
	{
		// Arrange
		const string singleSubPath = "/invalid-path.jpg";
		const string fileHash = "test-hash";
		const ThumbnailSize thumbnailSize = ThumbnailSize.Small;

		// Act
		var result = await _helper.NativePreviewImage(thumbnailSize, singleSubPath, fileHash);

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("File does not exist", result.ErrorMessage);
	}

	[TestMethod]
	public void CleanTemporaryFile_ShouldReturnTrue_WhenFileDeleted()
	{
		// Arrange
		const string resultPath = "/temp/test.preview.jpg";
		const SelectorStorage.StorageServices resultPathType =
			SelectorStorage.StorageServices.Temporary;

		// Act
		var result = _helper.CleanTemporaryFile(resultPath, resultPathType);

		// Assert
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void CleanTemporaryFile_ShouldLogError_WhenFileNotDeleted()
	{
		// Arrange
		const string resultPath = "/temp/test.preview.jpg";
		const SelectorStorage.StorageServices
			resultPathType = SelectorStorage.StorageServices.Thumbnail;

		// Act
		var result = _helper.CleanTemporaryFile(resultPath, resultPathType);

		// Assert
		Assert.IsFalse(result);
	}
}

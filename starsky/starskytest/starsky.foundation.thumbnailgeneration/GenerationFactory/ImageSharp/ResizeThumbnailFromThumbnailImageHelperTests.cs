using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.Models;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

[TestClass]
public class ResizeThumbnailFromThumbnailImageHelperTests
{
	private const string TestPath = "test.jpg";
	private const string FileHash = "test";

	private readonly ResizeThumbnailFromThumbnailImageHelper _sut;

	public ResizeThumbnailFromThumbnailImageHelperTests()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { TestPath },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var selectorStorage = new FakeSelectorStorage(storage);
		IWebLogger logger = new FakeIWebLogger();
		_sut = new ResizeThumbnailFromThumbnailImageHelper(selectorStorage, logger);
	}

	[TestMethod]
	public async Task ResizeThumbnailFromThumbnailImage_CorruptInput()
	{
		var storage = new FakeIStorage(
			["/"],
			[TestPath],
			new List<byte[]> { Array.Empty<byte>() });

		var sut = new ResizeThumbnailFromThumbnailImageHelper(
			new FakeSelectorStorage(storage),
			new FakeIWebLogger());

		var result = await sut.ResizeThumbnailFromThumbnailImage(FileHash,
			ThumbnailSize.Large, 1, null, FileHash,
			true, ThumbnailImageFormat.jpg);

		Assert.IsFalse(result.Success);
		Assert.IsFalse(result.IsNotFound);
		Assert.AreEqual("Image cannot be loaded", result.ErrorMessage);
	}

	[TestMethod]
	public async Task ResizeThumbnailFromThumbnailImage_Success()
	{
		var result = await _sut.ResizeThumbnailFromThumbnailImage(
			FileHash, ThumbnailSize.Large, 4,
			"subPath",
			"thumbnailOutputHash", false,
			ThumbnailImageFormat.jpg);

		Assert.IsTrue(result.Success);
		Assert.AreEqual("thumbnailOutputHash", result.FileHash);
		Assert.AreEqual(4, result.SizeInPixels);
		Assert.AreEqual(ThumbnailImageFormat.jpg, result.ImageFormat);
	}

	[TestMethod]
	public async Task ResizeThumbnailFromThumbnailImage_FileNotFound()
	{
		var result = await _sut.ResizeThumbnailFromThumbnailImage(
			"nonExistentFileHash", ThumbnailSize.Small,
			100, "subPath",
			"thumbnailOutputHash",
			false, ThumbnailImageFormat.jpg);

		Assert.IsFalse(result.IsNotFound);
		Assert.IsFalse(result.Success);
		Assert.IsTrue(result.ErrorMessage?.Contains("Image cannot be loaded"));
	}

	[TestMethod]
	public async Task ResizeThumbnailFromThumbnailImage_InvalidParameters()
	{
		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await _sut.ResizeThumbnailFromThumbnailImage(
				"fileHash", ThumbnailSize.Small,
				100, "subPath",
				"", false,
				ThumbnailImageFormat.jpg));
	}

	[TestMethod]
	public async Task ResizeThumbnailFromThumbnailImage_InvalidParameters2()
	{
		await Assert.ThrowsExactlyAsync<InvalidEnumArgumentException>(async () =>
			await _sut.ResizeThumbnailFromThumbnailImage(
				"fileHash", ThumbnailSize.Small,
				100, "subPath",
				"test", false,
				ThumbnailImageFormat.unknown));
	}

	[TestMethod]
	public void SetError_ShouldSetErrorMessageAndLogException()
	{
		// Arrange
		var logger = new FakeIWebLogger();
		var helper = new ResizeThumbnailFromThumbnailImageHelper(new FakeSelectorStorage(), logger);
		var result = new GenerationResultModel
		{
			FileHash = "test",
			SubPath = "test.jpg",
			Success = false,
			ToGenerate = false,
			IsNotFound = false,
			ErrorLog = false,
			Size = ThumbnailSize.Unknown,
			ImageFormat = ThumbnailImageFormat.unknown
		};
		var exception = new Exception("Image cannot be loaded: corrupted file");
		const string fileHash = "testFileHash";
		const string subPathReference = "testSubPath";

		// Act
		var updatedResult = helper.SetError(result, exception, fileHash, subPathReference);

		// Assert
		Assert.IsFalse(updatedResult.Success);
		Assert.AreEqual("Image cannot be loaded", updatedResult.ErrorMessage);
		Assert.AreEqual($"[ResizeThumbnailFromThumbnailImageHelper] " +
		                $"Exception H:{fileHash} M: Image cannot be loaded F: {subPathReference}",
			logger.TrackedExceptions.LastOrDefault().Item2?.Trim());
	}
}

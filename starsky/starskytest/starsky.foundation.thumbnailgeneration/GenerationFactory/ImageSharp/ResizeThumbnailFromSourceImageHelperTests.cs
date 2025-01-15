using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetadataExtractor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.
	ImageSharp;

[TestClass]
public class ResizeThumbnailFromSourceImageHelperTests
{
	private const string TestPath = "test.jpg";
	private const string FileHash = "test";

	private readonly ResizeThumbnailFromSourceImageHelper _sut;

	public ResizeThumbnailFromSourceImageHelperTests()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { TestPath },
			new List<byte[]> { CreateAnImage.Bytes.ToArray() });

		var selectorStorage = new FakeSelectorStorage(storage);
		var logger = new FakeIWebLogger();
		_sut = new ResizeThumbnailFromSourceImageHelper(selectorStorage, logger);
	}

	[TestMethod]
	[DataRow(ThumbnailImageFormat.jpg)]
	[DataRow(ThumbnailImageFormat.png)]
	[DataRow(ThumbnailImageFormat.webp)]
	public async Task ResizeThumbnailFromSourceImage__HostDependency_Format_Test(
		ThumbnailImageFormat thumbnailImageFormat)
	{
		var newImage = new CreateAnImage();
		var iStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		const int width = 4;
		var testOutputHash = Path.Combine(newImage.BasePath,
			"test_hash_" + thumbnailImageFormat);
		var testOutputPath = $"{testOutputHash}@{width}.{thumbnailImageFormat}";

		// string subPath, int width, string outputHash = null,bool
		// removeExif = false,ExtensionRolesHelper.ImageFormat
		// imageFormat = ExtensionRolesHelper.ImageFormat.jpg
		var sut = new ResizeThumbnailFromSourceImageHelper(
			new FakeSelectorStorage(iStorage),
			new FakeIWebLogger());

		var model = await sut.ResizeThumbnailFromSourceImage(
			newImage.FullFilePath, width, testOutputHash,
			true, thumbnailImageFormat);

		var meta = ImageMetadataReader.ReadMetadata(
			iStorage.ReadStream(testOutputPath)).ToList();

		// clean
		File.Delete(testOutputPath);

		// asserts
		Assert.AreEqual(4, ReadMetaExif.GetImageWidthHeight(meta, true));
		Assert.AreEqual(3, ReadMetaExif.GetImageWidthHeight(meta, false));
		Assert.AreEqual(thumbnailImageFormat, model.ImageFormat);
		Assert.IsTrue(model.Success);
		Assert.AreEqual(ThumbnailSize.TinyIcon, model.Size);
	}

	[TestMethod]
	public async Task ResizeThumbnailFromSourceImage_CorruptInput()
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
	public async Task ResizeThumbnailFromSourceImage_Success()
	{
		var result = await _sut.ResizeThumbnailFromSourceImage(
			TestPath, 4, "thumbnailOutputHash",
			false, ThumbnailImageFormat.jpg);

		Assert.IsTrue(result.Success);
		Assert.AreEqual("thumbnailOutputHash", result.FileHash);
		Assert.AreEqual(4, result.SizeInPixels);
		Assert.AreEqual(ThumbnailImageFormat.jpg, result.ImageFormat);
	}

	[TestMethod]
	public async Task ResizeThumbnailFromSourceImage_FileNotFound()
	{
		var result = await _sut.ResizeThumbnailFromSourceImage(
			"non-existing-filepath", 4,
			"thumbnailOutputHash",
			false, ThumbnailImageFormat.jpg);

		Assert.IsFalse(result.Success);
		Assert.AreEqual("Image cannot be loaded", result.ErrorMessage);
	}

	[TestMethod]
	public async Task ResizeThumbnailFromSourceImage_InvalidOutputHash()
	{
		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await _sut.ResizeThumbnailFromSourceImage(
				"non-existing-filepath", 4, "",
				false, ThumbnailImageFormat.jpg));
	}

	[TestMethod]
	public async Task ResizeThumbnailFromSourceImage_InvalidImageFormat()
	{
		await Assert.ThrowsExceptionAsync<InvalidEnumArgumentException>(async () =>
			await _sut.ResizeThumbnailFromSourceImage(
				"non-existing-filepath", 4,
				"thumbnailOutputHash",
				false, ThumbnailImageFormat.unknown));
	}
}

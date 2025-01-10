using System;
using System.Collections.Generic;
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
	[TestMethod]
	[DataRow(ThumbnailImageFormat.jpg)]
	[DataRow(ThumbnailImageFormat.png)]
	public async Task ResizeThumbnailToStream__HostDependency_Format_Test(
		ThumbnailImageFormat thumbnailImageFormat)
	{
		var newImage = new CreateAnImage();
		var iStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());

		// string subPath, int width, string outputHash = null,bool removeExif = false,ExtensionRolesHelper.ImageFormat
		// imageFormat = ExtensionRolesHelper.ImageFormat.jpg
		var sut = new ResizeThumbnailFromSourceImageHelper(new FakeSelectorStorage(iStorage),
			new FakeIWebLogger());

		var (thumb, model) = await sut.ResizeThumbnailFromSourceImage(
			newImage.FullFilePath, 1, null, true, thumbnailImageFormat);
		var meta = ImageMetadataReader.ReadMetadata(new MemoryStream(thumb!.ToArray())).ToList();

		Assert.IsTrue(thumb.CanRead);
		Assert.AreEqual(1, ReadMetaExif.GetImageWidthHeight(meta, true));
		Assert.AreEqual(1, ReadMetaExif.GetImageWidthHeight(meta, false));
		Assert.AreEqual(thumbnailImageFormat, model.ImageFormat);
		Assert.IsTrue(model.Success);
		Assert.AreEqual(ThumbnailSize.Unknown, model.Size);
	}

	[TestMethod]
	public async Task ResizeThumbnailToStream_CorruptImage_MemoryStream()
	{
		var storage = new FakeIStorage(
			["/"],
			["test"],
			new List<byte[]> { Array.Empty<byte>() });

		var sut = new ResizeThumbnailFromSourceImageHelper(new FakeSelectorStorage(storage),
			new FakeIWebLogger());

		var (thumb, model) = await sut.ResizeThumbnailFromSourceImage(
			"test", 1, null, true, ThumbnailImageFormat.jpg);

		Assert.IsNull(thumb);
		Assert.IsFalse(model.Success);
	}
}

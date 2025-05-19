using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Models;
using starsky.foundation.thumbnailmeta.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailmeta.Services;

[TestClass]
public sealed class WriteMetaThumbnailServiceTest
{
	[TestMethod]
	public async Task WriteAndCropFile_Fail_BufferNull()
	{
		var storage = new FakeIStorage([],
			new List<string> { "/test.jpg" }, []);
		var service = new WriteMetaThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings());
		var result = await service.WriteAndCropFile("/test.jpg",
			new OffsetModel(), 0, 0,
			ImageRotation.Rotation.Horizontal);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task WriteAndCropFile_Fail_ImageCantBeLoaded()
	{
		var storage = new FakeIStorage(new List<string>(),
			["/test.jpg"], []); // instead of new byte[0][]
		var service = new WriteMetaThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings());
		var result = await service.WriteAndCropFile("/test.jpg",
			new OffsetModel { Data = new byte[10] }, 0, 0,
			ImageRotation.Rotation.Horizontal);
		Assert.IsFalse(result);
	}

	[DataTestMethod]
	[DataRow(ThumbnailImageFormat.jpg)]
	[DataRow(ThumbnailImageFormat.webp)]
	public async Task WriteAndCropFile_FileIsWritten(ThumbnailImageFormat imageFormat)
	{
		var storage = new FakeIStorage();
		var service = new WriteMetaThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings { ThumbnailImageFormat = imageFormat });
		var result = await service.WriteAndCropFile("test",
			new OffsetModel
			{
				Count = CreateAnImage.Bytes.Length, Data = [.. CreateAnImage.Bytes], Index = 0
			}, 6, 6,
			ImageRotation.Rotation.Horizontal);

		Assert.IsTrue(result);
		Assert.IsTrue(storage.ExistFile(ThumbnailNameHelper.Combine("test",
			ThumbnailSize.TinyMeta, imageFormat)));
	}
}

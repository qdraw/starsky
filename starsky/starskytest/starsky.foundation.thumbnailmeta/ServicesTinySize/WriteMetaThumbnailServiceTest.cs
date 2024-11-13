using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.Models;
using starsky.foundation.thumbnailmeta.ServicesTinySize;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailmeta.ServicesTinySize;

[TestClass]
public sealed class WriteMetaThumbnailServiceTest
{
	[TestMethod]
	public async Task WriteAndCropFile_Fail_BufferNull()
	{
		var storage = new FakeIStorage(new List<string>(),
			new List<string> { "/test.jpg" }, Array.Empty<byte[]>());
		var service = new WriteMetaThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings());
		var result = await service.WriteAndCropFile("/test.jpg", new OffsetModel(), 0, 0,
			RotationModel.Rotation.Horizontal);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task WriteAndCropFile_Fail_ImageCantBeLoaded()
	{
		var storage = new FakeIStorage(new List<string>(),
			new List<string> { "/test.jpg" }, Array.Empty<byte[]>()); // instead of new byte[0][]
		var service = new WriteMetaThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings());
		var result = await service.WriteAndCropFile("/test.jpg",
			new OffsetModel { Data = new byte[10] }, 0, 0,
			RotationModel.Rotation.Horizontal);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task WriteAndCropFile_FileIsWritten()
	{
		var storage = new FakeIStorage();
		var service = new WriteMetaThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings());
		var result = await service.WriteAndCropFile("test",
			new OffsetModel
			{
				Count = CreateAnImage.Bytes.Length,
				Data = CreateAnImage.Bytes.ToArray(),
				Index = 0
			}, 6, 6,
			RotationModel.Rotation.Horizontal);

		Assert.IsTrue(result);
		Assert.IsTrue(
			storage.ExistFile(ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta)));
	}
}

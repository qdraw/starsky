using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
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
		var storage = new FakeIStorage(new List<string>(),
			new List<string> { "/test.jpg" }, Array.Empty<byte[]>());
		var service = new WriteMetaThumbnailService(new FakeSelectorStorage(storage),
			new FakeIWebLogger(), new AppSettings());
		var result = await service.WriteAndCropFile("/test.jpg", new OffsetModel(), 0, 0,
			FileIndexItem.Rotation.Horizontal);
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
			FileIndexItem.Rotation.Horizontal);
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
			FileIndexItem.Rotation.Horizontal);

		Assert.IsTrue(result);
		Assert.IsTrue(
			storage.ExistFile(ThumbnailNameHelper.Combine("test",
				ThumbnailSize.TinyMeta, new AppSettings().ThumbnailImageFormat)));
	}

	[TestMethod]
	public void RotateEnumToDegrees_Horizontal()
	{
		var result =
			WriteMetaThumbnailService.RotateEnumToDegrees(FileIndexItem.Rotation.Horizontal);
		Assert.AreEqual(0, result, 0.00001);
	}

	[TestMethod]
	public void RotateEnumToDegrees_Default()
	{
		var result =
			WriteMetaThumbnailService.RotateEnumToDegrees(FileIndexItem.Rotation.DoNotChange);
		Assert.AreEqual(0, result, 0.00001);
	}

	[TestMethod]
	public void RotateEnumToDegrees_180()
	{
		var result =
			WriteMetaThumbnailService.RotateEnumToDegrees(FileIndexItem.Rotation.Rotate180);
		Assert.AreEqual(180, result, 0.00001);
	}

	[TestMethod]
	public void RotateEnumToDegrees_90()
	{
		var result =
			WriteMetaThumbnailService.RotateEnumToDegrees(FileIndexItem.Rotation.Rotate90Cw);
		Assert.AreEqual(90, result, 0.00001);
	}

	[TestMethod]
	public void RotateEnumToDegrees_270()
	{
		var result =
			WriteMetaThumbnailService.RotateEnumToDegrees(FileIndexItem.Rotation.Rotate270Cw);
		Assert.AreEqual(270, result, 0.00001);
	}
}

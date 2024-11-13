using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.ServicesTinySize;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageA330Raw;
using starskytest.FakeCreateAn.CreateAnImageA6600Raw;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailmeta.ServicesTinySize;

[TestClass]
public sealed class MetaExifThumbnailServiceTest
{
	private readonly FakeIStorage _iStorageFake;

	public MetaExifThumbnailServiceTest()
	{
		_iStorageFake = new FakeIStorage(
			new List<string> { "/" },
			new List<string>
			{
				"/no_thumbnail.jpg",
				"/poppy.jpg",
				ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta),
				"/A330.arw",
				"/A6600.arw"
			},
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(),
				new CreateAnImageWithThumbnail().Bytes.ToArray(),
				CreateAnImage.Bytes.ToArray(),
				new CreateAnImageA330Raw().Bytes.ToArray(),
				new CreateAnImageA6600Raw().Bytes.ToArray()
			}
		);
	}

	[TestMethod]
	public async Task NoThumbnail_InMemoryIntegration()
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();
		var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage,
				new OffsetDataMetaExifThumbnail(selectorStorage, logger),
				new WriteMetaThumbnailService(selectorStorage, logger, new AppSettings()),
				logger)
			.AddMetaThumbnail("/no_thumbnail.jpg", "anything");

		Assert.IsFalse(result.Item1);
	}

	[TestMethod]
	[DataRow("/A330.arw", "meta_image1", 192, 128)]
	[DataRow("/A6600.arw", "meta_image2", 192, 127)]
	[DataRow("/poppy.jpg", "meta_image3", 180, 120)]
	public async Task Image_WithThumbnail_InMemoryIntegrationTest(string subPath, string hash,
		int expectedWidth, int expectedHeight)
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();
		var service = new MetaExifThumbnailService(new AppSettings(), selectorStorage,
			new OffsetDataMetaExifThumbnail(selectorStorage, logger),
			new WriteMetaThumbnailService(selectorStorage, logger, new AppSettings()),
			logger);
		var result = await service
			.AddMetaThumbnail(subPath, $"/{hash}");

		Assert.IsTrue(result.Item1);
		Assert.IsTrue(_iStorageFake.ExistFile($"/{hash}@meta"));

		var readStream = _iStorageFake.ReadStream($"/{hash}@meta");

		var decoder = new DecoderOptions();
		var imageInfo = await Image.IdentifyAsync(decoder, readStream);

		Assert.AreEqual(expectedWidth, imageInfo.Width);
		Assert.AreEqual(expectedHeight, imageInfo.Height);

		_iStorageFake.FileDelete($"/{hash}@meta");
	}

	[TestMethod]
	public async Task AddMetaThumbnail_stringString_Fake_HappyFlow()
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();


		var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage,
				new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(),
				logger)
			.AddMetaThumbnail("/poppy.jpg", "/meta_image");

		Assert.IsTrue(result.Item1);
	}

	[TestMethod]
	public async Task AddMetaThumbnail_stringString_Fake_NoHashIncluded()
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();

		var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage,
				new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(),
				logger)
			.AddMetaThumbnail("/poppy.jpg", null!);

		Assert.IsTrue(result.Item1);
	}


	[TestMethod]
	public async Task AddMetaThumbnail_stringString_Fake_NotFound()
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();

		var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage,
				new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(),
				logger)
			.AddMetaThumbnail("/not-found.jpg", "/meta_image");

		Assert.IsFalse(result.Item1);
	}

	[TestMethod]
	[DataRow("/poppy.jpg")]
	public async Task AddMetaThumbnail_Fake_SingleString_File(string subPath)
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();

		var service = new MetaExifThumbnailService(new AppSettings(),
			selectorStorage,
			new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(),
			logger);
		var result = await service.AddMetaThumbnail(subPath);

		Assert.IsTrue(result.FirstOrDefault().Item1);
	}

	[TestMethod]
	public async Task AddMetaThumbnail_Fake_SingleString_Folder()
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();


		var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage,
				new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(),
				logger)
			.AddMetaThumbnail("/");

		Assert.IsTrue(result.FirstOrDefault().Item1);
	}

	[TestMethod]
	public async Task AddMetaThumbnail_Fake_SingleString_NotFound()
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();

		var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage,
				new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(),
				logger)
			.AddMetaThumbnail("/not_found.jpg");

		Assert.IsFalse(result.FirstOrDefault().Item1);
	}

	[TestMethod]
	public async Task AddMetaThumbnail_Fake_IEnumerableString_NotFound()
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();

		var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage,
				new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(),
				logger)
			.AddMetaThumbnail(new List<(string, string)> { ( "/not_found.jpg", "hash" ) });

		Assert.IsFalse(result.FirstOrDefault().Item1);
	}

	[TestMethod]
	public async Task AddMetaThumbnail_Fake_stringString_NotFound()
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();

		var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage,
				new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(),
				logger)
			.AddMetaThumbnail("/not_found.jpg", "hash");

		Assert.IsFalse(result.Item1);
	}

	[TestMethod]
	public async Task AddMetaThumbnail_Fake_Corrupt()
	{
		await _iStorageFake.WriteStreamAsync(StringToStreamHelper.StringToStream("test"),
			"/poppy_corrupt_22.jpg");

		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();

		var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage,
				new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(),
				logger)
			.AddMetaThumbnail("/poppy_corrupt_22.jpg", "hash");

		Assert.IsFalse(result.Item1);
	}

	[TestMethod]
	public async Task AddMetaThumbnail_Fake_AlReadyExists()
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();

		var result = await new MetaExifThumbnailService(new AppSettings(), selectorStorage,
				new FakeIOffsetDataMetaExifThumbnail(), new FakeIWriteMetaThumbnailService(),
				logger)
			.AddMetaThumbnail("/poppy.jpg", "test");

		Assert.IsTrue(result.Item1);
		Assert.AreEqual("already exist", result.Item4);
	}
}

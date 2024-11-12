using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailmeta.ServicesPreviewSize;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageA330Raw;
using starskytest.FakeCreateAn.CreateAnImageA6600Raw;
using starskytest.FakeCreateAn.CreateAnImageLargePreview;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailmeta.ServicesPreviewSize;

[TestClass]
public class ReadPreviewThumbnailTest
{
	private readonly FakeIStorage _iStorageFake;

	public ReadPreviewThumbnailTest()
	{
		_iStorageFake = new FakeIStorage(
			new List<string> { "/" },
			new List<string>
			{
				"/no_thumbnail.jpg",
				"/poppy.jpg",
				ThumbnailNameHelper.Combine("test", ThumbnailSize.TinyMeta),
				"/A330.arw",
				"/A6600.arw",
				"/13mini.jpg"
			},
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(),
				new CreateAnImageWithThumbnail().Bytes.ToArray(),
				CreateAnImage.Bytes.ToArray(),
				new CreateAnImageA330Raw().BytesFullImage,
				new CreateAnImageA6600Raw().BytesFullImage,
				new CreateAnImageLargePreview().BytesFullImage
			}
		);
	}

	[TestMethod]
	public async Task NoThumbnail_InMemoryIntegration()
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();
		var service = new MetaPreviewThumbnailService(new AppSettings(), selectorStorage,
			new OffsetDataMetaExifPreviewThumbnail(selectorStorage, logger),
			new WritePreviewThumbnailService(selectorStorage, logger, new AppSettings()),
			logger);
		var result = await service.AddPreviewThumbnail("/no_thumbnail.jpg", "anything");

		Assert.IsFalse(result.Item1);
	}

	[TestMethod]
	[DataRow("/A330.arw", "preview_image1", 1000, 668)]
	[DataRow("/A6600.arw", "preview_image2", 1000, 668)]
	[DataRow("/poppy.jpg", "preview_image3", 1000, 120)]
	[DataRow("/13mini.jpg", "preview_image4", 1000, 668)]
	public async Task Image_WithThumbnail_InMemoryIntegrationTest(string subPath, string hash,
		int expectedWidth, int expectedHeight)
	{
		var selectorStorage = new FakeSelectorStorage(_iStorageFake);
		var logger = new FakeIWebLogger();
		var service = new MetaPreviewThumbnailService(new AppSettings(), selectorStorage,
			new OffsetDataMetaExifPreviewThumbnail(selectorStorage, logger),
			new WritePreviewThumbnailService(selectorStorage, logger, new AppSettings()),
			logger);
		var result = await service
			.AddPreviewThumbnail(subPath, $"/{hash}");

		Assert.IsTrue(result.Item1);
		Assert.IsTrue(_iStorageFake.ExistFile($"/{hash}"));
		Assert.IsTrue(_iStorageFake.ExistFile($"/{hash}@300"));

		var exportStream = _iStorageFake.ReadStream($"/{hash}");
		await new StorageHostFullPathFilesystem(new FakeIWebLogger()).WriteStreamAsync(exportStream,
			$"/tmp/{hash}.jpg");

		var readStream = _iStorageFake.ReadStream($"/{hash}");

		var decoder = new DecoderOptions();
		var imageInfo = await Image.IdentifyAsync(decoder, readStream);

		Assert.AreEqual(expectedWidth, imageInfo.Width);
		Assert.AreEqual(expectedHeight, imageInfo.Height);

		_iStorageFake.FileDelete($"/{hash}@preview");
	}
}

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class EmbeddedRawThumbnailServiceTests
{
	private static byte[] CreateMinimalHeicWithJpeg()
	{
		using var ms = new MemoryStream();

		// ftyp box (32 bytes), major brand heic
		ms.Write(
		[
			0x00, 0x00, 0x00, 0x20,
			(byte)'f', (byte)'t', (byte)'y', (byte)'p',
			(byte)'h', (byte)'e', (byte)'i', (byte)'c',
			0x00, 0x00, 0x01, 0x00,
			(byte)'i', (byte)'s', (byte)'o', (byte)'m',
			0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00
		]);

		// minimal JPEG-like payload (scanner requires >= 4096 bytes)
		var jpeg = new byte[5000];
		jpeg[0] = 0xFF;
		jpeg[1] = 0xD8;
		jpeg[2] = 0xFF;
		jpeg[4998] = 0xFF;
		jpeg[4999] = 0xD9;
		ms.Write(jpeg, 0, jpeg.Length);

		return ms.ToArray();
	}

	[TestMethod]
	public async Task TryExtractPreview_FileDoesNotExist_ReturnsFalse()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		var selector = new FakeSelectorStorageByType(storage, storage, storage, storage);
		var service = new EmbeddedRawThumbnailService(logger, selector);

		var result = await service.TryExtractPreview("missing.raw", "output.jpg");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractPreview_UnsupportedFormat_ReturnsFalse()
	{
		var logger = new FakeIWebLogger();
		// Header for an unknown format
		var blob = "\0\0\0\0\0\0\0\0"u8.ToArray();
		var storage = new FakeIStorage(outputSubPathFiles: ["test.unknown"],
			byteListSource: new List<byte[]?> { blob });
		var selector = new FakeSelectorStorageByType(storage, storage, storage, storage);
		var service = new EmbeddedRawThumbnailService(logger, selector);

		var result = await service.TryExtractPreview("test.unknown", "output.jpg");

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractPreview_Exception_ReturnsFalse()
	{
		var logger = new FakeIWebLogger();
		// Passing null to selector will cause Exception in the constructor of extractor or when accessing SubPathStorage
		var service = new EmbeddedRawThumbnailService(logger, null!);

		var result = await service.TryExtractPreview("test.cr2", "output.jpg");

		Assert.IsFalse(result);
		Assert.IsNotEmpty(logger.TrackedExceptions);
	}

	[TestMethod]
	public async Task TryExtractPreview_Arw_CallsTiffExtractor()
	{
		var logger = new FakeIWebLogger();
		// ARW header (TIFF)
		var blob = new byte[ExtensionRolesHelper.ImageFormatByteSize];
		blob[0] = ( byte ) 'I';
		blob[1] = ( byte ) 'I';
		blob[2] = 42;
		blob[3] = 0;

		var storage = new FakeIStorage(outputSubPathFiles: ["test.arw"],
			byteListSource: new List<byte[]?> { blob });
		var selector = new FakeSelectorStorageByType(storage, storage, storage, storage);
		var service = new EmbeddedRawThumbnailService(logger, selector);

		// This will still return false because it's just a header, but it exercises the switch case
		var result = await service.TryExtractPreview("test.arw", "output.jpg");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractPreview_Raf_CallsRafExtractor()
	{
		var logger = new FakeIWebLogger();
		// RAF header
		var blob = "FUJIFILMCCD-RAW "u8.ToArray();

		var storage = new FakeIStorage(outputSubPathFiles: ["test.raf"],
			byteListSource: new List<byte[]?> { blob });
		var selector = new FakeSelectorStorageByType(storage, storage, storage, storage);
		var service = new EmbeddedRawThumbnailService(logger, selector);

		var result = await service.TryExtractPreview("test.raf", "output.jpg");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractPreview_Cr3_CallsContainerExtractor()
	{
		var logger = new FakeIWebLogger();
		// CR3 header (ISOBMFF) - simplified
		var blob = new byte[ExtensionRolesHelper.ImageFormatByteSize];
		blob[4] = ( byte ) 'f';
		blob[5] = ( byte ) 't';
		blob[6] = ( byte ) 'y';
		blob[7] = ( byte ) 'p';
		blob[8] = ( byte ) 'c';
		blob[9] = ( byte ) 'r';
		blob[10] = ( byte ) 'x';
		blob[11] = ( byte ) ' ';

		var storage = new FakeIStorage(outputSubPathFiles: ["test.cr3"],
			byteListSource: new List<byte[]?> { blob });
		var selector = new FakeSelectorStorageByType(storage, storage, storage, storage);
		var service = new EmbeddedRawThumbnailService(logger, selector);

		var result = await service.TryExtractPreview("test.cr3", "output.jpg");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractPreview_X3f_CallsLightweightContainerExtractor()
	{
		var logger = new FakeIWebLogger();
		// X3F header
		var blob = "FOVb"u8.ToArray();

		var storage = new FakeIStorage(outputSubPathFiles: ["test.x3f"],
			byteListSource: new List<byte[]?> { blob });
		var selector = new FakeSelectorStorageByType(storage, storage, storage, storage);
		var service = new EmbeddedRawThumbnailService(logger, selector);

		var result = await service.TryExtractPreview("test.x3f", "output.jpg");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractPreview_Jpg_CallsJpegExifExtractor()
	{
		var logger = new FakeIWebLogger();
		// JPG header
		var blob = new byte[] { 0xFF, 0xD8, 0xFF };

		var storage = new FakeIStorage(outputSubPathFiles: ["test.jpg"],
			byteListSource: new List<byte[]?> { blob });
		var selector = new FakeSelectorStorageByType(storage, storage, storage, storage);
		var service = new EmbeddedRawThumbnailService(logger, selector);

		var result = await service.TryExtractPreview("test.jpg", "output.jpg");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtractPreview_Heic_UsesContainerExtractorAndReturnsTrue()
	{
		var logger = new FakeIWebLogger();
		var subPathStorage = new FakeIStorage(outputSubPathFiles: ["test.heic"],
			byteListSource: new List<byte[]?> { CreateMinimalHeicWithJpeg() });
		var tempStorage = new FakeIStorage(["/"]);
		var selector = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);
		var service = new EmbeddedRawThumbnailService(logger, selector);

		var result = await service.TryExtractPreview("test.heic", "output.jpg");

		Assert.IsTrue(result);
		Assert.IsTrue(tempStorage.ExistFile("output.jpg"));
	}
}

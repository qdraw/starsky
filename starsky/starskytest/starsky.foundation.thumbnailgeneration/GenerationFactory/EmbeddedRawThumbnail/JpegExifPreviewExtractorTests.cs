using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetadataExtractor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Helpers;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail.TiffEmbeded;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starskytest.FakeCreateAn.CreateAnImageA6700PreviewRawJpeg;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class JpegExifPreviewExtractorTests
{
	public TestContext TestContext { get; set; }

	private static byte[] BuildJpegWithApp1(byte[] app1Payload)
	{
		var segLen = app1Payload.Length + 2; // includes length bytes
		using var ms = new MemoryStream();
		// SOI
		ms.WriteByte(0xFF);
		ms.WriteByte(0xD8);
		// APP1 marker
		ms.WriteByte(0xFF);
		ms.WriteByte(0xE1);
		// length (big-endian)
		ms.WriteByte(( byte ) ( ( segLen >> 8 ) & 0xFF ));
		ms.WriteByte(( byte ) ( segLen & 0xFF ));
		ms.Write(app1Payload, 0, app1Payload.Length);
		// EOI
		ms.WriteByte(0xFF);
		ms.WriteByte(0xD9);
		return ms.ToArray();
	}

	[TestMethod]
	public async Task TryExtract_IntegrationTest()
	{
		var path = new CreateAnImageA6700PreviewRawJpeg().FilePathJpeg;

		var host = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		var temp = new FakeIStorage();
		var thumb = new FakeIStorage();
		var selector = new FakeSelectorStorageByType(host, thumb, host, temp);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);
		await extractor.TryExtract(path, "preview.jpg");

		Assert.IsTrue(temp.ExistFile("preview.jpg"));
		var stream1 = temp.ReadStream("preview.jpg");
		var metaPreview = ImageMetadataReader.ReadMetadata(stream1).ToList();
		await stream1.DisposeAsync();
		Assert.AreEqual(1616, ReadMetaExif.GetImageWidthHeight(metaPreview, true));
		Assert.AreEqual(1080, ReadMetaExif.GetImageWidthHeight(metaPreview, false));

		var imageHelper = new ResizeThumbnailFromSourceImageHelper(
			selector,
			new FakeIWebLogger());

		await imageHelper.ResizeThumbnailFromSourceImage(
			"preview.jpg",
			SelectorStorage.StorageServices.Temporary,
			1000, "output",
			true, ThumbnailImageFormat.jpg);

		var stream = selector.Get(SelectorStorage.StorageServices.Thumbnail)
			.ReadStream("output.jpg");
		var meta = ImageMetadataReader.ReadMetadata(stream).ToList();
		await stream.DisposeAsync();

		Assert.AreEqual(1000, ReadMetaExif.GetImageWidthHeight(meta, true));
		Assert.AreEqual(668, ReadMetaExif.GetImageWidthHeight(meta, false));
	}

	[TestMethod]
	public async Task TryExtract_FileMissing_ReturnsFalse()
	{
		var sub = new FakeIStorage();
		var temp = new FakeIStorage();
		var selector = new FakeSelectorStorageByType(sub, sub, sub, temp);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);

		var res = await extractor.TryExtract("missing.jpg", null);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task TryExtract_InvalidSoi_ReturnsFalse()
	{
		var blob = "\0\0"u8.ToArray();
		var sub = new FakeIStorage(outputSubPathFiles: ["f"],
			byteListSource: new List<byte[]?> { blob });
		var temp = new FakeIStorage();
		var selector = new FakeSelectorStorageByType(sub, sub, sub, temp);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);

		var res = await extractor.TryExtract("f", null);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task TryExtract_MinimalJpeg_NoApp1_ReturnsFalse()
	{
		var blob = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 };
		var sub = new FakeIStorage(outputSubPathFiles: ["f"],
			byteListSource: new List<byte[]?> { blob });
		var temp = new FakeIStorage();
		var selector = new FakeSelectorStorageByType(sub, sub, sub, temp);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);

		var res = await extractor.TryExtract("f", null);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task TryExtract_App1NonExif_ReturnsFalse()
	{
		var payload = "NOTEXIF"u8.ToArray();
		var blob = BuildJpegWithApp1(payload);
		var sub = new FakeIStorage(outputSubPathFiles: ["f"],
			byteListSource: new List<byte[]?> { blob });
		var temp = new FakeIStorage();
		var selector = new FakeSelectorStorageByType(sub, sub, sub, temp);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);

		var res = await extractor.TryExtract("f", null);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task TryExtract_App1Exif_NoPreview_ReturnsFalse()
	{
		// Build APP1 payload: Exif\0\0 + TIFF header (II 42, first IFD offset = 8)
		using var tiff = new MemoryStream();
		// header
		await tiff.WriteAsync("Exif\0\0"u8.ToArray(), TestContext.CancellationToken);
		// TIFF header
		tiff.WriteByte(( byte ) 'I');
		tiff.WriteByte(( byte ) 'I');
		tiff.WriteByte(0x2A);
		tiff.WriteByte(0x00);
		// first IFD offset = 8 (little-endian)
		tiff.WriteByte(0x08);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);
		// At offset 8, put entryCount = 0
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);

		var app1Payload = tiff.ToArray();
		var blob = BuildJpegWithApp1(app1Payload);
		var sub = new FakeIStorage(outputSubPathFiles: ["f"],
			byteListSource: new List<byte[]?> { blob });
		var temp = new FakeIStorage();
		var selector = new FakeSelectorStorageByType(sub, sub, sub, temp);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);

		var res = await extractor.TryExtract("f", null);
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task TryExtract_App1Exif_WithPreview_ReturnsTrue_And_WritesTemp()
	{
		// Build a JPEG preview with minimum accepted JPEG size (>= 4096 bytes)
		var preview = new byte[4096];
		// Set JPEG SOI and EOI markers in the buffer
		preview[0] = 0xFF;
		preview[1] = 0xD8;
		preview[2] = 0xFF;
		preview[^2] = 0xFF;
		preview[^1] = 0xD9;

		using var tiff = new MemoryStream();
		await tiff.WriteAsync("Exif\0\0"u8.ToArray(), TestContext.CancellationToken);
		// TIFF header (little endian)
		tiff.WriteByte(( byte ) 'I');
		tiff.WriteByte(( byte ) 'I');
		tiff.WriteByte(0x2A);
		tiff.WriteByte(0x00);
		// first IFD offset = 8
		tiff.WriteByte(0x08);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);

		// Start IFD at offset 8
		// entry count = 2
		tiff.WriteByte(0x02);
		tiff.WriteByte(0x00);

		// Entry 1: Tag 0x0201 (JPEGOffset), type=4 (LONG), count=1, value = offset (we'll set to 100)
		tiff.WriteByte(0x01);
		tiff.WriteByte(0x02); // tag 0x0201 little-endian
		tiff.WriteByte(0x04);
		tiff.WriteByte(0x00); // type LONG
		tiff.WriteByte(0x01);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00); // count=1
		const uint jpegOffset = 100u;
		await tiff.WriteAsync(BitConverter.GetBytes(jpegOffset),
			TestContext.CancellationToken); // little-endian

		// Entry 2: Tag 0x0202 (JPEG LENGTH), type=4, count=1, value = length
		tiff.WriteByte(0x02);
		tiff.WriteByte(0x02);
		tiff.WriteByte(0x04);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x01);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);
		var jpegLength = ( uint ) preview.Length;
		await tiff.WriteAsync(BitConverter.GetBytes(jpegLength),
			TestContext.CancellationToken);

		// next IFD pointer = 0
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);
		tiff.WriteByte(0x00);

		// pad until absolute position (Exif header length + jpegOffset)
		while ( tiff.Length < jpegOffset + 6 )
		{
			tiff.WriteByte(0x00);
		}

		// write preview at offset
		await tiff.WriteAsync(preview, TestContext.CancellationToken);
		var app1Payload = tiff.ToArray();

		// Sanity-check TIFF parsing and direct extraction before embedding into JPEG
		using ( var checkTiffMs = new MemoryStream(app1Payload, 6, app1Payload.Length - 6, false) )
		{
			var headerOk =
				TiffEmbeddedPreviewExtractor.TryParseTiffHeader(checkTiffMs, out var le,
					out var firstIfdOffset);
			Assert.IsTrue(headerOk, "TIFF header should parse");
			Assert.IsTrue(le, "Expected little-endian");
			Assert.AreEqual(8u, firstIfdOffset, "Expected first IFD offset 8");

			// Validate entryCount bytes at the IFD offset are 0x02 0x00 (little-endian 2)
			checkTiffMs.Seek(firstIfdOffset, SeekOrigin.Begin);
			var entryCountBuf = new byte[2];
			var rc = await checkTiffMs.ReadAsync(entryCountBuf.AsMemory(0, 2),
				TestContext.CancellationToken);
			Assert.AreEqual(2, rc, "Should read 2 bytes for entry count");
			Assert.AreEqual(0x02, entryCountBuf[0]);
			Assert.AreEqual(0x00, entryCountBuf[1]);

			var candidates = new List<PreviewCandidate>();
			var ctx = new ParseTraversalContext
			{
				Previews = candidates,
				Visited = [],
				ReferenceInfo = "unit-test",
				RawFlavor = RawFlavor.Unknown
			};

			TiffEmbeddedPreviewExtractor.ParseIfdRecursive(checkTiffMs, firstIfdOffset, le, ctx, 0,
				false);
			Assert.IsNotEmpty(candidates,
				"Expected at least one preview candidate from TIFF");
			var best = SelectBestPreviewHelper.SelectBestPreview(candidates);
			Assert.IsNotNull(best);

			// Try extracting directly from a TIFF stream
			using var outMs = new MemoryStream();
			var previewCandidate =
				new PreviewCandidate
				{
					Offset = best.Offset, Length = best.Length
				};
			var okExtract =
				await TiffEmbeddedPreviewExtractor.ExtractPreviewToStream(checkTiffMs,
					previewCandidate, outMs);
			Assert.IsTrue(okExtract, "ExtractPreviewToStream should succeed");
			outMs.Seek(0, SeekOrigin.Begin);
			var extracted = new byte[outMs.Length];
			var read = await outMs.ReadAsync(extracted, TestContext.CancellationToken);
			Assert.AreEqual(previewCandidate.Length, ( uint ) read);
		}

		var blob = BuildJpegWithApp1(app1Payload);
		var sub = new FakeIStorage(outputSubPathFiles: ["f"],
			byteListSource: new List<byte[]?> { blob });
		var temp = new FakeIStorage();
		var selector = new FakeSelectorStorageByType(sub, sub, sub, temp);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);

		// request that extractor write to outputLargePath so it uses _tempStorage.WriteStreamAsync
		var res = await extractor.TryExtract("f", "out.jpg");
		Assert.IsTrue(res);
		// verify temp storage got written via ReadStream
		await using var written = temp.ReadStream("out.jpg");
		Assert.IsNotNull(written);
		var buf = new byte[preview.Length];
		var r = await written.ReadAsync(buf, TestContext.CancellationToken);
		Assert.AreEqual(preview.Length, r);
		CollectionAssert.AreEqual(preview, buf);
	}

	[TestMethod]
	public async Task TryExtract_StandaloneMarkers_HandlesCorrectly()
	{
		// Standalone markers don't have length bytes. 0xFF01 is a standalone marker.
		using var ms = new MemoryStream();
		ms.WriteByte(0xFF);
		ms.WriteByte(0xD8); // SOI
		ms.WriteByte(0xFF);
		ms.WriteByte(0x01); // Standalone TEM
		ms.WriteByte(0xFF);
		ms.WriteByte(0xD9); // EOI

		var storage = new FakeIStorage(outputSubPathFiles: ["test.jpg"],
			byteListSource: [ms.ToArray()]);
		var selector = new FakeSelectorStorageByType(storage, storage, storage, storage);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract("test.jpg", null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_UnexpectedEndOfStream_ReturnsFalse()
	{
		using var ms = new MemoryStream();
		ms.WriteByte(0xFF);
		ms.WriteByte(0xD8); // SOI
		ms.WriteByte(0xFF);
		ms.WriteByte(0xDB); // DQT but no length

		var storage = new FakeIStorage(outputSubPathFiles: ["test.jpg"],
			byteListSource: [ms.ToArray()]);
		var selector = new FakeSelectorStorageByType(storage, storage, storage, storage);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract("test.jpg", null);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_OutputLargePathNull_ReturnsOk()
	{
		// Test case: outputLargePath is null, so it shouldn't try to write to temp storage
		var path = new CreateAnImageA6700PreviewRawJpeg().FilePathJpeg;

		var host = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		var temp = new FakeIStorage();
		var selector = new FakeSelectorStorageByType(host, temp, host, temp);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);

		var res = await extractor.TryExtract(path, null);
		Assert.IsTrue(res);
	}

	[TestMethod]
	public async Task TryExtract_Exception_ReturnsFalse()
	{
		var sub = new FakeIStorage(new Exception("fail"));
		var selector = new FakeSelectorStorageByType(sub, sub, sub, sub);
		var logger = new FakeIWebLogger();
		var extractor = new JpegExifPreviewExtractor(logger, selector);

		var res = await extractor.TryExtract("f", "out.jpg");
		Assert.IsFalse(res);
	}

	[TestMethod]
	public async Task TryExtract_MultipleFF_AndStandaloneMarkers_HandlesCorrectly()
	{
		using var ms = new MemoryStream();
		ms.WriteByte(0xFF);
		ms.WriteByte(0xD8); // SOI
		ms.WriteByte(0xFF);
		ms.WriteByte(0xFF);
		ms.WriteByte(0xFF);
		ms.WriteByte(0x01); // Standalone TEM with extra FFs
		ms.WriteByte(0xFF);
		ms.WriteByte(0xD0); // Standalone RST0
		ms.WriteByte(0xFF);
		ms.WriteByte(0xE0); // APP0
		ms.WriteByte(0x00);
		ms.WriteByte(0x03); // Length 3
		ms.WriteByte(0x01); // payload
		ms.WriteByte(0xFF);
		ms.WriteByte(0xD9); // EOI

		var storage = new FakeIStorage(outputSubPathFiles: ["test.jpg"],
			byteListSource: [ms.ToArray()]);
		var selector = new FakeSelectorStorageByType(storage, storage, storage, storage);
		var extractor = new JpegExifPreviewExtractor(new FakeIWebLogger(), selector);

		var result = await extractor.TryExtract("test.jpg", null);
		Assert.IsFalse(result);
	}
}

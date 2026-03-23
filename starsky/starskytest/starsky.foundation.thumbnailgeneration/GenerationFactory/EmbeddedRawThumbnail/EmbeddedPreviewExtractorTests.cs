using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Unit tests for EmbeddedPreviewExtractor - TIFF metadata parser
/// </summary>
[TestClass]
public class EmbeddedPreviewExtractorTests
{
	private const string InputSubPath = "/raw/test.dng";
	private const string OutputSubPath = "/tmp/output.jpg";

	private static FakeSelectorStorageByType CreateSelectorStorage(byte[]? inputBytes,
		out FakeIStorage subPathStorage,
		out FakeIStorage tempStorage)
	{
		subPathStorage = inputBytes != null
			? new FakeIStorage(
				outputSubPathFolders: ["/raw"],
				outputSubPathFiles: [InputSubPath],
				byteListSource: [inputBytes])
			: new FakeIStorage(outputSubPathFolders: ["/raw"]);

		tempStorage = new FakeIStorage(outputSubPathFolders: ["/tmp"]);
		var thumbnailStorage = new FakeIStorage();
		var hostStorage = new FakeIStorage();

		return new FakeSelectorStorageByType(subPathStorage, thumbnailStorage, hostStorage,
			tempStorage);
	}

	private static byte[] CreateMinimalTiffHeader(uint firstIfdOffset = 8, bool littleEndian = true)
	{
		var header = new byte[8];
		// Byte order
		header[0] = littleEndian ? ( byte ) 'I' : ( byte ) 'M';
		header[1] = littleEndian ? ( byte ) 'I' : ( byte ) 'M';
		// Magic number (42)
		header[2] = 42;
		header[3] = 0;
		// First IFD offset
		var offset = BitConverter.GetBytes(firstIfdOffset);
		if ( BitConverter.IsLittleEndian != littleEndian )
		{
			Array.Reverse(offset);
		}

		Array.Copy(offset, 0, header, 4, 4);
		return header;
	}

	private static byte[] CreateIfdWithJpegTags(uint jpegOffset = 100, uint jpegLength = 5000)
	{
		var ifd = new byte[2 + 36 + 4]; // count + 3 entries + next IFD pointer
		var pos = 0;

		// Entry count (3 entries, little-endian)
		ifd[pos++] = 3;
		ifd[pos++] = 0;

		// Tag: JPEG offset (0x0201)
		ifd[pos++] = 0x01;
		ifd[pos++] = 0x02;
		ifd[pos++] = 4; // Type LONG
		ifd[pos++] = 0;
		ifd[pos++] = 1; // Count
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		// Value: JPEG offset
		ifd[pos++] = ( byte ) ( jpegOffset & 0xFF );
		ifd[pos++] = ( byte ) ( ( jpegOffset >> 8 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( jpegOffset >> 16 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( jpegOffset >> 24 ) & 0xFF );

		// Tag: JPEG length (0x0202)
		ifd[pos++] = 0x02;
		ifd[pos++] = 0x02;
		ifd[pos++] = 4; // Type LONG
		ifd[pos++] = 0;
		ifd[pos++] = 1; // Count
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		// Value: JPEG length
		ifd[pos++] = ( byte ) ( jpegLength & 0xFF );
		ifd[pos++] = ( byte ) ( ( jpegLength >> 8 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( jpegLength >> 16 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( jpegLength >> 24 ) & 0xFF );

		// Tag: Image width (0x0100)
		ifd[pos++] = 0x00;
		ifd[pos++] = 0x01;
		ifd[pos++] = 4; // Type LONG
		ifd[pos++] = 0;
		ifd[pos++] = 1; // Count
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0x00;
		ifd[pos++] = 0x0C;
		ifd[pos++] = 0x00;
		ifd[pos++] = 0x00; // 3072 in little-endian

		// Next IFD offset (0, little-endian)
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;

		return ifd;
	}

	private static byte[] CreateMinimalJpeg(int size = 5000)
	{
		var jpeg = new byte[size];
		// JPEG SOI marker
		jpeg[0] = 0xFF;
		jpeg[1] = 0xD8;
		// APP0 marker
		jpeg[2] = 0xFF;
		jpeg[3] = 0xE0;
		// Rest is just padding
		for ( var i = 4; i < size - 2; i++ )
		{
			jpeg[i] = 0x00;
		}

		// EOI marker at end
		jpeg[size - 2] = 0xFF;
		jpeg[size - 1] = 0xD9;
		return jpeg;
	}

	[TestMethod]
	public async Task TryExtract_WithValidTiffAndJpeg_ReturnsTrue()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader());
		ms.Write(CreateIfdWithJpegTags());
		ms.Seek(100, SeekOrigin.Begin);
		ms.Write(CreateMinimalJpeg());
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _, out var tempStorage);
		var extractor = new EmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsTrue(result, "Should successfully extract from valid TIFF via storage");
		Assert.IsTrue(tempStorage.ExistFile(OutputSubPath),
			"Output preview should be written to temp storage");
	}

	[TestMethod]
	public async Task TryExtract_WithInvalidMagic_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream();
		var header = new byte[8];
		header[0] = ( byte ) 'X'; // Invalid magic
		header[1] = ( byte ) 'X';
		ms.Write(header);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _, out _);
		var extractor = new EmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsFalse(result, "Should fail with invalid TIFF magic");
	}

	[TestMethod]
	public async Task TryExtract_WithEmptyIfd_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader());
		// Empty IFD (0 entries)
		ms.Write(new byte[6]); // count (0) + next IFD pointer (0)

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _, out _);
		var extractor = new EmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsFalse(result, "Should fail with empty IFD");
	}

	[TestMethod]
	public async Task TryExtract_WithInvalidJpegMarker_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader());
		ms.Write(CreateIfdWithJpegTags());
		ms.Seek(100, SeekOrigin.Begin);
		// Write invalid JPEG data (no SOI marker)
		ms.Write(new byte[5000]);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _, out _);
		var extractor = new EmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsFalse(result, "Should fail with invalid JPEG marker");
	}

	[TestMethod]
	public async Task TryExtract_WithJpegTooSmall_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader());
		ms.Write(CreateIfdWithJpegTags(100, 1024)); // Less than 4KB minimum
		ms.Seek(100, SeekOrigin.Begin);
		ms.Write(CreateMinimalJpeg(1024));
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _, out _);
		var extractor = new EmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsFalse(result, "Should fail with JPEG smaller than 4KB");
	}

	[TestMethod]
	public async Task TryExtract_WithMissingSubPath_ReturnsFalse()
	{
		// Arrange
		var selectorStorage = CreateSelectorStorage(null, out _, out _);
		var extractor = new EmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsFalse(result, "Should fail gracefully for missing subpath source file");
	}

	[TestMethod]
	public async Task TryExtract_WithBigEndianTiff_Processes()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader(8, false));
		// Big-endian IFD
		var ifd = new byte[2 + 12 + 4];
		// Entry count (1)
		ifd[0] = 0;
		ifd[1] = 1;
		// Tag (big-endian): 0x0201
		ifd[2] = 0x02;
		ifd[3] = 0x01;
		// Type: LONG (4)
		ifd[4] = 0x00;
		ifd[5] = 0x04;
		// Count: 1
		ifd[6] = 0x00;
		ifd[7] = 0x00;
		ifd[8] = 0x00;
		ifd[9] = 0x01;
		// Value: offset 100 (big-endian)
		ifd[10] = 0x00;
		ifd[11] = 0x00;
		ifd[12] = 0x00;
		ifd[13] = 0x64;
		// Next IFD
		ifd[14] = 0x00;
		ifd[15] = 0x00;
		ifd[16] = 0x00;
		ifd[17] = 0x00;

		ms.Write(ifd);
		ms.Seek(100, SeekOrigin.Begin);
		ms.Write(CreateMinimalJpeg());
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _, out _);
		var extractor = new EmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert - should process without error
		Assert.IsNotNull(result, "Should process big-endian TIFF");
	}

	[TestMethod]
	public async Task TryExtract_WithOutputPath_WritesJpegToTempStorage()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader());
		ms.Write(CreateIfdWithJpegTags());
		ms.Seek(100, SeekOrigin.Begin);
		ms.Write(CreateMinimalJpeg());

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _, out var tempStorage);
		var extractor = new EmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsTrue(result, "Expected successful extraction to temp storage output");
		Assert.IsTrue(tempStorage.ExistFile(OutputSubPath),
			"Expected output preview written to temp storage");
		using var written = tempStorage.ReadStream(OutputSubPath);
		using var outMs = new MemoryStream();
		await written.CopyToAsync(outMs, TestContext.CancellationToken);
		var extractedBytes = outMs.ToArray();
		Assert.IsGreaterThanOrEqualTo(4096, extractedBytes.Length,
			"Output file should contain valid JPEG data");
		Assert.AreEqual(0xFF, extractedBytes[0], "Output should be valid JPEG");
		Assert.AreEqual(0xD8, extractedBytes[1], "Output should be valid JPEG");
	}

	public TestContext TestContext { get; set; }
}

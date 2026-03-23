using System;
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
public class TiffEmbeddedPreviewExtractorTests
{
	private const string InputSubPath = "/raw/test.dng";
	private const string InputArwSubPath = "/raw/test.arw";
	private const string InputCr2SubPath = "/raw/test.cr2";
	private const string OutputSubPath = "/tmp/output.jpg";

	public TestContext TestContext { get; set; }

	private static FakeSelectorStorageByType CreateSelectorStorage(byte[]? inputBytes,
		string inputSubPath,
		out FakeIStorage subPathStorage,
		out FakeIStorage tempStorage)
	{
		subPathStorage = inputBytes != null
			? new FakeIStorage(
				["/raw"],
				[inputSubPath],
				[inputBytes])
			: new FakeIStorage(["/raw"]);

		tempStorage = new FakeIStorage(["/tmp"]);
		var thumbnailStorage = new FakeIStorage();
		var hostStorage = new FakeIStorage();

		return new FakeSelectorStorageByType(subPathStorage, thumbnailStorage, hostStorage,
			tempStorage);
	}

	private static FakeSelectorStorageByType CreateSelectorStorage(byte[]? inputBytes,
		out FakeIStorage tempStorage)
	{
		return CreateSelectorStorage(inputBytes, InputSubPath, out _,
			out tempStorage);
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
		ifd[pos] = 0;

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

	private static byte[] CreateIfdWithMakerNote(uint makerNoteOffset, uint makerNoteLength)
	{
		var ifd = new byte[2 + 12 + 4];
		var pos = 0;

		ifd[pos++] = 1;
		ifd[pos++] = 0;

		// Tag: MakerNote (0x927C)
		ifd[pos++] = 0x7C;
		ifd[pos++] = 0x92;
		ifd[pos++] = 7; // UNDEFINED
		ifd[pos++] = 0;
		ifd[pos++] = ( byte ) ( makerNoteLength & 0xFF );
		ifd[pos++] = ( byte ) ( ( makerNoteLength >> 8 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( makerNoteLength >> 16 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( makerNoteLength >> 24 ) & 0xFF );
		ifd[pos++] = ( byte ) ( makerNoteOffset & 0xFF );
		ifd[pos++] = ( byte ) ( ( makerNoteOffset >> 8 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( makerNoteOffset >> 16 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( makerNoteOffset >> 24 ) & 0xFF );

		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos] = 0;

		return ifd;
	}

	private static byte[] CreateIfdWithJpegAndMakerNote(uint jpegOffset, uint jpegLength,
		uint width, uint height, uint makerNoteOffset, uint makerNoteLength)
	{
		var ifd = new byte[2 + 5 * 12 + 4];
		var pos = 0;

		ifd[pos++] = 5;
		ifd[pos++] = 0;

		void WriteEntry(ushort tag, ushort type, uint count, uint value)
		{
			ifd[pos++] = ( byte ) ( tag & 0xFF );
			ifd[pos++] = ( byte ) ( ( tag >> 8 ) & 0xFF );
			ifd[pos++] = ( byte ) ( type & 0xFF );
			ifd[pos++] = ( byte ) ( ( type >> 8 ) & 0xFF );
			ifd[pos++] = ( byte ) ( count & 0xFF );
			ifd[pos++] = ( byte ) ( ( count >> 8 ) & 0xFF );
			ifd[pos++] = ( byte ) ( ( count >> 16 ) & 0xFF );
			ifd[pos++] = ( byte ) ( ( count >> 24 ) & 0xFF );
			ifd[pos++] = ( byte ) ( value & 0xFF );
			ifd[pos++] = ( byte ) ( ( value >> 8 ) & 0xFF );
			ifd[pos++] = ( byte ) ( ( value >> 16 ) & 0xFF );
			ifd[pos++] = ( byte ) ( ( value >> 24 ) & 0xFF );
		}

		WriteEntry(0x0100, 4, 1, width);
		WriteEntry(0x0101, 4, 1, height);
		WriteEntry(0x0201, 4, 1, jpegOffset);
		WriteEntry(0x0202, 4, 1, jpegLength);
		WriteEntry(0x927C, 7, makerNoteLength, makerNoteOffset);

		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos] = 0;

		return ifd;
	}

	private static byte[] CreateSonyMakerNote(uint jpegOffset, uint jpegLength,
		bool includeLengthTag = true)
	{
		var entryCount = includeLengthTag ? 2 : 1;
		var makerNote = new byte[2 + entryCount * 12 + 4];
		var pos = 0;

		makerNote[pos++] = ( byte ) entryCount;
		makerNote[pos++] = 0;

		// TagSonyPreviewOffset 0x2010
		makerNote[pos++] = 0x10;
		makerNote[pos++] = 0x20;
		makerNote[pos++] = 4;
		makerNote[pos++] = 0;
		makerNote[pos++] = 1;
		makerNote[pos++] = 0;
		makerNote[pos++] = 0;
		makerNote[pos++] = 0;
		makerNote[pos++] = ( byte ) ( jpegOffset & 0xFF );
		makerNote[pos++] = ( byte ) ( ( jpegOffset >> 8 ) & 0xFF );
		makerNote[pos++] = ( byte ) ( ( jpegOffset >> 16 ) & 0xFF );
		makerNote[pos++] = ( byte ) ( ( jpegOffset >> 24 ) & 0xFF );

		if ( includeLengthTag )
		{
			// TagSonyPreviewLength 0x2011
			makerNote[pos++] = 0x11;
			makerNote[pos++] = 0x20;
			makerNote[pos++] = 4;
			makerNote[pos++] = 0;
			makerNote[pos++] = 1;
			makerNote[pos++] = 0;
			makerNote[pos++] = 0;
			makerNote[pos++] = 0;
			makerNote[pos++] = ( byte ) ( jpegLength & 0xFF );
			makerNote[pos++] = ( byte ) ( ( jpegLength >> 8 ) & 0xFF );
			makerNote[pos++] = ( byte ) ( ( jpegLength >> 16 ) & 0xFF );
			makerNote[pos++] = ( byte ) ( ( jpegLength >> 24 ) & 0xFF );
		}

		makerNote[pos++] = 0;
		makerNote[pos++] = 0;
		makerNote[pos++] = 0;
		makerNote[pos] = 0;

		return makerNote;
	}

	[TestMethod]
	public async Task TryExtract_WithValidTiffAndJpeg_ReturnsTrue()
	{
		// Arrange
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);
		await ms.WriteAsync(CreateIfdWithJpegTags(), TestContext.CancellationToken);
		ms.Seek(100, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(), TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out var tempStorage);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

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
		await ms.WriteAsync(header, TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

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
		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);
		// Empty IFD (0 entries)
		await ms.WriteAsync(new byte[6],
			TestContext.CancellationToken); // count (0) + next IFD pointer (0)

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

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
		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);
		await ms.WriteAsync(CreateIfdWithJpegTags(), TestContext.CancellationToken);
		ms.Seek(100, SeekOrigin.Begin);
		// Write invalid JPEG data (no SOI marker)
		await ms.WriteAsync(new byte[5000], TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

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
		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);
		await ms.WriteAsync(CreateIfdWithJpegTags(100, 1024),
			TestContext.CancellationToken); // Less than 4KB minimum
		ms.Seek(100, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(1024), TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsFalse(result, "Should fail with JPEG smaller than 4KB");
	}

	[TestMethod]
	public async Task TryExtract_WithMissingSubPath_ReturnsFalse()
	{
		// Arrange
		var selectorStorage = CreateSelectorStorage(null, out _);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

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
		await ms.WriteAsync(CreateMinimalTiffHeader(8, false), TestContext.CancellationToken);
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

		await ms.WriteAsync(ifd, TestContext.CancellationToken);
		ms.Seek(100, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(), TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert - should process without error
		Assert.IsFalse(result, "Big-endian file without JPEG length should not extract");
	}

	[TestMethod]
	public async Task TryExtract_WithOutputPath_WritesJpegToTempStorage()
	{
		// Arrange
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);
		await ms.WriteAsync(CreateIfdWithJpegTags(), TestContext.CancellationToken);
		ms.Seek(100, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(), TestContext.CancellationToken);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out var tempStorage);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

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

	[TestMethod]
	public async Task TryExtract_WithSonyMakerNoteOffsetAndLength_ExtractsPreview()
	{
		const uint makerNoteOffset = 128;
		const uint jpegOffset = 512;
		const uint jpegLength = 5200;

		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);
		var makerNote = CreateSonyMakerNote(jpegOffset, jpegLength);
		await ms.WriteAsync(CreateIfdWithMakerNote(makerNoteOffset, ( uint ) makerNote.Length),
			TestContext.CancellationToken);
		ms.Seek(makerNoteOffset, SeekOrigin.Begin);
		await ms.WriteAsync(makerNote, TestContext.CancellationToken);
		ms.Seek(jpegOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(( int ) jpegLength), TestContext.CancellationToken);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), InputArwSubPath,
			out _, out var tempStorage);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		var result = await extractor.TryExtract(InputArwSubPath, OutputSubPath);

		Assert.IsTrue(result, "Sony MakerNote offset/length should extract preview");
		Assert.IsTrue(tempStorage.ExistFile(OutputSubPath),
			"Expected extracted preview written to temp storage");
	}

	[TestMethod]
	public async Task TryExtract_WithSonyMakerNoteOffsetWithoutLength_DetectsEoi()
	{
		const uint makerNoteOffset = 160;
		const uint jpegOffset = 600;
		const uint jpegLength = 5300;

		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);
		var makerNote = CreateSonyMakerNote(jpegOffset, 0, false);
		await ms.WriteAsync(CreateIfdWithMakerNote(makerNoteOffset, ( uint ) makerNote.Length),
			TestContext.CancellationToken);
		ms.Seek(makerNoteOffset, SeekOrigin.Begin);
		await ms.WriteAsync(makerNote, TestContext.CancellationToken);
		ms.Seek(jpegOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(( int ) jpegLength), TestContext.CancellationToken);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), InputArwSubPath,
			out _, out var tempStorage);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		var result = await extractor.TryExtract(InputArwSubPath, OutputSubPath);

		Assert.IsTrue(result, "Sony MakerNote without length should detect JPEG EOI");
		Assert.IsTrue(tempStorage.ExistFile(OutputSubPath),
			"Expected extracted preview written to temp storage");
	}

	[TestMethod]
	public async Task TryExtract_WithCanonMakerNoteScan_FindsLargestJpeg()
	{
		const uint makerNoteOffset = 192;
		const int firstJpegLength = 4500;
		const int secondJpegLength = 6200;

		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);

		var firstJpegOffset = makerNoteOffset + 100;
		var secondJpegOffset = makerNoteOffset + 9000;
		var makerNoteLength = secondJpegOffset + secondJpegLength - makerNoteOffset +
		                      200;
		await ms.WriteAsync(CreateIfdWithMakerNote(makerNoteOffset, makerNoteLength),
			TestContext.CancellationToken);

		ms.Seek(firstJpegOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(firstJpegLength), TestContext.CancellationToken);
		ms.Seek(secondJpegOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(secondJpegLength), TestContext.CancellationToken);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), InputCr2SubPath,
			out _, out var tempStorage);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		var result = await extractor.TryExtract(InputCr2SubPath, OutputSubPath);

		Assert.IsTrue(result, "Canon MakerNote scan should find an embedded JPEG");
		using var written = tempStorage.ReadStream(OutputSubPath);
		using var outMs = new MemoryStream();
		await written.CopyToAsync(outMs, TestContext.CancellationToken);
		Assert.IsGreaterThanOrEqualTo(secondJpegLength, outMs.ToArray().Length,
			"Largest JPEG in MakerNote should be selected");
	}

	private static byte[] CreateLosslessJpeg(int size = 5000)
	{
		var jpeg = new byte[size];
		jpeg[0] = 0xFF;
		jpeg[1] = 0xD8;
		jpeg[2] = 0xFF;
		jpeg[3] = 0xC4; // DHT without DQT → lossless marker
		jpeg[size - 2] = 0xFF;
		jpeg[size - 1] = 0xD9;
		return jpeg;
	}

	[TestMethod]
	public async Task TryExtract_WithStripJpegCompression7_ExtractsPreview()
	{
		const uint stripOffset = 1200;
		const int stripLength = 7000;

		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);

		// IFD0 with Compression=7 + StripOffsets + StripByteCounts
		var ifd = new byte[2 + 3 * 12 + 4];
		var pos = 0;
		ifd[pos++] = 3;
		ifd[pos++] = 0;

		WriteLongEntry(0x0103, 7);
		WriteLongEntry(0x0111, stripOffset);
		WriteLongEntry(0x0117, stripLength);

		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos] = 0;

		await ms.WriteAsync(ifd, TestContext.CancellationToken);
		ms.Seek(stripOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(stripLength), TestContext.CancellationToken);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), InputSubPath, out _,
			out var tempStorage);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		Assert.IsTrue(result, "Compression=7 strip JPEG should be extracted");
		Assert.IsTrue(tempStorage.ExistFile(OutputSubPath), "Expected output preview written");
		return;

		void WriteLongEntry(ushort tag, uint value)
		{
			ifd[pos++] = ( byte ) ( tag & 0xFF );
			ifd[pos++] = ( byte ) ( ( tag >> 8 ) & 0xFF );
			ifd[pos++] = 4;
			ifd[pos++] = 0;
			ifd[pos++] = 1;
			ifd[pos++] = 0;
			ifd[pos++] = 0;
			ifd[pos++] = 0;
			ifd[pos++] = ( byte ) ( value & 0xFF );
			ifd[pos++] = ( byte ) ( ( value >> 8 ) & 0xFF );
			ifd[pos++] = ( byte ) ( ( value >> 16 ) & 0xFF );
			ifd[pos++] = ( byte ) ( ( value >> 24 ) & 0xFF );
		}
	}

	/// <summary>
	///     Reproduces canon_eos_5d_mark_iv_01.cr2 layout:
	///     IFD0  0x0111/0x0117 (count=1) → standard JPEG preview (large)
	///     IFD1  0x0201/0x0202            → 15KB small thumbnail
	///     IFD3  0x0111/0x0117 (count=1) → lossless JPEG (raw, must be skipped)
	///     The extractor must pick IFD0's standard preview and ignore IFD3's lossless JPEG.
	/// </summary>
	[TestMethod]
	public async Task
		TryExtract_WithCanonIfd0StripPreviewAndIfd1SmallThumbnailAndIfd3LosslessRaw_PrefersIfd0Preview()
	{
		const uint ifd0PreviewOffset = 2000;
		const int ifd0PreviewLength = 60000;  // standard JPEG – large preview
		const uint ifd1ThumbOffset = 1000;
		const int ifd1ThumbLength = 5000;     // small standard JPEG thumbnail
		const uint ifd3LosslessOffset = 80000;
		const int ifd3LosslessLength = 130000; // bigger bytes but lossless – must be skipped

		using var ms = new MemoryStream();

		// IFD0: 3 entries (Compression + 0x0111 + 0x0117), next → IFD1
		var ifd0Entries = 3;
		var ifd0Size = 2 + ifd0Entries * 12 + 4;
		var ifd1Offset = ( uint ) ( 8 + ifd0Size );
		var ifd1Entries = 2;
		var ifd1Size = 2 + ifd1Entries * 12 + 4;
		var ifd3Offset = ( uint ) ( ifd1Offset + ifd1Size );

		byte[] MakeIfd(int entryCount, uint nextIfd, params (ushort tag, uint val)[] entries)
		{
			var buf = new byte[2 + entryCount * 12 + 4];
			var p = 0;
			buf[p++] = ( byte ) entryCount;
			buf[p++] = 0;
			foreach ( var (tag, val) in entries )
			{
				buf[p++] = ( byte ) ( tag & 0xFF );
				buf[p++] = ( byte ) ( ( tag >> 8 ) & 0xFF );
				buf[p++] = 4; buf[p++] = 0;        // type LONG
				buf[p++] = 1; buf[p++] = 0; buf[p++] = 0; buf[p++] = 0; // count=1
				buf[p++] = ( byte ) ( val & 0xFF );
				buf[p++] = ( byte ) ( ( val >> 8 ) & 0xFF );
				buf[p++] = ( byte ) ( ( val >> 16 ) & 0xFF );
				buf[p++] = ( byte ) ( ( val >> 24 ) & 0xFF );
			}
			buf[p++] = ( byte ) ( nextIfd & 0xFF );
			buf[p++] = ( byte ) ( ( nextIfd >> 8 ) & 0xFF );
			buf[p++] = ( byte ) ( ( nextIfd >> 16 ) & 0xFF );
			buf[p] = ( byte ) ( ( nextIfd >> 24 ) & 0xFF );
			return buf;
		}

		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);
		await ms.WriteAsync(MakeIfd(ifd0Entries, ifd1Offset, (0x0103, 6), (0x0111, ifd0PreviewOffset),
			(0x0117, ifd0PreviewLength)), TestContext.CancellationToken);
		await ms.WriteAsync(MakeIfd(ifd1Entries, ifd3Offset, (0x0201, ifd1ThumbOffset),
			(0x0202, ifd1ThumbLength)), TestContext.CancellationToken);
		await ms.WriteAsync(MakeIfd(ifd0Entries, 0, (0x0103, 6), (0x0111, ifd3LosslessOffset),
			(0x0117, ifd3LosslessLength)), TestContext.CancellationToken);

		ms.Seek(ifd1ThumbOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(), TestContext.CancellationToken);
		ms.Seek(ifd0PreviewOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(ifd0PreviewLength), TestContext.CancellationToken);
		ms.Seek(ifd3LosslessOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateLosslessJpeg(ifd3LosslessLength), TestContext.CancellationToken);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), InputCr2SubPath,
			out _, out var tempStorage);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		var result = await extractor.TryExtract(InputCr2SubPath, OutputSubPath);

		Assert.IsTrue(result,
			"Should extract IFD0 standard preview ignoring the IFD3 lossless raw strip");
		using var written = tempStorage.ReadStream(OutputSubPath);
		using var outMs = new MemoryStream();
		await written.CopyToAsync(outMs, TestContext.CancellationToken);
		var extracted = outMs.ToArray();
		Assert.IsGreaterThanOrEqualTo(ifd0PreviewLength, extracted.Length,
			"Should pick the IFD0 standard preview, not the larger lossless IFD3 raw data");
		Assert.AreNotEqual(( byte ) 0xC4, extracted[3],
			"Extracted JPEG must not be lossless (FF D8 FF C4)");
	}

	[TestMethod]
	public async Task TryExtract_WithCanonLargeMakerNoteAndSmallIfdThumbnail_PrefersMakerNote()
	{
		const uint makerNoteOffset = 192;
		const uint smallIfdJpegOffset = 1000;
		const int smallIfdJpegLength = 6000;
		const int largeMakerNoteJpegLength = 18000;

		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalTiffHeader(), TestContext.CancellationToken);

		var largeMakerNoteJpegOffset = makerNoteOffset + 9000;
		var makerNoteLength = largeMakerNoteJpegOffset + largeMakerNoteJpegLength -
		                      makerNoteOffset +
		                      200;
		await ms.WriteAsync(CreateIfdWithJpegAndMakerNote(smallIfdJpegOffset,
			smallIfdJpegLength,
			320,
			240,
			makerNoteOffset,
			makerNoteLength), TestContext.CancellationToken);

		ms.Seek(smallIfdJpegOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(smallIfdJpegLength), TestContext.CancellationToken);
		ms.Seek(largeMakerNoteJpegOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(largeMakerNoteJpegLength),
			TestContext.CancellationToken);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), InputCr2SubPath,
			out _, out var tempStorage);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		var result = await extractor.TryExtract(InputCr2SubPath, OutputSubPath);

		Assert.IsTrue(result,
			"Canon extraction should succeed when both IFD and MakerNote JPEGs are present");
		using var written = tempStorage.ReadStream(OutputSubPath);
		using var outMs = new MemoryStream();
		await written.CopyToAsync(outMs, TestContext.CancellationToken);
		Assert.IsGreaterThanOrEqualTo(largeMakerNoteJpegLength, outMs.ToArray().Length,
			"Larger MakerNote preview should win over a small IFD thumbnail");
	}

	[TestMethod]
	public async Task TryExtract_WithAppleIPhoneXsDng_16BitLittleEndian_4By3Aspect_ExtractsLargePreview()
	{
		// Arrange: Simulate Apple iPhone XS DNG characteristics
		// - Little-endian TIFF (iPhone uses 'II' byte order marker)
		// - 16-bit image data
		// - 4:3 aspect ratio (e.g., 4096x3072)
		// - Large embedded JPEG preview >= 50KB
		const uint iPhoneWidth = 4096;
		const uint iPhoneHeight = 3072;
		const uint jpegPreviewOffset = 200;
		const uint jpegPreviewLength = 65000;  // >= 50KB minimum

		using var ms = new MemoryStream();

		// Write little-endian TIFF header (Apple iPhone XS uses little-endian)
		await ms.WriteAsync(CreateMinimalTiffHeader(),
			TestContext.CancellationToken);

		// Create IFD with image dimensions and JPEG preview offset/length
		var ifd = new byte[2 + 4 * 12 + 4];
		var pos = 0;

		// Entry count = 4 (ImageWidth, ImageLength, JPEGInterchangeFormat, JPEGInterchangeFormatLength)
		ifd[pos++] = 4;
		ifd[pos++] = 0;

		// Helper to write a LONG entry (little-endian)
		void WriteLongEntry(ushort tag, uint value)
		{
			ifd[pos++] = ( byte ) ( tag & 0xFF );
			ifd[pos++] = ( byte ) ( ( tag >> 8 ) & 0xFF );
			ifd[pos++] = 4; // Type: LONG
			ifd[pos++] = 0;
			ifd[pos++] = 1; // Count: 1
			ifd[pos++] = 0;
			ifd[pos++] = 0;
			ifd[pos++] = 0;
			ifd[pos++] = ( byte ) ( value & 0xFF );
			ifd[pos++] = ( byte ) ( ( value >> 8 ) & 0xFF );
			ifd[pos++] = ( byte ) ( ( value >> 16 ) & 0xFF );
			ifd[pos++] = ( byte ) ( ( value >> 24 ) & 0xFF );
		}

		// Tag 0x0100: ImageWidth
		WriteLongEntry(0x0100, iPhoneWidth);

		// Tag 0x0101: ImageLength (height)
		WriteLongEntry(0x0101, iPhoneHeight);

		// Tag 0x0201: JPEGInterchangeFormat (offset to JPEG preview)
		WriteLongEntry(0x0201, jpegPreviewOffset);

		// Tag 0x0202: JPEGInterchangeFormatLength (JPEG size in bytes)
		WriteLongEntry(0x0202, jpegPreviewLength);

		// Next IFD offset (0, no more IFDs)
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos] = 0;

		await ms.WriteAsync(ifd, TestContext.CancellationToken);

		// Write the JPEG preview at offset 200
		ms.Seek(jpegPreviewOffset, SeekOrigin.Begin);
		await ms.WriteAsync(CreateMinimalJpeg(( int ) jpegPreviewLength),
			TestContext.CancellationToken);

		ms.Seek(0, SeekOrigin.Begin);

		// Arrange: Create fake storage and extractor
		const string iPhoneDngPath = "/raw/Apple-iPhone-XS-16bit-4_3.dng";
		var selectorStorage = CreateSelectorStorage(ms.ToArray(), iPhoneDngPath,
			out _, out var tempStorage);
		var extractor = new TiffEmbeddedPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(iPhoneDngPath, OutputSubPath);

		// Assert: Verify extraction succeeded
		Assert.IsTrue(result,
			"Apple iPhone XS DNG with little-endian TIFF should successfully extract preview");

		// Assert: Verify output file exists
		Assert.IsTrue(tempStorage.ExistFile(OutputSubPath),
			"Expected extracted JPEG preview written to temp storage");

		// Assert: Verify extracted JPEG has valid markers and size
		await using var written = tempStorage.ReadStream(OutputSubPath);
		using var outMs = new MemoryStream();
		await written.CopyToAsync(outMs, TestContext.CancellationToken);
		var extractedBytes = outMs.ToArray();

		// Verify JPEG SOI marker (0xFF 0xD8)
		Assert.AreEqual(0xFF, extractedBytes[0], "JPEG should start with SOI marker");
		Assert.AreEqual(0xD8, extractedBytes[1], "JPEG should start with SOI marker");

		// Verify extracted preview size is large enough for Apple iPhone XS
		const int minPreviewBytes = 50000;
		Assert.IsGreaterThanOrEqualTo(minPreviewBytes, extractedBytes.Length,
			$"Apple iPhone XS DNG preview payload should be >= {minPreviewBytes} bytes, " +
			$"but got {extractedBytes.Length}");
	}
}

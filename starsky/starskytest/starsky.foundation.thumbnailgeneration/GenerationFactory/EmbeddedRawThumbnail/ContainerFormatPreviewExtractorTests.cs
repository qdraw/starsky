using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Unit tests for ContainerFormatPreviewExtractor - ISOBMFF parser for CR3/HEIF
/// </summary>
[TestClass]
public class ContainerFormatPreviewExtractorTests
{
	private const string InputSubPath = "/raw/test.cr3";
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

	/// <summary>
	///     Creates a minimal ISO Base Media File Format (ISOBMFF) file with ftyp box
	/// </summary>
	private static byte[] CreateMinimalCr3Header(string brand = "crx ")
	{
		var header = new byte[32];
		var pos = 0;

		// ftyp box
		// Box size: 32 bytes (big-endian)
		header[pos++] = 0x00;
		header[pos++] = 0x00;
		header[pos++] = 0x00;
		header[pos++] = 0x20;

		// Box type: 'ftyp'
		header[pos++] = ( byte ) 'f';
		header[pos++] = ( byte ) 't';
		header[pos++] = ( byte ) 'y';
		header[pos++] = ( byte ) 'p';

		// Major brand
		header[pos++] = ( byte ) brand[0];
		header[pos++] = ( byte ) brand[1];
		header[pos++] = ( byte ) brand[2];
		header[pos++] = ( byte ) brand[3];

		// Minor version
		header[pos++] = 0x00;
		header[pos++] = 0x00;
		header[pos++] = 0x01;
		header[pos++] = 0x00;

		// Compatible brands (isom)
		header[pos++] = ( byte ) 'i';
		header[pos++] = ( byte ) 's';
		header[pos++] = ( byte ) 'o';
		header[pos++] = ( byte ) 'm';

		// Padding
		while ( pos < 32 )
		{
			header[pos++] = 0x00;
		}

		return header;
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
	public async Task TryExtract_WithValidCr3Container_ReturnsFalse()
	{
		// Arrange: Create minimal CR3 with FTYP header but no preview
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalCr3Header(), TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert - no preview data, so should return false
		Assert.IsFalse(result, "CR3 without preview should return false");
	}

	[TestMethod]
	public async Task TryExtract_WithCr3ContainerWithJpegPreview_ReturnsTrue()
	{
		// Arrange: Create CR3-like container with embedded JPEG preview
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalCr3Header(), TestContext.CancellationToken);

		// Create a meta box with embedded JPEG preview
		const uint jpegOffset = 256;
		const uint jpegSize = 50000;

		// Add padding to reach JPEG offset
		ms.Seek(32, SeekOrigin.Begin);
		var padding = new byte[jpegOffset - 32];
		await ms.WriteAsync(padding, TestContext.CancellationToken);

		// Write JPEG preview
		await ms.WriteAsync(CreateMinimalJpeg(( int ) jpegSize), TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out var tempStorage);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsTrue(result, "CR3 with JPEG preview should extract successfully");
		Assert.IsTrue(tempStorage.ExistFile(OutputSubPath),
			"Expected extracted JPEG preview written to temp storage");
	}

	[TestMethod]
	public async Task TryExtract_WithInvalidHeader_ReturnsFalse()
	{
		// Arrange - invalid ISOBMFF header
		var invalidHeader = new byte[32];
		invalidHeader[0] = ( byte ) 'X'; // Not 'ftyp'
		invalidHeader[1] = ( byte ) 'X';

		var selectorStorage = CreateSelectorStorage(invalidHeader, out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsFalse(result, "Invalid ISOBMFF header should return false");
	}

	[TestMethod]
	public async Task TryExtract_WithHeifContainer_DetectsFormat()
	{
		// Arrange: Create HEIF (mif1) container with JPEG preview
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalCr3Header("mif1"), TestContext.CancellationToken);

		// Add JPEG preview
		const uint jpegOffset = 256;
		ms.Seek(32, SeekOrigin.Begin);
		var padding = new byte[jpegOffset - 32];
		await ms.WriteAsync(padding, TestContext.CancellationToken);

		await ms.WriteAsync(CreateMinimalJpeg(50000), TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out var tempStorage);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsTrue(result, "HEIF container should extract preview");
		Assert.IsTrue(tempStorage.ExistFile(OutputSubPath),
			"HEIF preview should be written to output");
	}

	[TestMethod]
	public async Task TryExtract_WithMissingFile_ReturnsFalse()
	{
		// Arrange
		var selectorStorage = CreateSelectorStorage(null, out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsFalse(result, "Missing file should return false");
	}

	[TestMethod]
	public async Task TryExtract_WithMultipleJpegPreviews_SelectsLargest()
	{
		// Arrange: Container with multiple JPEG previews (small then large)
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalCr3Header(), TestContext.CancellationToken);

		// Add small JPEG at offset 100
		const uint smallJpegOffset = 100;
		const int smallJpegSize = 8000;
		ms.Seek(32, SeekOrigin.Begin);
		var padding = new byte[smallJpegOffset - 32];
		await ms.WriteAsync(padding, TestContext.CancellationToken);
		await ms.WriteAsync(CreateMinimalJpeg(smallJpegSize), TestContext.CancellationToken);

		// Add large JPEG at offset 20000
		const uint largeJpegOffset = 20000;
		const int largeJpegSize = 75000;
		ms.Seek(smallJpegOffset + smallJpegSize, SeekOrigin.Begin);
		var padding2 = new byte[largeJpegOffset - ( smallJpegOffset + smallJpegSize )];
		await ms.WriteAsync(padding2, TestContext.CancellationToken);
		await ms.WriteAsync(CreateMinimalJpeg(largeJpegSize), TestContext.CancellationToken);

		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out var tempStorage);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsTrue(result, "Should extract largest JPEG preview");
		using var written = tempStorage.ReadStream(OutputSubPath);
		using var outMs = new MemoryStream();
		await written.CopyToAsync(outMs, TestContext.CancellationToken);
		Assert.IsGreaterThanOrEqualTo(largeJpegSize, outMs.ToArray().Length,
			"Should select largest preview found");
	}

	[TestMethod]
	public async Task TryExtract_WithNullOutputPath_ReturnsTrue()
	{
		// Arrange: Create CR3 with JPEG preview, but request NO output file (outputLargePath = null)
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalCr3Header(), TestContext.CancellationToken);

		// Add JPEG preview at offset 256
		ms.Seek(32, SeekOrigin.Begin);
		var padding = new byte[256 - 32];
		await ms.WriteAsync(padding, TestContext.CancellationToken);
		await ms.WriteAsync(CreateMinimalJpeg(50000), TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act: Pass NULL for outputLargePath - extraction succeeds but no file is written
		var result = await extractor.TryExtract(InputSubPath, null);

		// Assert: Should return true because extraction succeeded, even though output was null
		Assert.IsTrue(result,
			"TryExtract should return true when extraction succeeds even with null output path");
	}

	[TestMethod]
	public async Task TryExtract_WithExceptionDuringExtraction_LogsAndReturnsFalse()
	{
		// Arrange: Create a selector storage that throws an exception when reading the input stream
		var fakeLogger = new FakeIWebLogger();
		var subPathStorage = new FakeIStorage(["/raw"]);
		var tempStorage = new FakeIStorage(["/tmp"]);

		var selectorStorage = new FakeSelectorStorageByType(subPathStorage, new FakeIStorage(),
			new FakeIStorage(), tempStorage);

		var extractor = new ContainerFormatPreviewExtractor(fakeLogger, selectorStorage);

		// Act: Call with non-existent file (will throw in ReadStream)
		var result = await extractor.TryExtract("/nonexistent/file.cr3", OutputSubPath);

		// Assert: Should return false due to exception
		Assert.IsFalse(result, "TryExtract should return false when exception occurs");
	}
}

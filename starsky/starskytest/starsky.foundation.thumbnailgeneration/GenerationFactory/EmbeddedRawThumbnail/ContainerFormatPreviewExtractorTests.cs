using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeCreateAn;
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
		var source = (byte[])[.. CreateAnImage.Bytes];
		if ( source.Length >= size )
		{
			return source;
		}

		// Keep the fixture JPEG valid while inflating size by injecting APP15 data after SOI.
		using var ms = new MemoryStream();
		ms.Write(source, 0, 2); // SOI
		var remaining = size - source.Length;
		while ( remaining > 0 )
		{
			var chunk = Math.Min(remaining, 65533); // max APP payload
			ms.WriteByte(0xFF);
			ms.WriteByte(0xEF); // APP15
			var segmentLength = chunk + 2;
			ms.WriteByte(( byte ) ( segmentLength >> 8 ));
			ms.WriteByte(( byte ) segmentLength);
			ms.Write(new byte[chunk], 0, chunk);
			remaining -= chunk;
		}

		ms.Write(source, 2, source.Length - 2);
		return ms.ToArray();
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
	public async Task TryExtract_WithCr3ContainerWithJpegPreview_ReturnsTrue_NoSave()
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

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act - Pass null as outputLargePath
		var result = await extractor.TryExtract(InputSubPath, null);

		// Assert
		Assert.IsTrue(result, "CR3 with JPEG preview should return true even if not saving");
	}

	[TestMethod]
	public async Task TryExtract_Exception_ReturnsFalse()
	{
		// Arrange

		var selectorStorage = new FakeSelectorStorageByType(new FakeIStorageThrowException(),
			new FakeIStorage(), new FakeIStorage(), new FakeIStorage());
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);

		// Act
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);

		// Assert
		Assert.IsFalse(result);
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
	public async Task TryExtract_HeaderTooShort_ReturnsFalse()
	{
		var selectorStorage = CreateSelectorStorage(new byte[10], out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_BoxSizeTooSmall_ReturnsFalse()
	{
		var header = CreateMinimalCr3Header();
		header[0] = 0;
		header[1] = 0;
		header[2] = 0;
		header[3] = 10; // Box size 10 (too small)
		var selectorStorage = CreateSelectorStorage(header, out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_BoxSizeTooLarge_ReturnsFalse()
	{
		var header = CreateMinimalCr3Header();
		header[0] = 0;
		header[1] = 0;
		header[2] = 1;
		header[3] = 0; // Box size 256 (larger than stream)
		var selectorStorage = CreateSelectorStorage(header, out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_UnknownBrand_ReturnsTrueIfJpegFound()
	{
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalCr3Header("xxxx"), TestContext.CancellationToken);
		await ms.WriteAsync(CreateMinimalJpeg(), TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);
		Assert.IsTrue(result);
	}

	[TestMethod]
	public async Task TryExtract_VariousBrands_ReturnsTrueIfJpegFound()
	{
		string[] brands = ["heic", "heix"];
		foreach ( var brand in brands )
		{
			using var ms = new MemoryStream();
			await ms.WriteAsync(CreateMinimalCr3Header(brand), TestContext.CancellationToken);
			await ms.WriteAsync(CreateMinimalJpeg(), TestContext.CancellationToken);
			ms.Seek(0, SeekOrigin.Begin);

			var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
			var extractor =
				new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);
			var result = await extractor.TryExtract(InputSubPath, OutputSubPath);
			Assert.IsTrue(result, $"Should work for brand {brand}");
		}
	}

	[TestMethod]
	public async Task TryExtract_NonSeekableStream_ReturnsFalse()
	{
		var selectorStorage = new FakeSelectorStorageByType(new FakeIStorageNonSeekable(),
			new FakeIStorage(), new FakeIStorage(), new FakeIStorage());
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_TooSmallForJpeg_ReturnsFalse()
	{
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalCr3Header(), TestContext.CancellationToken);
		// Just small data
		await ms.WriteAsync(new byte[100], TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task TryExtract_JpegStartNoEnd_ReturnsFalse()
	{
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalCr3Header(), TestContext.CancellationToken);
		var jpegStart = new byte[] { 0xFF, 0xD8, 0xFF, 0x00, 0x00 };
		await ms.WriteAsync(jpegStart, TestContext.CancellationToken);
		await ms.WriteAsync(new byte[5000], TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);
		Assert.IsFalse(result);
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
	public async Task TryExtract_MultiplePotentiallyIncompleteJpegs_ReturnsFalse()
	{
		using var ms = new MemoryStream();
		await ms.WriteAsync(CreateMinimalCr3Header(), TestContext.CancellationToken);
		// FF D8 FF followed by some data but no FF D9
		await ms.WriteAsync(new byte[] { 0xFF, 0xD8, 0xFF, 0x01, 0x02 },
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[5000], TestContext.CancellationToken);
		// Another start
		await ms.WriteAsync(new byte[] { 0xFF, 0xD8, 0xFF, 0x03, 0x04 },
			TestContext.CancellationToken);
		await ms.WriteAsync(new byte[5000], TestContext.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);

		var selectorStorage = CreateSelectorStorage(ms.ToArray(), out _);
		var extractor = new ContainerFormatPreviewExtractor(new FakeIWebLogger(), selectorStorage);
		var result = await extractor.TryExtract(InputSubPath, OutputSubPath);
		Assert.IsFalse(result);
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
		await using var written = tempStorage.ReadStream(OutputSubPath);
		using var outMs = new MemoryStream();
		await written.CopyToAsync(outMs, TestContext.CancellationToken);
		Assert.IsGreaterThanOrEqualTo(largeJpegSize, outMs.ToArray().Length,
			"Should select largest preview found");
	}

	private sealed class FakeIStorageThrowException : FakeIStorage
	{
		public override bool ExistFile(string path)
		{
			return true;
		}

		public override Stream ReadStream(string path, int maxRead = -1)
		{
			throw new IOException("Fake exception");
		}
	}

	private sealed class FakeIStorageNonSeekable : FakeIStorage
	{
		public override bool ExistFile(string path)
		{
			return true;
		}

		public override Stream ReadStream(string path, int maxRead = -1)
		{
			return new NonSeekableStream(new MemoryStream(CreateMinimalCr3Header()));
		}
	}

	private sealed class NonSeekableStream(Stream inner) : Stream
	{
		public override bool CanRead => inner.CanRead;
		public override bool CanSeek => false;
		public override bool CanWrite => inner.CanWrite;
		public override long Length => inner.Length;

		public override long Position
		{
			get => inner.Position;
			set => throw new NotSupportedException();
		}

		public override void Flush()
		{
			inner.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return inner.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			inner.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			inner.Write(buffer, offset, count);
		}
	}
}

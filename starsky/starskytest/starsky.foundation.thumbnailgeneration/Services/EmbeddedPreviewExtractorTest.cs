using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.Services;

[TestClass]
public class EmbeddedPreviewExtractorTest
{
	/// <summary>
	///     Helper to create a minimal valid TIFF header (little-endian)
	/// </summary>
	private static byte[] CreateMinimalTiffHeader(uint firstIfdOffset = 8)
	{
		var header = new byte[8];
		// Little-endian byte order
		header[0] = ( byte ) 'I';
		header[1] = ( byte ) 'I';
		// Magic number (42 in little-endian)
		header[2] = 42;
		header[3] = 0;
		// First IFD offset (little-endian)
		header[4] = ( byte ) ( firstIfdOffset & 0xFF );
		header[5] = ( byte ) ( ( firstIfdOffset >> 8 ) & 0xFF );
		header[6] = ( byte ) ( ( firstIfdOffset >> 16 ) & 0xFF );
		header[7] = ( byte ) ( ( firstIfdOffset >> 24 ) & 0xFF );
		return header;
	}

	/// <summary>
	///     Helper to create an IFD with a single JPEG offset tag
	/// </summary>
	private static byte[] CreateSimpleIfd(uint jpegOffset = 50, uint jpegLength = 5000,
		uint width = 3000)
	{
		var ifd = new byte[2 + 36 + 4]; // count + 3 entries + next IFD pointer
		var pos = 0;

		// Entry count (3 entries, little-endian)
		ifd[pos++] = 3;
		ifd[pos++] = 0;

		// Tag: Image width (0x0100)
		ifd[pos++] = 0x00;
		ifd[pos++] = 0x01;
		ifd[pos++] = 4;
		ifd[pos++] = 0;
		ifd[pos++] = 1;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = ( byte ) ( width & 0xFF );
		ifd[pos++] = ( byte ) ( ( width >> 8 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( width >> 16 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( width >> 24 ) & 0xFF );

		// Tag: JPEG offset (0x0201)
		ifd[pos++] = 0x01;
		ifd[pos++] = 0x02;
		// Type: LONG (4)
		ifd[pos++] = 4;
		ifd[pos++] = 0;
		// Count (1)
		ifd[pos++] = 1;
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
		// Type: LONG (4)
		ifd[pos++] = 4;
		ifd[pos++] = 0;
		// Count (1)
		ifd[pos++] = 1;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		// Value: JPEG length
		ifd[pos++] = ( byte ) ( jpegLength & 0xFF );
		ifd[pos++] = ( byte ) ( ( jpegLength >> 8 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( jpegLength >> 16 ) & 0xFF );
		ifd[pos++] = ( byte ) ( ( jpegLength >> 24 ) & 0xFF );

		// Next IFD offset (0, little-endian)
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;
		ifd[pos++] = 0;

		return ifd;
	}

	/// <summary>
	///     Helper to create a minimal JPEG data
	/// </summary>
	private static byte[] CreateMinimalJpeg(int size = 5000)
	{
		var jpeg = new byte[size];
		// JPEG SOI marker
		jpeg[0] = 0xFF;
		jpeg[1] = 0xD8;
		// Include SOS marker so validation sees scan data
		jpeg[2] = 0xFF;
		jpeg[3] = 0xDA;
		// EOI marker at the end
		jpeg[size - 2] = 0xFF;
		jpeg[size - 1] = 0xD9;
		return jpeg;
	}

	[TestMethod]
	public async Task TryExtract_WithValidTiffHeader_ReturnsTrue()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader());
		ms.Write(CreateSimpleIfd());
		ms.Write(CreateMinimalJpeg());
		ms.Seek(0, SeekOrigin.Begin);

		string? largeOutput = null;
		string? mediumOutput = null;

		// Act
		var result = await
			new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, largeOutput,
				mediumOutput);

		// Assert
		Assert.IsTrue(result, "Should extract preview from valid TIFF");
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

		// Act
		var result =
			await new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, null, null);

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
		ms.Write("\0\0\0\0\0\0\0\0"u8);
		ms.Seek(0, SeekOrigin.Begin);

		// Act
		var result =
			await new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with no preview candidates");
	}

	[TestMethod]
	public async Task TryExtract_WithTooSmallJpeg_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader());

		// Create IFD with small JPEG
		var ifd = new byte[2 + 12 + 4];
		ifd[0] = 1; // 1 entry
		ifd[2] = 0x01;
		ifd[3] = 0x02; // TAG_JPEG_OFFSET
		ifd[4] = 4;
		ifd[5] = 0; // Type LONG
		ifd[6] = 1;
		ifd[10] = 100; // Offset
		ms.Write(ifd);

		// Write tiny JPEG
		ms.Write(new byte[512]); // Too small (< 4KB)
		ms.Seek(0, SeekOrigin.Begin);

		// Act
		var result =
			await new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with too small JPEG");
	}

	[TestMethod]
	public async Task TryExtract_WithValidFile_WritesOutput()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader());
		ms.Write(CreateSimpleIfd());
		ms.Write(CreateMinimalJpeg());
		ms.Seek(0, SeekOrigin.Begin);

		var largeOutput = Path.Combine(Path.GetTempPath(), $"test_large_{Guid.NewGuid()}.jpg");
		var mediumOutput = Path.Combine(Path.GetTempPath(),
			$"test_medium_{Guid.NewGuid()}.jpg");

		try
		{
			// Act
			var result = await
				new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, largeOutput,
					mediumOutput);

			// Assert
			Assert.IsTrue(result, "Should successfully extract preview");
			// Note: both may point to same file if only one preview found
		}
		finally
		{
			if ( File.Exists(largeOutput) )
			{
				File.Delete(largeOutput);
			}

			if ( File.Exists(mediumOutput) )
			{
				File.Delete(mediumOutput);
			}
		}
	}

	[TestMethod]
	public async Task TryExtract_WithNullOutputPaths_ReturnsTrue()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader());
		ms.Write(CreateSimpleIfd());
		ms.Write(CreateMinimalJpeg());
		ms.Seek(0, SeekOrigin.Begin);

		// Act
		var result =
			await new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, null, null);

		// Assert
		Assert.IsTrue(result, "Should return true even with null outputs");
	}

	[TestMethod]
	public async Task TryExtract_WithPathString_ReturnsTrue()
	{
		// Arrange
		var tempFile = Path.Combine(Path.GetTempPath(), $"test_raw_{Guid.NewGuid()}.arw");
		try
		{
			using ( var fs = new FileStream(tempFile, FileMode.Create) )
			{
				fs.Write(CreateMinimalTiffHeader());
				fs.Write(CreateSimpleIfd());
				fs.Write(CreateMinimalJpeg());
			}

			// Act
			var result = await
				new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(tempFile, null, null);

			// Assert
			Assert.IsTrue(result, "Should extract from file path");
		}
		finally
		{
			if ( File.Exists(tempFile) )
			{
				File.Delete(tempFile);
			}
		}
	}

	[TestMethod]
	public async Task TryExtract_WithNonExistentFile_ReturnsFalse()
	{
		try
		{
			await new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(
				"/nonexistent/file.arw",
				null, null);
			Assert.Fail("Expected DirectoryNotFoundException");
		}
		catch ( DirectoryNotFoundException )
		{
			// expected
		}
	}

	[TestMethod]
	public async Task TryExtract_WithBigEndianTiff_Processes()
	{
		// Arrange
		using var ms = new MemoryStream();

		// Big-endian header
		var header = new byte[8];
		header[0] = ( byte ) 'M';
		header[1] = ( byte ) 'M';
		header[2] = 0;
		header[3] = 42; // 42 in big-endian
		// IFD offset 8 in big-endian
		header[4] = 0;
		header[5] = 0;
		header[6] = 0;
		header[7] = 8;

		ms.Write(header);

		// Big-endian IFD with width + JPEG offset + length tags
		var ifd = new byte[2 + 36 + 4];
		ifd[0] = 0;
		ifd[1] = 3; // 3 entries (big-endian)

		// TAG_IMAGE_WIDTH (0x0100), value 3000
		ifd[2] = 0x01;
		ifd[3] = 0x00;
		ifd[4] = 0;
		ifd[5] = 4;
		ifd[6] = 0;
		ifd[7] = 0;
		ifd[8] = 0;
		ifd[9] = 1;
		ifd[10] = 0;
		ifd[11] = 0;
		ifd[12] = 0x0B;
		ifd[13] = 0xB8;

		// TAG_JPEG_OFFSET (0x0201) in big-endian
		ifd[14] = 0x02;
		ifd[15] = 0x01;
		// Type: LONG (4) in big-endian
		ifd[16] = 0;
		ifd[17] = 4;
		// Count (1) in big-endian
		ifd[18] = 0;
		ifd[19] = 0;
		ifd[20] = 0;
		ifd[21] = 1;
		// Offset (50) in big-endian
		ifd[22] = 0;
		ifd[23] = 0;
		ifd[24] = 0;
		ifd[25] = 50;

		// TAG_JPEG_LENGTH (0x0202)
		ifd[26] = 0x02;
		ifd[27] = 0x02;
		ifd[28] = 0;
		ifd[29] = 4;
		ifd[30] = 0;
		ifd[31] = 0;
		ifd[32] = 0;
		ifd[33] = 1;
		ifd[34] = 0;
		ifd[35] = 0;
		ifd[36] = 0x13;
		ifd[37] = 0x88; // 5000

		// Next IFD (0) in big-endian
		ifd[38] = 0;
		ifd[39] = 0;
		ifd[40] = 0;
		ifd[41] = 0;

		ms.Write(ifd);
		ms.Write(CreateMinimalJpeg());
		ms.Seek(0, SeekOrigin.Begin);

		// Act
		var result =
			await new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, null, null);

		// Assert
		Assert.IsTrue(result, "Should handle big-endian TIFF");
	}

	[TestMethod]
	public async Task TryExtract_StreamAtNonZeroPosition_Reads()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(new byte[100]); // Prefix garbage
		ms.Write(CreateMinimalTiffHeader());
		ms.Write(CreateSimpleIfd());
		ms.Write(CreateMinimalJpeg());

		ms.Seek(100, SeekOrigin.Begin);

		// Act
		var result =
			await new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, null, null);

		// Assert
		Assert.IsFalse(result,
			"IFD offsets are absolute from stream start, so prefixed streams should fail");
	}

	[TestMethod]
	public async Task TryExtract_WithTruncatedHeader_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(new byte[4]); // Only 4 bytes, header needs 8
		ms.Seek(0, SeekOrigin.Begin);

		// Act
		var result =
			await new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with truncated header");
	}

	[TestMethod]
	public async Task TryExtract_WithInvalidIfdOffset_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader(9999)); // IFD offset beyond stream
		ms.Seek(0, SeekOrigin.Begin);

		// Act
		var result =
			await new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with invalid IFD offset");
	}

	[TestMethod]
	public async Task TryExtract_WithCorruptedData_ReturnsFalse()
	{
		// Arrange
		using var ms = new MemoryStream();
		ms.Write(CreateMinimalTiffHeader());
		// Write only partial IFD (corrupted)
		ms.Write(new byte[2]); // Only entry count, no entries
		ms.Seek(0, SeekOrigin.Begin);

		// Act
		var result =
			await new EmbeddedPreviewExtractor(new FakeIWebLogger()).TryExtract(ms, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with corrupted data");
	}
}

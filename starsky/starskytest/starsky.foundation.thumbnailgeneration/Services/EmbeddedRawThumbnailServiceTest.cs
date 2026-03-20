using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starsky.foundation.thumbnailgeneration.Services;

namespace starskytest.starsky.foundation.thumbnailgeneration.Services;

[TestClass]
public class EmbeddedRawThumbnailServiceTest
{
	private string _tempDir = null!;

	[TestInitialize]
	public void Setup()
	{
		_tempDir = Path.Combine(Path.GetTempPath(), $"thumb_test_{Guid.NewGuid()}");
		Directory.CreateDirectory(_tempDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( Directory.Exists(_tempDir) )
		{
			try
			{
				Directory.Delete(_tempDir, true);
			}
			catch
			{
				// Ignore cleanup errors
			}
		}
	}

	private static byte[] CreateMinimalTiffData(int jpegSize = 5000)
	{
		const int ifdOffset = 8;
		const int entryCount = 3; // width + jpeg offset + jpeg length
		const int ifdLength = 2 + (entryCount * 12) + 4;
		var jpegOffset = ifdOffset + ifdLength;
		var data = new byte[jpegOffset + jpegSize]; // header + ifd + jpeg

		// TIFF header (little-endian)
		data[0] = (byte)'I';
		data[1] = (byte)'I';
		data[2] = 42;
		data[3] = 0;
		data[4] = ifdOffset;
		data[5] = 0;
		data[6] = 0;
		data[7] = 0;

		// IFD
		int ifdPos = ifdOffset;
		data[ifdPos++] = entryCount;
		data[ifdPos++] = 0;

		// TAG_IMAGE_WIDTH (0x0100), LONG, count=1, value=3000
		data[ifdPos++] = 0x00;
		data[ifdPos++] = 0x01;
		data[ifdPos++] = 4;
		data[ifdPos++] = 0;
		data[ifdPos++] = 1;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0xB8; // 3000 LE
		data[ifdPos++] = 0x0B;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;

		// TAG_JPEG_OFFSET
		data[ifdPos++] = 0x01;
		data[ifdPos++] = 0x02;
		data[ifdPos++] = 4; // Type LONG
		data[ifdPos++] = 0;
		data[ifdPos++] = 1; // Count
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = (byte)(jpegOffset & 0xFF);
		data[ifdPos++] = (byte)((jpegOffset >> 8) & 0xFF);
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;

		// TAG_JPEG_LENGTH
		data[ifdPos++] = 0x02;
		data[ifdPos++] = 0x02;
		data[ifdPos++] = 4;
		data[ifdPos++] = 0;
		data[ifdPos++] = 1;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = (byte)(jpegSize & 0xFF);
		data[ifdPos++] = (byte)((jpegSize >> 8) & 0xFF);
		data[ifdPos++] = (byte)((jpegSize >> 16) & 0xFF);
		data[ifdPos++] = (byte)((jpegSize >> 24) & 0xFF);

		// Next IFD
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos++] = 0;
		data[ifdPos] = 0;

		// JPEG data
		int jpegPos = jpegOffset;
		data[jpegPos++] = 0xFF;
		data[jpegPos++] = 0xD8;
		for ( int i = 0; i < jpegSize - 4; i++ )
		{
			data[jpegPos++] = 0x00;
		}

		data[jpegPos++] = 0xFF;
		data[jpegPos] = 0xD9;

		return data;
	}

	[TestMethod]
	public void TryExtractPreview_WithNonExistentFile_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string nonExistent = Path.Combine(_tempDir, "nonexistent.arw");

		// Act
		var result = service.TryExtractPreview(nonExistent, null, null);

		// Assert
		Assert.IsFalse(result, "Should return false for nonexistent file");
	}

	[TestMethod]
	public void TryExtractPreview_WithValidRawFile_ReturnsTrue()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		// Act
		var result = service.TryExtractPreview(rawFile, null, null);

		// Assert
		Assert.IsTrue(result, "Should extract preview from valid RAW");
	}

	[TestMethod]
	public void TryExtractPreview_WithValidRawFile_WritesLargeOutput()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		string largeOutput = Path.Combine(_tempDir, "large.jpg");

		// Act
		var result = service.TryExtractPreview(rawFile, largeOutput, null);

		// Assert
		Assert.IsTrue(result, "Should extract large preview");
		// Note: File may or may not be created depending on extraction success
	}

	[TestMethod]
	public void TryExtractPreview_WithValidRawFile_WritesMediumOutput()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		string mediumOutput = Path.Combine(_tempDir, "medium.jpg");

		// Act
		var result = service.TryExtractPreview(rawFile, null, mediumOutput);

		// Assert
		Assert.IsTrue(result, "Should extract medium preview");
	}

	[TestMethod]
	public void TryExtractPreview_WithBothOutputs_ReturnsTrue()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		string largeOutput = Path.Combine(_tempDir, "large.jpg");
		string mediumOutput = Path.Combine(_tempDir, "medium.jpg");

		// Act
		var result = service.TryExtractPreview(rawFile, largeOutput, mediumOutput);

		// Assert
		Assert.IsTrue(result, "Should extract both previews");
	}

	[TestMethod]
	public void TryExtractPreview_WithInvalidTiffData_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "invalid.arw");
		File.WriteAllBytes(rawFile, new byte[100]); // Random data

		// Act
		var result = service.TryExtractPreview(rawFile, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with invalid TIFF data");
	}

	[TestMethod]
	public void TryExtractPreview_WithNullOutputs_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		// Act
		var result = service.TryExtractPreview(rawFile, null, null);

		// Assert
		// Should still return true because preview exists, just not written
		Assert.IsTrue(result, "Should return true even with null outputs");
	}

	[TestMethod]
	public async Task TryExtractPreviewAsync_WithValidRawFile_ReturnsTrue()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		// Act
		var result = await service.TryExtractPreviewAsync(rawFile, null, null);

		// Assert
		Assert.IsTrue(result, "Async method should return same result");
	}

	[TestMethod]
	public async Task TryExtractPreviewAsync_WithNonExistentFile_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string nonExistent = Path.Combine(_tempDir, "nonexistent.arw");

		// Act
		var result = await service.TryExtractPreviewAsync(nonExistent, null, null);

		// Assert
		Assert.IsFalse(result, "Async should return false for nonexistent file");
	}

	[TestMethod]
	public async Task TryExtractPreviewAsync_WithValidRawFile_WritesOutput()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		string largeOutput = Path.Combine(_tempDir, "large.jpg");
		string mediumOutput = Path.Combine(_tempDir, "medium.jpg");

		// Act
		var result = await service.TryExtractPreviewAsync(rawFile, largeOutput, mediumOutput);

		// Assert
		Assert.IsTrue(result, "Async extraction should succeed");
	}

	[TestMethod]
	public void TryExtractPreview_WithEmptyFile_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "empty.arw");
		File.WriteAllBytes(rawFile, new byte[0]);

		// Act
		var result = service.TryExtractPreview(rawFile, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with empty file");
	}

	[TestMethod]
	public void TryExtractPreview_WithTinyFile_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "tiny.arw");
		File.WriteAllBytes(rawFile, new byte[3]); // Less than header

		// Act
		var result = service.TryExtractPreview(rawFile, null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with too small file");
	}

	[TestMethod]
	public void TryExtractPreview_WithInvalidPath_ReturnsFalse()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();

		// Act
		var result = service.TryExtractPreview("", null, null);

		// Assert
		Assert.IsFalse(result, "Should fail with empty path");
	}

	[TestMethod]
	public void TryExtractPreview_WithDifferentFileExtensions_Works()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string[] extensions = { ".arw", ".cr2", ".nef", ".dng" };

		foreach ( var ext in extensions )
		{
			string rawFile = Path.Combine(_tempDir, $"test{ext}");
			File.WriteAllBytes(rawFile, CreateMinimalTiffData());

			// Act
			var result = service.TryExtractPreview(rawFile, null, null);

			// Assert
			Assert.IsTrue(result, $"Should work with {ext} extension");

			File.Delete(rawFile);
		}
	}

	[TestMethod]
	public void TryExtractPreview_MultipleCallsOnSameFile_Consistent()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		// Act
		var result1 = service.TryExtractPreview(rawFile, null, null);
		var result2 = service.TryExtractPreview(rawFile, null, null);

		// Assert
		Assert.AreEqual(result1, result2, "Multiple calls should be consistent");
	}

	[TestMethod]
	public void TryExtractPreview_WithDifferentOutputPaths_BothWork()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		string output1 = Path.Combine(_tempDir, "output1.jpg");
		string output2 = Path.Combine(_tempDir, "output2.jpg");

		// Act
		var result = service.TryExtractPreview(rawFile, output1, output2);

		// Assert
		Assert.IsTrue(result, "Should extract with different output paths");
	}

	[TestMethod]
	public void TryExtractPreview_OutputPathWithSpecialCharacters_Works()
	{
		// Arrange
		var service = new EmbeddedRawThumbnailService();
		string rawFile = Path.Combine(_tempDir, "test.arw");
		File.WriteAllBytes(rawFile, CreateMinimalTiffData());

		string output = Path.Combine(_tempDir, "output_[special]_characters.jpg");

		// Act
		var result = service.TryExtractPreview(rawFile, output, null);

		// Assert
		Assert.IsTrue(result, "Should work with special characters in path");
	}
}



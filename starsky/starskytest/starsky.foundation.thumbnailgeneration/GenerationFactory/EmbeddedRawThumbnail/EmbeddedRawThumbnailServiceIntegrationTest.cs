using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageA6600Raw;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Integration tests for the embedded RAW thumbnail service.
///     Uses real test image files (CreateAnImage and CreateAnImageA6600Raw).
/// </summary>
[TestClass]
public class EmbeddedRawThumbnailServiceIntegrationTest
{
	private string _tempOutputDir = null!;

	public TestContext TestContext { get; set; }

	[TestInitialize]
	public void Setup()
	{
		_tempOutputDir = Path.Combine(Path.GetTempPath(), $"thumb_int_test_{Guid.NewGuid()}");
		Directory.CreateDirectory(_tempOutputDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( !Directory.Exists(_tempOutputDir) )
		{
			return;
		}

		try
		{
			Directory.Delete(_tempOutputDir, true);
		}
		catch
		{
			// Ignore cleanup errors
		}
	}

	[TestMethod]
	public void TryExtractPreview_WithCreateAnImageJpg_ReturnsFalse()
	{
		// Arrange
		var createAnImage = new CreateAnImage();
		File.Exists(createAnImage.FullFilePath);
		var tempImagePath = Path.Combine(_tempOutputDir, "test_image.jpg");
		File.WriteAllBytes(tempImagePath, CreateAnImage.Bytes.ToArray());

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act - JPG is not a RAW file with embedded previews
		var result = service.TryExtractPreview(tempImagePath, null, null);

		// Assert
		Assert.IsFalse(result, "JPEG file should not contain embedded RAW preview");
	}

	[TestMethod]
	public void TryExtractPreview_WithCreateAnImageA6600RawArw_ReturnsTrue()
	{
		// Arrange
		var createAnImageRaw = new CreateAnImageA6600Raw();

		// Skip test if image is empty (missing test file)
		if ( createAnImageRaw.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		var tempRawPath = Path.Combine(_tempOutputDir, "test_raw.arw");
		File.WriteAllBytes(tempRawPath, createAnImageRaw.Bytes.ToArray());

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act
		var result = service.TryExtractPreview(tempRawPath, null, null);

		// Assert
		// A6600 RAW files should contain embedded JPEG previews
		Assert.IsTrue(result, "A6600 RAW file should contain extractable preview");
	}

	[TestMethod]
	public void TryExtractPreview_WithCreateAnImageA6600Raw_WritesOutput()
	{
		// Arrange
		var createAnImageRaw = new CreateAnImageA6600Raw();

		if ( createAnImageRaw.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		var tempRawPath = Path.Combine(_tempOutputDir, "test_raw.arw");
		File.WriteAllBytes(tempRawPath, createAnImageRaw.Bytes.ToArray());

		var largeOutput = Path.Combine(_tempOutputDir, "preview_large.jpg");
		var mediumOutput = Path.Combine(_tempOutputDir, "preview_medium.jpg");

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act
		var result = service.TryExtractPreview(tempRawPath, largeOutput, mediumOutput);

		// Assert
		// Fixture contains only the first 50KB of an ARW; candidate detection can work,
		// but streaming full preview bytes should fail.
		Assert.IsFalse(result, "Truncated RAW head fixture should fail writing preview bytes");
	}

	[TestMethod]
	public void TryExtractPreview_WithCreateAnImageA6600Raw_LargePreviewOnly()
	{
		// Arrange
		var createAnImageRaw = new CreateAnImageA6600Raw();

		if ( createAnImageRaw.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		var tempRawPath = Path.Combine(_tempOutputDir, "test_raw.arw");
		File.WriteAllBytes(tempRawPath, createAnImageRaw.Bytes.ToArray());

		var largeOutput = Path.Combine(_tempOutputDir, "preview_large.jpg");

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act
		var result = service.TryExtractPreview(tempRawPath, largeOutput, null);

		// Assert
		Assert.IsFalse(result,
			"Truncated RAW head fixture should fail when writing large preview bytes");
	}

	[TestMethod]
	public void TryExtractPreview_WithCreateAnImageA6600Raw_MediumPreviewOnly()
	{
		// Arrange
		var createAnImageRaw = new CreateAnImageA6600Raw();

		if ( createAnImageRaw.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		var tempRawPath = Path.Combine(_tempOutputDir, "test_raw.arw");
		File.WriteAllBytes(tempRawPath, createAnImageRaw.Bytes.ToArray());

		var mediumOutput = Path.Combine(_tempOutputDir, "preview_medium.jpg");

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act
		var result = service.TryExtractPreview(tempRawPath, null, mediumOutput);

		// Assert
		Assert.IsTrue(result, "Should extract medium preview only");
	}

	[TestMethod]
	public async Task TryExtractPreviewAsync_WithCreateAnImageA6600Raw_ReturnsTrue()
	{
		// Arrange
		var createAnImageRaw = new CreateAnImageA6600Raw();

		if ( createAnImageRaw.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		var tempRawPath = Path.Combine(_tempOutputDir, "test_raw.arw");
		await File.WriteAllBytesAsync(tempRawPath,
			[.. createAnImageRaw.Bytes], TestContext.CancellationToken);

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act
		var result = await service.TryExtractPreviewAsync(tempRawPath, null, null);

		// Assert
		Assert.IsTrue(result, "Async extraction should work with A6600 RAW");
	}

	[TestMethod]
	public async Task TryExtractPreviewAsync_WithCreateAnImageA6600Raw_WritesOutput()
	{
		// Arrange
		var createAnImageRaw = new CreateAnImageA6600Raw();

		if ( createAnImageRaw.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		var tempRawPath = Path.Combine(_tempOutputDir, "test_raw.arw");
		await File.WriteAllBytesAsync(tempRawPath, [.. createAnImageRaw.Bytes],
			TestContext.CancellationToken);

		var largeOutput = Path.Combine(_tempOutputDir, "preview_large.jpg");
		var mediumOutput = Path.Combine(_tempOutputDir, "preview_medium.jpg");

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act
		var result = await service.TryExtractPreviewAsync(tempRawPath, largeOutput, mediumOutput);

		// Assert
		Assert.IsFalse(result,
			"Async write should fail for truncated RAW head fixture");
	}

	[TestMethod]
	public void TryExtractPreview_WithCreateAnImageA6600Raw_MultipleExtractions()
	{
		// Arrange
		var createAnImageRaw = new CreateAnImageA6600Raw();

		if ( createAnImageRaw.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		var tempRawPath = Path.Combine(_tempOutputDir, "test_raw.arw");
		File.WriteAllBytes(tempRawPath, createAnImageRaw.Bytes.ToArray());

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act - Extract multiple times to same file
		var result1 = service.TryExtractPreview(tempRawPath, null, null);
		var result2 = service.TryExtractPreview(tempRawPath, null, null);
		var result3 = service.TryExtractPreview(tempRawPath, null, null);

		// Assert
		Assert.IsTrue(result1, "First extraction should succeed");
		Assert.IsTrue(result2, "Second extraction should succeed");
		Assert.IsTrue(result3, "Third extraction should succeed");
	}

	[TestMethod]
	public void TryExtractPreview_WithCreateAnImageA6600Raw_DifferentOutputLocations()
	{
		// Arrange
		var createAnImageRaw = new CreateAnImageA6600Raw();

		if ( createAnImageRaw.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		var tempRawPath = Path.Combine(_tempOutputDir, "test_raw.arw");
		File.WriteAllBytes(tempRawPath, createAnImageRaw.Bytes.ToArray());

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		var dir1 = Path.Combine(_tempOutputDir, "dir1");
		var dir2 = Path.Combine(_tempOutputDir, "dir2");
		Directory.CreateDirectory(dir1);
		Directory.CreateDirectory(dir2);

		// Act - Extract to different directories
		var result1 = service.TryExtractPreview(tempRawPath, Path.Combine(dir1, "large.jpg"), null);
		var result2 = service.TryExtractPreview(tempRawPath, Path.Combine(dir2, "large.jpg"), null);

		// Assert
		Assert.IsFalse(result1,
			"Extraction to dir1 should fail for truncated RAW head fixture");
		Assert.IsFalse(result2,
			"Extraction to dir2 should fail for truncated RAW head fixture");
	}

	[TestMethod]
	public void TryExtractPreview_WithCreateAnImageA6600Raw_LargeAndMediumSeparate()
	{
		// Arrange
		var createAnImageRaw = new CreateAnImageA6600Raw();

		if ( createAnImageRaw.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		var tempRawPath = Path.Combine(_tempOutputDir, "test_raw.arw");
		File.WriteAllBytes(tempRawPath, createAnImageRaw.Bytes.ToArray());

		var largeOutput = Path.Combine(_tempOutputDir, "large.jpg");
		var mediumOutput = Path.Combine(_tempOutputDir, "medium.jpg");

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act
		var result = service.TryExtractPreview(tempRawPath, largeOutput, mediumOutput);

		// Assert
		Assert.IsFalse(result,
			"Truncated RAW head fixture should fail writing large/medium preview bytes");
	}

	[TestMethod]
	public void TryExtractPreview_WithCreateAnImageJpg_CreatedAndTested()
	{
		// Arrange
		var createAnImage = new CreateAnImage();
		var jpgPath = createAnImage.FullFilePath;

		// Skip if test image doesn't exist
		if ( !File.Exists(jpgPath) )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		// Act - JPG is not RAW, should fail
		var result = service.TryExtractPreview(jpgPath, null, null);

		// Assert
		Assert.IsFalse(result, "Regular JPEG should not be extracted as RAW");
	}

	[TestMethod]
	public void TryExtractPreview_WithCreateAnImageA6600Raw_VerifyFileType()
	{
		// Arrange
		var createAnImageRaw = new CreateAnImageA6600Raw();

		if ( createAnImageRaw.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Test image file not available");
			return;
		}

		// Verify it's a real ARW file
		Assert.IsNotEmpty(createAnImageRaw.Bytes, "A6600 RAW bytes should not be empty");

		// First 4 bytes should be TIFF header
		var header = new byte[4];
		for ( var i = 0; i < 4; i++ )
		{
			header[i] = createAnImageRaw.Bytes[i];
		}

		var isValidTiff = ( header[0] == 'I' && header[1] == 'I' ) // Little-endian
		                  || ( header[0] == 'M' && header[1] == 'M' ); // Big-endian

		Assert.IsTrue(isValidTiff, "A6600 RAW should be a valid TIFF-based file");
	}
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeCreateAn.CreateAnImageSonyA100ArwRaw;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class CreateAnImageSonyA100ArwRawTest
{
	private string _tempOutputDir = null!;

	[TestInitialize]
	public void Setup()
	{
		_tempOutputDir = Path.Combine(Path.GetTempPath(), $"sony_a100_thumb_test_{Guid.NewGuid()}");
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
			// Ignore cleanup errors.
		}
	}

	[TestMethod]
	public async Task TryExtractPreview_WithCreateAnImageSonyA100Arw_WritesLargeJpeg()
	{
		// Arrange
		var fixture = new CreateAnImageSonyA100ArwRaw();
		if ( fixture.Bytes.IsEmpty )
		{
			Assert.Inconclusive("Sony A100 ARW test image file not available");
			return;
		}

		var largeOutput = Path.Combine(_tempOutputDir, "preview_large.jpg");
		var mediumOutput = Path.Combine(_tempOutputDir, "preview_medium.jpg");

		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger(), new FakeImageOptimisationService());

		// Act
		var result = await service.TryExtractPreview(fixture.FullFilePath,
			largeOutput, mediumOutput);

		// Assert
		Assert.IsTrue(result, "Sony A100 ARW should produce an embedded large preview");
		Assert.IsTrue(File.Exists(largeOutput), "Large preview output should exist");

		var bytes = await File.ReadAllBytesAsync(largeOutput, TestContext.CancellationToken);
		Assert.IsGreaterThan(2, bytes.Length, "Output JPEG should contain data");
		Assert.AreEqual(0xFF, bytes[0], "JPEG SOI marker byte 1");
		Assert.AreEqual(0xD8, bytes[1], "JPEG SOI marker byte 2");
	}

	public TestContext TestContext { get; set; }
}

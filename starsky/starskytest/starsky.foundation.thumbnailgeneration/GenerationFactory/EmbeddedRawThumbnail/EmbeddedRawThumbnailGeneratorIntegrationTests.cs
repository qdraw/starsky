using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starsky.foundation.thumbnailgeneration.Interfaces;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

/// <summary>
///     Integration tests for EmbeddedRawThumbnailGenerator with real RAW files.
///     Tests extraction from actual RAW image files in the test data directory.
/// </summary>
[TestClass]
public class EmbeddedRawThumbnailGeneratorIntegrationTests
{
	private const string TestRawDirectory = "/Users/dion/data/testcontent/raws";
	private const string DngAdobeSample = "20260308_210002_DSC05386-Verbeterd-NR.dng";
	private const int DngAdobeSampleMinLongEdge = 1000;
	private const string Canon5dMarkIvSample = "canon_eos_5d_mark_iv_01.cr2";
	private const int Canon5dMarkIvMinLongEdge = 1200;
	private IEmbeddedRawThumbnailService _embeddedRawThumbnailService = null!;

	private IWebLogger _logger = null!;
	private ISelectorStorage _selectorStorage = null!;

	public TestContext TestContext { get; set; }

	[TestInitialize]
	public void Initialize()
	{
		_logger = new FakeIWebLogger();
		_selectorStorage = new FakeSelectorStorage();
		_embeddedRawThumbnailService =
			new EmbeddedRawThumbnailService(_logger, _selectorStorage);
	}

	private static string[] GetTestFiles()
	{
		// List of test RAW files to process
		var testFiles = new[]
		{
			"20260308_210002_DSC05386-Verbeterd-NR.dng",
			"Sony - ILCE-7SM3 - 14bit 14bit uncompressed (3_2).arw", "RAW_SONY_A700.ARW",
			"RAW_OLYMPUS_E1.ORF", "RAW_NIKON_D50.NEF", "RAW_CANON_EOS_1DX.CR2",
			"nikon_z7_ii_01.nef", "canon_eos_1d_x_mark_iii_01.cr3", "fujifilm_x_s10_01.raf",
			"leica_cl_01.dng", "nikon_d850_01.nef", "panasonic_lumix_gh5_ii_01.rw2",
			"canon_eos_5d_mark_iv_01.cr2"
		};

		return testFiles
			.Where(f => File.Exists(Path.Combine(TestRawDirectory, f)))
			.ToArray();
	}

	[TestMethod]
	[DataRow("20260308_210002_DSC05386-Verbeterd-NR.dng")]
	[DataRow("Sony - ILCE-7SM3 - 14bit 14bit uncompressed (3_2).arw")]
	[DataRow("RAW_SONY_A700.ARW")]
	[DataRow("RAW_NIKON_D50.NEF")]
	[DataRow("RAW_CANON_EOS_1DX.CR2")]
	[DataRow("leica_cl_01.dng")]
	[DataRow("nikon_d850_01.nef")]
	[DataRow("canon_eos_5d_mark_iv_01.cr2")]
	[DataRow("nikon_z7_ii_01.nef")]
	public async Task TryExtractPreview_WithRealRawFile_ExtractsPreview(string fileName)
	{
		var filePath = Path.Combine(TestRawDirectory, fileName);
		if ( !File.Exists(filePath) )
		{
			Assert.Inconclusive($"Test file not found: {filePath}");
		}

		var largePath = Path.Combine(Path.GetTempPath(), $"large_{Guid.NewGuid()}.jpg");

		try
		{
			var result = await _embeddedRawThumbnailService.TryExtractPreview(filePath, largePath);

			// File should be successfully processed
			if ( result )
			{
				Assert.IsTrue(File.Exists(largePath),
					$"At least one preview should be extracted for {fileName}");

				// Verify preview files are valid JPEG by checking magic bytes
				if ( File.Exists(largePath) )
				{
					var bytes =
						await File.ReadAllBytesAsync(largePath, TestContext.CancellationToken);
					Assert.IsGreaterThan(4, bytes.Length, "Extracted preview should have content");
					Assert.AreEqual(0xFF, bytes[0], "JPEG should start with 0xFF");
					Assert.AreEqual(0xD8, bytes[1], "JPEG should start with 0xD8");

					await AssertLargePreviewForKnownSamples(fileName, largePath);

					await WriteImageSharp(largePath);
				}
			}
		}
		finally
		{
			// Cleanup
			try
			{
				if ( File.Exists(largePath) )
				{
					File.Delete(largePath);
				}
			}
			catch
			{
				// Ignore cleanup errors
			}
		}
	}

	private async Task AssertLargePreviewForKnownSamples(string fileName,
		string extractedPreviewPath)
	{
		var isCanon5dMarkIv = fileName.Equals(Canon5dMarkIvSample,
			StringComparison.OrdinalIgnoreCase);
		var isKnownDngSample = fileName.Equals(DngAdobeSample,
			StringComparison.OrdinalIgnoreCase);

		if ( !isCanon5dMarkIv && !isKnownDngSample )
		{
			return;
		}

		using var image =
			await Image.LoadAsync(extractedPreviewPath, TestContext.CancellationToken);
		var longEdge = Math.Max(image.Width, image.Height);
		var minLongEdge = isCanon5dMarkIv
			? Canon5dMarkIvMinLongEdge
			: DngAdobeSampleMinLongEdge;
		Assert.IsGreaterThanOrEqualTo(minLongEdge,
			longEdge,
			$"Expected a large preview for {fileName}, but got {image.Width}x{image.Height}");
	}

	private async Task WriteImageSharp(string path)
	{
		var fakeStorage = new FakeIStorage();
		var outputStream = new MemoryStream();
		using var image = await Image.LoadAsync(path, TestContext.CancellationToken);
		ImageSharpImageResizeHelper.ImageSharpImageResize(image, 1000, true);
		await SaveThumbnailImageFormatHelper.SaveThumbnailImageFormat(image,
			ThumbnailImageFormat.jpg,
			outputStream);
		await fakeStorage.WriteStreamAsync(outputStream, "test_large.jpg");
	}

	[TestMethod]
	public void GetTestFiles_ReturnsAvailableFiles()
	{
		var files = GetTestFiles();

		if ( files.Length == 0 )
		{
			Assert.Inconclusive(
				$"No test RAW files found in {TestRawDirectory}. Some tests will be skipped.");
		}

		Assert.IsNotEmpty(files, "Should find at least one test RAW file");
	}
}

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
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
	private const int DngAdobeSampleMinBytes = 50 * 1024;
	private const string AppleXsDngSample = "Apple - iPhone XS - 16bit (4_3).dng";
	private const int AppleXsDngMinLongEdge = 640;
	private const int AppleXsDngMinBytes = 150 * 1024;
	private const string Canon5dMarkIvSample = "canon_eos_5d_mark_iv_01.cr2";
	private const int Canon5dMarkIvMinLongEdge = 1200;

	private const string HuaweiNoEmbeddedPreviewSample =
		"HUAWEI - EVA-AL00 - 16bit (4_3).dng";

	private const string LeicaLosslessJpegSample =
		"Leica - M (Typ 240) - 16bit 16bit compressed (3_2).dng";

	// NOTE: CR3 files are ISO Base Media containers (not TIFF-based) and require a separate extractor
	// They are currently skipped as TiffEmbeddedPreviewExtractor is TIFF-specific
	private EmbeddedRawThumbnailService _embeddedRawThumbnailService = null!;

	private IWebLogger _logger = null!;
	private ISelectorStorage _selectorStorage = null!;
	private FakeIStorage _tempStorage = null!;

	public TestContext TestContext { get; set; }

	[TestInitialize]
	public void Initialize()
	{
		_logger = new FakeIWebLogger();
		_selectorStorage = new FakeSelectorStorage();
		_embeddedRawThumbnailService = new EmbeddedRawThumbnailService(_logger, _selectorStorage);
	}

	private async Task ConfigureStorageForInputFile(string filePath)
	{
		var bytes = await File.ReadAllBytesAsync(filePath, TestContext.CancellationToken);
		var parent = Path.GetDirectoryName(filePath) ?? "/";
		var subPathStorage = new FakeIStorage([parent],
			[filePath],
			[bytes]);
		_tempStorage = new FakeIStorage(["/tmp"]);
		var thumbnailStorage = new FakeIStorage();
		var hostStorage = new FakeIStorage();
		_selectorStorage = new FakeSelectorStorageByType(subPathStorage, thumbnailStorage,
			hostStorage,
			_tempStorage);
		_embeddedRawThumbnailService = new EmbeddedRawThumbnailService(_logger, _selectorStorage);
	}

	private static string[] GetTestFiles()
	{
		// List of test RAW files to process
		var testFiles = new[]
		{
			"20260308_210002_DSC05386-Verbeterd-NR.dng", "fujifilm_x_t3_01.raf",
			"Sony - ILCE-7SM3 - 14bit 14bit uncompressed (3_2).arw", "RAW_SONY_A700.ARW",
			"RAW_OLYMPUS_E1.ORF", "RAW_NIKON_D50.NEF", "RAW_CANON_EOS_1DX.CR2",
			"nikon_z7_ii_01.nef", "canon_eos_1d_x_mark_iii_01.cr3", "fujifilm_x_s10_01.raf",
			"leica_cl_01.dng", "nikon_d850_01.nef", "panasonic_lumix_gh5_ii_01.rw2",
			"canon_eos_5d_mark_iv_01.cr2", "HUAWEI - EVA-AL00 - 16bit (4_3).dng",
			"Apple - iPhone XS - 16bit (4_3).dng",
			"Leica - M (Typ 240) - 16bit 16bit compressed (3_2).dng",
			"Canon - EOS M200 - CRAW (3_2).CR3", "sigma_sd1_merrill_01.x3f",
			"20221029_101722_DSC05623_3.jpg"
		};

		return testFiles
			.Where(f => File.Exists(Path.Combine(TestRawDirectory, f)))
			.ToArray();
	}

	[TestMethod]
	[DataRow("20260308_210002_DSC05386-Verbeterd-NR.dng")]
	[DataRow("fujifilm_x_t3_01.raf")]
	[DataRow("fujifilm_x_s10_01.raf")]
	[DataRow("Sony - ILCE-7SM3 - 14bit 14bit uncompressed (3_2).arw")]
	[DataRow("RAW_SONY_A700.ARW")]
	[DataRow("RAW_NIKON_D50.NEF")]
	[DataRow("RAW_CANON_EOS_1DX.CR2")]
	[DataRow("leica_cl_01.dng")]
	[DataRow("nikon_d850_01.nef")]
	[DataRow("canon_eos_5d_mark_iv_01.cr2")]
	[DataRow("nikon_z7_ii_01.nef")]
	[DataRow("HUAWEI - EVA-AL00 - 16bit (4_3).dng")]
	[DataRow("Apple - iPhone XS - 16bit (4_3).dng")]
	[DataRow("Leica - M (Typ 240) - 16bit 16bit compressed (3_2).dng")]
	[DataRow("Canon - EOS M200 - CRAW (3_2).CR3")]
	[DataRow("sigma_sd1_merrill_01.x3f")]
	[DataRow("20221029_101722_DSC05623_3.jpg")]
	public async Task TryExtractPreview_WithRealRawFile_ExtractsPreview(string fileName)
	{
		var filePath = Path.Combine(TestRawDirectory, fileName);
		if ( !File.Exists(filePath) )
		{
			Assert.Inconclusive($"Test file not found: {filePath}");
		}

		var largePath = Path.Combine(Path.GetTempPath(), $"large_{Guid.NewGuid()}.jpg");
		await ConfigureStorageForInputFile(filePath);

		try
		{
			var result = await _embeddedRawThumbnailService.TryExtractPreview(filePath, largePath);

			// File should be successfully processed
			if ( result )
			{
				Assert.IsTrue(_tempStorage.ExistFile(largePath),
					$"At least one preview should be extracted for {fileName}");

				await using var stream1 = _tempStorage.ReadStream(largePath);
				await new StorageHostFullPathFilesystem(_logger).WriteStreamAsync(stream1,
					largePath);

				if ( _tempStorage.ExistFile(largePath) )
				{
					await using var stream = _tempStorage.ReadStream(largePath);


					using var outMs = new MemoryStream();
					await stream.CopyToAsync(outMs, TestContext.CancellationToken);
					var bytes = outMs.ToArray();
					Assert.IsGreaterThan(4, bytes.Length, "Extracted preview should have content");
					Assert.AreEqual(0xFF, bytes[0], "JPEG should start with 0xFF");
					Assert.AreEqual(0xD8, bytes[1], "JPEG should start with 0xD8");

					await AssertLargePreviewForKnownSamples(fileName, bytes);

					try
					{
						await WriteImageSharp(bytes);
					}
					catch ( NotSupportedException ex ) when ( ex.Message.Contains("lossless") )
					{
						// ImageSharp doesn't support lossless JPEG decoding
						// This is acceptable - extraction succeeded, just can't validate with ImageSharp
						TestContext.WriteLine(
							$"Skipping ImageSharp validation for {fileName}: {ex.Message}");
					}
				}
				else
				{
					Assert.Fail($"Did not find preview for {fileName}");
				}
			}
			else
			{
				if ( fileName.Equals(HuaweiNoEmbeddedPreviewSample,
					    StringComparison.OrdinalIgnoreCase) )
				{
					Assert.Inconclusive(
						$"No embedded JPEG preview is present in {fileName}; extractor correctly returned false.");
				}

				Assert.Fail($"Failed to extract preview for {fileName}");
			}
		}
		finally
		{
			// Cleanup
			try
			{
				if ( _tempStorage.ExistFile(largePath) )
				{
					_tempStorage.FileDelete(largePath);
				}
			}
			catch
			{
				// Ignore cleanup errors
			}
		}
	}

	private async Task AssertLargePreviewForKnownSamples(string fileName,
		byte[] extractedPreviewBytes)
	{
		var isCanon5dMarkIv = fileName.Equals(Canon5dMarkIvSample,
			StringComparison.OrdinalIgnoreCase);
		var isKnownDngSample = fileName.Equals(DngAdobeSample,
			StringComparison.OrdinalIgnoreCase);
		var isAppleXsDngSample = fileName.Equals(AppleXsDngSample,
			StringComparison.OrdinalIgnoreCase);
		var isLeicaLosslessJpeg = fileName.Equals(LeicaLosslessJpegSample,
			StringComparison.OrdinalIgnoreCase);

		// Skip validation for lossless JPEG files - ImageSharp doesn't support them
		if ( isLeicaLosslessJpeg )
		{
			return;
		}

		if ( !isCanon5dMarkIv && !isKnownDngSample && !isAppleXsDngSample )
		{
			return;
		}

		using var source = new MemoryStream(extractedPreviewBytes);
		using var image = await Image.LoadAsync(source, TestContext.CancellationToken);
		var longEdge = Math.Max(image.Width, image.Height);
		var minLongEdge = DngAdobeSampleMinLongEdge;
		if ( isCanon5dMarkIv )
		{
			minLongEdge = Canon5dMarkIvMinLongEdge;
		}
		else if ( isAppleXsDngSample )
		{
			minLongEdge = AppleXsDngMinLongEdge;
		}

		Assert.IsGreaterThanOrEqualTo(minLongEdge,
			longEdge,
			$"Expected a large preview for {fileName}, but got {image.Width}x{image.Height}");

		if ( isKnownDngSample || isAppleXsDngSample )
		{
			var bytes = extractedPreviewBytes.LongLength;
			var minBytes = isAppleXsDngSample ? AppleXsDngMinBytes : DngAdobeSampleMinBytes;
			Assert.IsGreaterThanOrEqualTo(minBytes,
				bytes,
				$"Expected DNG preview payload >= {minBytes} bytes for {fileName}, but got {bytes}");
		}
	}

	private async Task WriteImageSharp(byte[] sourceBytes)
	{
		var fakeStorage = new FakeIStorage();
		var outputStream = new MemoryStream();
		using var input = new MemoryStream(sourceBytes);
		using var image = await Image.LoadAsync(input, TestContext.CancellationToken);
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

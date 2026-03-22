using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;

[TestClass]
public class EmbeddedRawThumbnailRealWorldIntegrationTest
{
	private string _tempDir = null!;

	[TestInitialize]
	public void Setup()
	{
		_tempDir = Path.Combine(Path.GetTempPath(), $"embedded_raw_real_{Guid.NewGuid()}");
		Directory.CreateDirectory(_tempDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		if ( !Directory.Exists(_tempDir) )
		{
			return;
		}

		try
		{
			Directory.Delete(_tempDir, true);
		}
		catch
		{
			// ignore cleanup issues in test temp folder
		}
	}

	[DataTestMethod]
	[DataRow("fujifilm_x_t3_01.raf")]
	[DataRow("fujifilm_x_s10_01.raf")]
	[DataRow("pentax_k_1_mark_ii_01.dng")]
	[DataRow("hasselblad_x1d_01.fff")]
	public async Task TryExtractPreview_RealWorldRawSamples_WritesDecodablePreview(
		string fileName)
	{
		var root = ResolveRawRoot();
		var inputPath = Path.Combine(root, fileName);
		if ( !File.Exists(inputPath) )
		{
			Assert.Inconclusive($"Raw sample missing: {inputPath}");
			return;
		}

		var large = Path.Combine(_tempDir, $"{fileName}.large.jpg");
		var medium = Path.Combine(_tempDir, $"{fileName}.medium.jpg");
		var service = new EmbeddedRawThumbnailService(new FakeIWebLogger());

		var result = await service.TryExtractPreview(inputPath, large, medium);

		Assert.IsTrue(result, $"Expected preview extraction to succeed for {fileName}");
		Assert.IsTrue(File.Exists(large), $"Expected large preview for {fileName}");
		var bytes = new FileInfo(large).Length;
		Assert.IsTrue(bytes > 1024, $"Expected non-trivial JPEG size for {fileName}");
	}

	private static string ResolveRawRoot()
	{
		var fromEnv = Environment.GetEnvironmentVariable("STARSKY_RAW_TESTDATA_DIR");
		if ( !string.IsNullOrWhiteSpace(fromEnv) )
		{
			return fromEnv;
		}

		return "/Users/dion/data/testcontent/raws";
	}
}


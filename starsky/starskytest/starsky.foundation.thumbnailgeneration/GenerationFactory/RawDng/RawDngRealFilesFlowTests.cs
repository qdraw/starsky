using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class RawDngRealFilesFlowTests
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	public void TryRunToJpeg_WithProvidedRealDngFiles_ReportsPerFileResult()
	{
		string[] files =
		[
			"/Users/dion/data/testcontent/raws/20260308_210002_DSC05386-Verbeterd-NR.dng",
			"/Users/dion/data/testcontent/raws/Apple - iPhone XS - 16bit (4_3).dng",
			"/Users/dion/data/testcontent/raws/Apple iPhone 13 Pro (ProRAW mode) IMG_3234.dng",
			"/Users/dion/data/testcontent/raws/Canon - EOS 5D Mark III - 16bit 16bit RAW.dng",
			"/Users/dion/data/testcontent/raws/DJI - FC7303 - 16bit (16_9).dng",
			"/Users/dion/data/testcontent/raws/Google - Pixel 8 Pro - 16bit (4_3).dng",
			"/Users/dion/data/testcontent/raws/HUAWEI - EVA-AL00 - 16bit (4_3).dng",
			"/Users/dion/data/testcontent/raws/Leica - M (Typ 240) - 16bit 16bit compressed (3_2).dng",
			"/Users/dion/data/testcontent/raws/leica_cl_01.dng",
			"/Users/dion/data/testcontent/raws/OnePlus - ONEPLUS A6003 - 16bit (4_3).dng",
			"/Users/dion/data/testcontent/raws/Pentax - K-3 II - 14bit (3_2).dng",
			"/Users/dion/data/testcontent/raws/Pentax - K-S1 - 12bit (3_2).dng",
			"/Users/dion/data/testcontent/raws/pentax_k_1_mark_ii_01.dng",
			"/Users/dion/data/testcontent/raws/Plustek - OpticFilm 8100 - 16bit (4_3).dng",
			"/Users/dion/data/testcontent/raws/RAW_LEICA_M8.dng",
			"/Users/dion/data/testcontent/raws/Samsung - Galaxy S22 Ulra - 4_3.dng",
			"/Users/dion/data/testcontent/raws/Xiaomi - Redmi Note 7 - 16bit (4_3).dng"
		];

		var failed = new List<string>();
		var succeeded = new List<string>();
		var unsupported = new List<string>();

		foreach ( var file in files )
		{
			if ( !File.Exists(file) )
			{
				failed.Add($"MISSING|{file}");
				continue;
			}

			using var input = File.OpenRead(file);
			using var output = new MemoryStream();
			var ok = RawDngPipelineRunner.TryRunToJpeg(input, output, out var error);
			var result = $"{( ok ? "OK" : "FAIL" )}|{output.Length}|{error}|{file}";
			TestContext.WriteLine(result);
			if ( ok )
			{
				succeeded.Add(result);
			}
			else if ( IsExpectedUnsupported(error) )
			{
				unsupported.Add(result);
			}
			else
			{
				failed.Add(result);
			}
		}
		
		TestContext.WriteLine(
			$"SUMMARY|ok={succeeded.Count}|unsupported={unsupported.Count}|fail={failed.Count}");
		Assert.IsGreaterThan(0, succeeded.Count,
			"Expected at least one DNG file to pass current subset pipeline.");


		if ( failed.Count > 0 )
		{
			Assert.Fail("DNG flow failures:\n" + string.Join("\n", failed));
		}
	}

	private static bool IsExpectedUnsupported(string error)
	{
		return error.StartsWith("Only uncompressed DNG is supported", System.StringComparison.OrdinalIgnoreCase)
		       || error.StartsWith("Unsupported bits per sample", System.StringComparison.OrdinalIgnoreCase);
	}
}

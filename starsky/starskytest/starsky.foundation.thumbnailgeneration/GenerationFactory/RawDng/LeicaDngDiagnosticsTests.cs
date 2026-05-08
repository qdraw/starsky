using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;

[TestClass]
public class LeicaDngDiagnosticsTests
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	[Ignore("Run manually to diagnose specific Leica files")]
	public void DiagnoseLeicaFiles()
	{
		string[] files =
		[
			"/Users/dion/data/testcontent/main/raws/leica_cl_01.dng",
			"/Users/dion/data/testcontent/main/raws/RAW_LEICA_M8.dng",
			"/Users/dion/data/testcontent/main/raws/Leica - M (Typ 240) - 16bit 16bit compressed (3_2).dng"
		];

		foreach ( var file in files )
		{
			if ( !File.Exists(file) )
			{
				TestContext.WriteLine($"MISSING: {file}");
				continue;
			}

			var diagnostics = LeicaDngDiagnostics.AnalyzeDngFile(file);
			TestContext.WriteLine(diagnostics);
			TestContext.WriteLine("\n" + new string('=', 80) + "\n");
		}
	}
}

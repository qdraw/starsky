using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace starskytest.FakeCreateAn.CreateAnZipFileMacOs;

public class CreateAnZipFileMacOs
{
	public CreateAnZipFileMacOs()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) )
		{
			return;
		}

		var path = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnZipFileMacOs", "macOsSubfolder.zip");
		FilePath = path;
	}

	public string FilePath { get; set; } = string.Empty;

	public static IEnumerable<string> Content { get; set; } =
		new List<string> { "__MACOSX/._ffmpeg" };
}

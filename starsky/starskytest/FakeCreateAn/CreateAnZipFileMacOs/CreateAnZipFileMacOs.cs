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

	/// <summary>
	///     Skip the __MACOSX/.ffmpeg file
	/// </summary>
	public static List<string> Content { get; set; } = new() { "ffmpeg", "__MACOSX/.ffmpeg" };
}

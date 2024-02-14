using System.IO;
using System.Reflection;

namespace starskytest.FakeCreateAn.CreateFakeStarskyExe;

public class CreateFakeStarskyUnixBash
{
	public CreateFakeStarskyUnixBash()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) ) return;
		var parentFolder = Path.Combine(dirName, "FakeCreateAn",
			"CreateFakeStarskyExe");
		var path = Path.Combine(parentFolder, "starsky");
		FullFilePath = path;
		StarskyDotStarskyPath = Path.Combine(parentFolder, "starsky.starsky");
	}

	public string StarskyDotStarskyPath { get; set; } = string.Empty;

	public string FullFilePath { get; set; } = string.Empty;
}

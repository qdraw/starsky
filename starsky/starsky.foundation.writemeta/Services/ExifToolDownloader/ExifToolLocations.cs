using System.IO;
using starsky.foundation.platform.Models;

namespace starsky.foundation.writemeta.Services.ExifToolDownloader;

public class ExifToolLocations(AppSettings appSettings)
{
	public string ExeExifToolFullFilePath(bool isWindows)
	{
		return isWindows ? ExeExifToolWindowsFullFilePath() : ExeExifToolUnixFullFilePath();
	}

	internal string ExeExifToolWindowsFullFilePath()
	{
		return Path.Combine(Path.Combine(appSettings.DependenciesFolder, "exiftool-windows"),
			"exiftool.exe");
	}

	internal string ExeExifToolUnixFullFilePath()
	{
		var path = Path.Combine(appSettings.DependenciesFolder,
			"exiftool-unix",
			"exiftool");
		return path;
	}
	
	internal const string Https = "https://";
	
	internal const string CheckSumLocation = "exiftool.org/checksums.txt";

	internal const string CheckSumLocationMirror =
		"qdraw.nl/special/mirror/exiftool/checksums.txt";

	internal const string
		ExiftoolDownloadBasePath = "exiftool.org/"; // with slash at the end

	internal const string ExiftoolDownloadBasePathMirror =
		"qdraw.nl/special/mirror/exiftool/"; // with slash at the end
}

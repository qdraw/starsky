using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace starskytest.FakeCreateAn.CreateAnZipFileMacOs;

public class CreateAnZipFileMacOs
{
	[SuppressMessage("ReSharper", "StringLiteralTypo")]
	private const string Base64ZipString = "UEsDBBQAAAAIAElHQ1mzpFMiXAAAANwAAAARACAAX19NQU" +
	                                       "NPU1gvLl9mZm1wZWdVVA0AB4pA/mbuQP5m9EL+ZnV4CwABBPUBAAAEFA" +
	                                       "AAAGNgFWNnYGJg8E1MVvAPVohQgAKQGAMn" +
	                                       "EBsB8SogBvHvMBAFHENCgqBMkI4pQOyBpoQRIc6fnJ+rl1hQkJOql5uY" +
	                                       "nAMUZGMw0K6SVmv3ZdlfW7ZvwafaB8TZiw4" +
	                                       "AUEsDBAoAAAAAANqGiVk2DVKQHAAAABwAAAAGABwAZmZtcGVnVVQJAAP" +
	                                       "LEldn9BJXZ3V4CwABBPUBAAAEFAAAACMhL2" +
	                                       "Jpbi9iYXNoCmVjaG8gRmFrZSBGZm1wZWdQSwECFAMUAAAACABJR0NZs6" +
	                                       "RTIlwAAADcAAAAEQAgAAAAAAAAAAAA7YEAA" +
	                                       "AAAX19NQUNPU1gvLl9mZm1wZWdVVA0AB4pA/mbuQP5m9EL+ZnV4CwABBP" +
	                                       "UBAAAEFAAAAFBLAQIeAwoAAAAAANqGiVk" +
	                                       "2DVKQHAAAABwAAAAGABgAAAAAAAEAAACkgasAAABmZm1wZWdVVAUAA8s" +
	                                       "SV2d1eAsAAQT1AQAABBQAAABQSwUGAAAAAAI" +
	                                       "AAgCrAAAABwEAAAAA";

	public CreateAnZipFileMacOs()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if ( string.IsNullOrEmpty(dirName) )
		{
			return;
		}

		var parentFolder = Path.Combine(dirName, "FakeCreateAn",
			"CreateAnZipFileMacOs");
		var path = Path.Combine(parentFolder, "ArchiveWithDotFiles.zip");
		FilePath = path;

		if ( File.Exists(path) )
		{
			return;
		}

		if ( !Directory.Exists(parentFolder) )
		{
			Directory.CreateDirectory(parentFolder);
		}

		File.WriteAllBytes(path, Convert.FromBase64String(Base64ZipString));
	}

	public string FilePath { get; set; } = string.Empty;

	/// <summary>
	///     Skip the __MACOSX/.ffmpeg file
	/// </summary>
	public static List<string> Content { get; set; } = new() { "ffmpeg", "__MACOSX/.ffmpeg" };
}

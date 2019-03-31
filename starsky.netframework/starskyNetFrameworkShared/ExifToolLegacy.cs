using System;
using System.Diagnostics;
using System.IO;
using starskycore.Models;

namespace starskyNetFrameworkShared
{
	public class ExifToolLegacy
	{
		// Write To Meta data using Exiftool.
		// This is a exiftool wrapper

		private readonly AppSettings _appSettings;

		public ExifToolLegacy(AppSettings appSettings)
		{
			_appSettings = appSettings;
		}
        
		public string Run(string options, string fullFilePathSpaceSeperated)
		{
			options = " " + options + " " + fullFilePathSpaceSeperated;

			Console.WriteLine($"{_appSettings.ExifToolPath}{options}");

			if (!File.Exists(_appSettings.ExifToolPath)) return null;

			var exifToolPath = _appSettings.ExifToolPath;

			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				FileName = exifToolPath,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				Arguments = options
			};


			Process process = Process.Start(processStartInfo);

			string strOutput = process.StandardOutput.ReadToEnd();

			process.WaitForExit();

			if ( !process.HasExited )
			{
				process.CloseMainWindow();
				process.Close();
				return null;
			}
	        
			// make sure that there nothing left
			process.Dispose();

			return strOutput;
		}
	}
}

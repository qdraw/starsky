using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskysynchronizecli;
using starskytest.FakeCreateAn;

namespace starskytest.starskySynchronizeCli
{
	[TestClass]
	public sealed class SynchronizeCliTest
	{
		private static string _prePort;
		private static string _preAspNetUrls;
		private static string _diskWatcherSetting;
		private static string _syncOnStartup;
		private static string _thumbnailGenerationIntervalInMinutes;
		private static string _geoFilesSkipDownloadOnStartup;
		private static string _exiftoolSkipDownloadOnStartup;
		// also see:
		// starsky/starskytest/root/ProgramTest.cs
		// starsky/starskytest/starskyGeoCli/starskyGeoCliTest.cs
		
		public SynchronizeCliTest()
		{
			_prePort = Environment.GetEnvironmentVariable("PORT");
			_preAspNetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
			_diskWatcherSetting =
				Environment.GetEnvironmentVariable("app__useDiskWatcher");
			_syncOnStartup = Environment.GetEnvironmentVariable("app__SyncOnStartup");
			_thumbnailGenerationIntervalInMinutes =
				Environment.GetEnvironmentVariable("app__thumbnailGenerationIntervalInMinutes");
			_geoFilesSkipDownloadOnStartup =
				Environment.GetEnvironmentVariable("app__GeoFilesSkipDownloadOnStartup");
			_exiftoolSkipDownloadOnStartup =
				Environment.GetEnvironmentVariable("app__ExiftoolSkipDownloadOnStartup");
			
			// also see:
			// starsky/starskytest/root/ProgramTest.cs
			// starsky/starskytest/starskyGeoCli/starskyGeoCliTest.cs
		}
		
		[TestMethod]
		[Timeout(5000)]
		public async Task SynchronizeCliHelpVerbose()
		{
			var args = new List<string> {
				"-h","-v"
			}.ToArray();
			await Program.Main(args);
			Assert.IsNotNull(args);
		}
        
		[TestMethod]
		[Timeout(5000)]
		public async Task SynchronizeCliHelpTest()
		{
			var newImage = new CreateAnImage();
			var args = new List<string> {"-h","-v","-c","test","-d", "InMemoryDatabase", 
				"-b", newImage.BasePath, "--thumbnailtempfolder", 
				newImage.BasePath, "-e", newImage.FullFilePath 
			}.ToArray();
			await Program.Main(args);
			Assert.IsNotNull(args);
		}
        
		
		[ClassCleanup]
		public static void CleanEnvsAfterwards()
		{
			Environment.SetEnvironmentVariable("PORT",_prePort);
			Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
			Environment.SetEnvironmentVariable("app__useDiskWatcher",_diskWatcherSetting);
			Environment.SetEnvironmentVariable("app__SyncOnStartup",_syncOnStartup);
			Environment.SetEnvironmentVariable("app__thumbnailGenerationIntervalInMinutes",_thumbnailGenerationIntervalInMinutes);
			Environment.SetEnvironmentVariable("app__GeoFilesSkipDownloadOnStartup",_geoFilesSkipDownloadOnStartup);
			Environment.SetEnvironmentVariable("app__ExiftoolSkipDownloadOnStartup",_exiftoolSkipDownloadOnStartup);
		}
        
	}
}

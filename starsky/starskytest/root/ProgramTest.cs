using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;

namespace starskytest.root;

[TestClass]
public class ProgramTest
{
	private static string _prePort;
	private static string _preAspNetUrls;
	private static string _diskWatcherSetting;
	private static string _syncOnStartup;
	private static string _thumbnailGenerationIntervalInMinutes;
	private static string _geoFilesSkipDownloadOnStartup;
	private static string _exiftoolSkipDownloadOnStartup;

	public ProgramTest()
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
		
		// see also:
		// starsky/starskytest/starskyGeoCli/starskyGeoCliTest.cs
	}

	[TestMethod]
	[Timeout(5000)]
	[ExpectedException(typeof(System.IO.IOException))]
	public async Task TestMethod1()
	{
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","http://*:9514");
		Environment.SetEnvironmentVariable("app__useDiskWatcher","false");
		Environment.SetEnvironmentVariable("app__SyncOnStartup","false");
		Environment.SetEnvironmentVariable("app__thumbnailGenerationIntervalInMinutes","0");
		Environment.SetEnvironmentVariable("app__GeoFilesSkipDownloadOnStartup","true");
		Environment.SetEnvironmentVariable("app__ExiftoolSkipDownloadOnStartup","true");

		var builder = WebApplication.CreateBuilder(Array.Empty<string>());
		var app = builder.Build();
		
		await Task.Factory.StartNew(() => app.RunAsync(), TaskCreationOptions.LongRunning);

		await Program.Main(Array.Empty<string>());
	}
	
	[ClassCleanup]
	public static void CleanEnvsAfterwards()
	{
		// see also:
		// starsky/starskytest/starskyGeoCli/starskyGeoCliTest.cs
		
		Environment.SetEnvironmentVariable("PORT",_prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS",_preAspNetUrls);
		Environment.SetEnvironmentVariable("app__useDiskWatcher",_diskWatcherSetting);
		Environment.SetEnvironmentVariable("app__SyncOnStartup",_syncOnStartup);
		Environment.SetEnvironmentVariable("app__thumbnailGenerationIntervalInMinutes",_thumbnailGenerationIntervalInMinutes);
		Environment.SetEnvironmentVariable("app__GeoFilesSkipDownloadOnStartup",_geoFilesSkipDownloadOnStartup);
		Environment.SetEnvironmentVariable("app__ExiftoolSkipDownloadOnStartup",_exiftoolSkipDownloadOnStartup);
	}
	
}
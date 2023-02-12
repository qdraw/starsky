using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Models;

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
	private static string _enablePackageTelemetry;

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
		_enablePackageTelemetry =
			Environment.GetEnvironmentVariable("app__EnablePackageTelemetry");
		
		// see also:
		// starsky/starskytest/starskyGeoCli/starskyGeoCliTest.cs
	}
	
	[TestMethod]
	[Timeout(5000)]
	[ExpectedException(typeof(HttpRequestException))]
	public async Task Program_Main_NoAddress_UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			// this test has issues with timing on windows
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}
		
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","http://*:7514");
		Environment.SetEnvironmentVariable("app__useDiskWatcher","false");
		Environment.SetEnvironmentVariable("app__SyncOnStartup","false");
		Environment.SetEnvironmentVariable("app__thumbnailGenerationIntervalInMinutes","0");
		Environment.SetEnvironmentVariable("app__GeoFilesSkipDownloadOnStartup","true");
		Environment.SetEnvironmentVariable("app__ExiftoolSkipDownloadOnStartup","true");
		Environment.SetEnvironmentVariable("app__EnablePackageTelemetry","false");
		
		await Program.Main(new []{"--do-not-start"});

		using HttpClient client = new();
		await client.GetAsync("http://localhost:7514");
		// and this address does not exists
	}
	
	[TestMethod]
	[Timeout(5000)]
	public async Task Program_RunAsync()
	{
		var result = await Program.RunAsync(null,false);
		Assert.IsFalse(result);
	}
	
	[TestMethod]
	[Timeout(5000)]
	public async Task Program_RunAsync_ReturnedTrue()
	{
		var builder = WebApplication.CreateBuilder(Array.Empty<string>());
		var app = builder.Build();
		
		var result = await Task.Factory.StartNew(async () => 
			await Program.RunAsync(app).TimeoutAfter(100), 
			TaskCreationOptions.LongRunning);
		
		await app.StopAsync();
		
		Assert.IsNotNull(result);
	}
	
	[TestMethod]
	[Timeout(5000)]
	[ExpectedException(typeof(FormatException))]
	public async Task Program_RunAsync_InvalidUrl()
	{
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS","test");

		var builder = WebApplication.CreateBuilder(Array.Empty<string>());
		var app = builder.Build();
		
		await Program.RunAsync(app);
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

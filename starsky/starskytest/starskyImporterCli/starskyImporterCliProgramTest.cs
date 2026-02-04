using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskyimportercli;

namespace starskytest.starskyImporterCli;

[TestClass]
public sealed class StarskyImporterCliProgramTest
{
	private static string? _prePort;
	private static string? _preAspNetUrls;
	private static string? _diskWatcherSetting;
	private static string? _syncOnStartup;
	private static string? _thumbnailGenerationIntervalInMinutes;
	private static string? _geoFilesSkipDownloadOnStartup;
	private static string? _ffmpegSkipDownloadOnStartup;

	private static string? _exiftoolSkipDownloadOnStartup;
	// also see:
	// starsky/starskytest/root/ProgramTest.cs
	// starskytest/starskythumbnailcli/ProgramTest.cs
	// starsky/starskytest/starskySynchronizeCli/ProgramTest.cs

	public StarskyImporterCliProgramTest()
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
		_ffmpegSkipDownloadOnStartup =
			Environment.GetEnvironmentVariable("app__ffmpegSkipDownloadOnStartup");

		// also see:
		// starsky/starskytest/root/ProgramTest.cs
		// starskytest/starskythumbnailcli/ProgramTest.cs
		// starsky/starskytest/starskySynchronizeCli/ProgramTest.cs

		Environment.SetEnvironmentVariable("app__GeoFilesSkipDownloadOnStartup", "true");
		Environment.SetEnvironmentVariable("app__ExiftoolSkipDownloadOnStartup", "true");
		Environment.SetEnvironmentVariable("app__EnablePackageTelemetry", "false");
		Environment.SetEnvironmentVariable("app__FfmpegSkipDownloadOnStartup", "true");
	}


	[TestMethod]
	public async Task StarskyCliHelpVerbose()
	{
		var args = new List<string> { "-h", "-v" }.ToArray();
		await Program.Main(args);
		// should not crash
		Assert.IsNotNull(args);
	}

	[TestMethod]
	public async Task StarskyProvider()
	{
		var args = new List<string> { "--provider" }.ToArray();
		await Program.Main(args);
		Assert.IsNotNull(args);
	}

	[TestMethod]
	public async Task StarskyProviderTest()
	{
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__Id", "test");
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__Enabled", "false");
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__Provider", "Dropbox");
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__RemoteFolder",
			"/Camera Uploads");
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__DeleteAfterImport",
			"false");
		Environment.SetEnvironmentVariable(
			"app__CloudImport__Providers__0__Credentials__RefreshToken", "");
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__Credentials__AppKey",
			"24353");
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__Credentials__AppSecret",
			"243");

		var args = new List<string> { "--provider", "test" }.ToArray();
		await Program.Main(args);
		Assert.IsNotNull(args);

		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__Id", null);
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__Enabled", null);
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__Provider", null);
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__RemoteFolder",
			null);
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__DeleteAfterImport",
			null);
		Environment.SetEnvironmentVariable(
			"app__CloudImport__Providers__0__Credentials__RefreshToken", null);
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__Credentials__AppKey",
			null);
		Environment.SetEnvironmentVariable("app__CloudImport__Providers__0__Credentials__AppSecret",
			null);
	}

	[ClassCleanup(ClassCleanupBehavior.EndOfClass)]
	public static void CleanEnvsAfterwards()
	{
		Environment.SetEnvironmentVariable("PORT", _prePort);
		Environment.SetEnvironmentVariable("ASPNETCORE_URLS", _preAspNetUrls);
		Environment.SetEnvironmentVariable("app__useDiskWatcher", _diskWatcherSetting);
		Environment.SetEnvironmentVariable("app__SyncOnStartup", _syncOnStartup);
		Environment.SetEnvironmentVariable("app__thumbnailGenerationIntervalInMinutes",
			_thumbnailGenerationIntervalInMinutes);
		Environment.SetEnvironmentVariable("app__GeoFilesSkipDownloadOnStartup",
			_geoFilesSkipDownloadOnStartup);
		Environment.SetEnvironmentVariable("app__ExiftoolSkipDownloadOnStartup",
			_exiftoolSkipDownloadOnStartup);
		Environment.SetEnvironmentVariable("app__FfmpegSkipDownloadOnStartup",
			_ffmpegSkipDownloadOnStartup);
	}
}

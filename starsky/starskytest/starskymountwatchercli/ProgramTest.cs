using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskymountwatchercli;
using starskytest.FakeCreateAn;

namespace starskytest.starskymountwatchercli;

[TestClass]
public sealed class ProgramTest
{
	// Snapshot existing env-vars so they can be restored after the class runs.
	// Pattern mirrors:
	//   starskytest/starskythumbnailcli/ProgramTest.cs
	//   starskytest/starskySynchronizeCli/ProgramTest.cs
	//   starskytest/starskyImporterCli/starskyImporterCliProgramTest.cs
	private static string? _prePort;
	private static string? _preAspNetUrls;
	private static string? _diskWatcherSetting;
	private static string? _syncOnStartup;
	private static string? _thumbnailGenerationIntervalInMinutes;
	private static string? _geoFilesSkipDownloadOnStartup;
	private static string? _exiftoolSkipDownloadOnStartup;
	private static string? _ffmpegSkipDownloadOnStartup;

	public ProgramTest()
	{
		_prePort = Environment.GetEnvironmentVariable("PORT");
		_preAspNetUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
		_diskWatcherSetting = Environment.GetEnvironmentVariable("app__useDiskWatcher");
		_syncOnStartup = Environment.GetEnvironmentVariable("app__SyncOnStartup");
		_thumbnailGenerationIntervalInMinutes =
			Environment.GetEnvironmentVariable("app__thumbnailGenerationIntervalInMinutes");
		_geoFilesSkipDownloadOnStartup =
			Environment.GetEnvironmentVariable("app__GeoFilesSkipDownloadOnStartup");
		_exiftoolSkipDownloadOnStartup =
			Environment.GetEnvironmentVariable("app__ExiftoolSkipDownloadOnStartup");
		_ffmpegSkipDownloadOnStartup =
			Environment.GetEnvironmentVariable("app__ffmpegSkipDownloadOnStartup");

		Environment.SetEnvironmentVariable("app__GeoFilesSkipDownloadOnStartup", "true");
		Environment.SetEnvironmentVariable("app__ExiftoolSkipDownloadOnStartup", "true");
		Environment.SetEnvironmentVariable("app__EnablePackageTelemetry", "false");
		Environment.SetEnvironmentVariable("app__FfmpegSkipDownloadOnStartup", "true");
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task Main_HelpFlag_CompletesWithoutHangingHost()
	{
		// -h causes StartWatcher to return early and skips host.RunAsync
		var args = new List<string> { "-h" }.ToArray();
		await Program.Main(args);
		Assert.IsNotNull(args);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task Main_HelpAndVerboseFlags_CompletesWithoutHangingHost()
	{
		var args = new List<string> { "-h", "-v" }.ToArray();
		await Program.Main(args);
		Assert.IsNotNull(args);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task Main_HelpWithInMemoryDatabase_CompletesWithoutHangingHost()
	{
		var image = new CreateAnImage();
		var args = new List<string>
		{
			"-h",
			"-v",
			"-d",
			"InMemoryDatabase",
			"-b",
			image.BasePath,
			"--thumbnailtempfolder",
			image.BasePath
		}.ToArray();

		await Program.Main(args);
		Assert.IsNotNull(args);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task Main_InstallFlag_CompletesWithoutHangingHost()
	{
		// --install triggers service installation but NOT host.RunAsync(),
		// so the call must return regardless of whether the install itself
		// succeeds or fails on the current OS / CI environment.
		var args = new List<string> { "--install" }.ToArray();
		await Program.Main(args);
		Assert.IsNotNull(args);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task Main_UninstallFlag_CompletesWithoutHangingHost()
	{
		// --uninstall removes the service (or silently succeeds when it
		// is not installed) and must NOT run host.RunAsync().
		var args = new List<string> { "--uninstall" }.ToArray();
		await Program.Main(args);
		Assert.IsNotNull(args);
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	public async Task Main_StatusFlag_CompletesWithoutHangingHost()
	{
		// --status gives a status and must NOT run host.RunAsync().
		var args = new List<string> { "--status" }.ToArray();
		await Program.Main(args);
		Assert.IsNotNull(args);
	}

	[ClassCleanup]
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

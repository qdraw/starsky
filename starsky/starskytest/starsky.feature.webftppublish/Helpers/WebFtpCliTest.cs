using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webftppublish.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webftppublish.Helpers;

[TestClass]
public sealed class WebFtpCliTest
{
	private readonly AppSettings _appSettings;
	private readonly FakeIFtpWebRequestFactory _webRequestFactory;

	public WebFtpCliTest()
	{
		_appSettings = new AppSettings { WebFtp = "ftp://test:test@testmedia.be" };
		_webRequestFactory = new FakeIFtpWebRequestFactory();
	}

	private static byte[]? ExampleManifest()
	{
		var input = "{\n  \"Name\": \"Test\",\n  " +
		            "\"Copy\": {\n    \"1000/0_kl1k.jpg\": " +
		            "true,\n    \"_settings.json\": false\n  },\n" +
		            "  \"Slug\": \"test\",\n  \"Export\": \"20200808121411\",\n" +
		            "  \"Version\": \"0.3.0.0\"\n}";
		var stream = StringToStreamHelper.StringToStream(input) as MemoryStream;
		return stream?.ToArray();
	}

	private static string CreateZipWithSettings(string tempRoot)
	{
		var zipPath = Path.Combine(tempRoot, "webftp-input.zip");
		using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
		var settingsEntry = zip.CreateEntry("_settings.json");
		using ( var settingsStream = settingsEntry.Open() )
		using ( var writer = new StreamWriter(settingsStream) )
		{
			writer.Write(System.Text.Encoding.UTF8.GetString(ExampleManifest()!));
		}

		var imageEntry = zip.CreateEntry("1000/0_kl1k.jpg");
		using ( var imageStream = imageEntry.Open() )
		using ( var writer = new StreamWriter(imageStream) )
		{
			writer.Write("test");
		}

		return zipPath;
	}

	[TestMethod]
	public async Task Run_Help()
	{
		var console = new FakeConsoleWrapper();
		await new WebFtpCli(_appSettings, new FakeSelectorStorage(), console, _webRequestFactory,
				new FakeIWebLogger())
			.RunAsync(["-h"]);

		Assert.IsTrue(console.WrittenLines.FirstOrDefault()
			?.Contains("Starsky WebFtp Cli ~ Help:"));
		Assert.IsTrue(console.WrittenLines.LastOrDefault()
			?.Contains("  use -v -help to show settings: "));
	}

	[TestMethod]
	public async Task Run_Default()
	{
		var console = new FakeConsoleWrapper();
		var logger = new FakeIWebLogger();
		await new WebFtpCli(_appSettings, new FakeSelectorStorage(), console, _webRequestFactory,
				logger)
			.RunAsync([""]);

		Assert.IsTrue(logger.TrackedExceptions.FirstOrDefault()
			.Item2?.Contains("Please use the -p to add a path first"));
	}

	[TestMethod]
	public async Task Run_PathArg()
	{
		var console = new FakeConsoleWrapper();
		var logger = new FakeIWebLogger();
		await new WebFtpCli(_appSettings, new FakeSelectorStorage(), console, _webRequestFactory,
				logger)
			.RunAsync(["-p"]);

		Assert.IsTrue(logger.TrackedExceptions.FirstOrDefault()
			.Item2?.Contains("is not found"));
	}

	[TestMethod]
	public async Task Run_NoFtpSettings()
	{
		var console = new FakeConsoleWrapper();
		var logger = new FakeIWebLogger();
		var fakeSelectorStorage =
			new FakeSelectorStorage(new FakeIStorage(["/test"]));

		// no ftp settings
		await new WebFtpCli(new AppSettings(), fakeSelectorStorage, console, _webRequestFactory,
				logger)
			.RunAsync(["-p", "/test"]);

		Assert.IsTrue(logger.TrackedExceptions.FirstOrDefault()
			.Item2?.Contains("WebFtp settings"));
	}

	[TestMethod]
	public async Task Run_NoSettingsFileInFolder()
	{
		var console = new FakeConsoleWrapper();
		var logger = new FakeIWebLogger();

		var fakeSelectorStorage =
			new FakeSelectorStorage(new FakeIStorage(["/test"]));

		await new WebFtpCli(_appSettings, fakeSelectorStorage, console, _webRequestFactory,
				logger)
			.RunAsync(["-p", "/test"]);
		
		Assert.IsTrue(logger.TrackedExceptions.FirstOrDefault()
			.Item2?.Contains("generate a settings file"));
	}

	[TestMethod]
	public async Task Run_SettingsFile_successful()
	{
		var console = new FakeConsoleWrapper();

		var fakeSelectorStorage = new FakeSelectorStorage(new FakeIStorage(
			["/test"],
			[$"/test{Path.DirectorySeparatorChar}_settings.json", "/test/1000/0_kl1k.jpg"], new List<byte[]?> { ExampleManifest(), Array.Empty<byte>() }));
		// instead of new byte[0]

		await new WebFtpCli(_appSettings, fakeSelectorStorage, console, _webRequestFactory,
				new FakeIWebLogger())
			.RunAsync(["-p", "/test"]);

		var isSuccess = console.WrittenLines.LastOrDefault()?
			.Contains("Ftp copy successful done");

		switch ( isSuccess )
		{
			// To Debug why the test has failed
			case false:
			{
				foreach ( var line in console.WrittenLines! )
				{
					Console.WriteLine(line);
				}

				break;
			}
			case null:
				Assert.IsNotNull(isSuccess);
				break;
		}

		Assert.IsTrue(isSuccess);
	}

	[TestMethod]
	public async Task Run_ZipSettingsFile_successful()
	{
		var console = new FakeConsoleWrapper();
		var logger = new FakeIWebLogger();
		var hostStorage = new StorageHostFullPathFilesystem(logger);
		var fakeSelectorStorage = new FakeSelectorStorage(hostStorage);

		var tempRoot = Path.Combine(Path.GetTempPath(), "starsky-webftp-test",
			Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(tempRoot);

		try
		{
			var zipPath = CreateZipWithSettings(tempRoot);

			await new WebFtpCli(_appSettings, fakeSelectorStorage, console, _webRequestFactory,
					new FakeIWebLogger())
				.RunAsync(["-p", zipPath]);

			var isSuccess = console.WrittenLines.LastOrDefault()?
				.Contains("Ftp copy successful done");

			Assert.IsTrue(isSuccess);
		}
		finally
		{
			if ( Directory.Exists(tempRoot) )
			{
				Directory.Delete(tempRoot, true);
			}
		}
	}
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.writemeta.Helpers;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateFakeExifToolWindows;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.writemeta.Helpers;

[TestClass]
public class ExifToolStreamToStreamRunnerTests
{
	private readonly AppSettings _appSettingsWithExifTool;
	private readonly string _exifToolExePosix;
	private readonly StorageHostFullPathFilesystem _hostFullPathFilesystem;

	public ExifToolStreamToStreamRunnerTests()
	{
		_hostFullPathFilesystem = new StorageHostFullPathFilesystem(new FakeIWebLogger());


		var exifToolExeWindows = new CreateFakeExifToolWindows().ExifToolPath;

		_hostFullPathFilesystem.CreateDirectory(Path.Combine(new CreateAnImage().BasePath,
			"ExifToolStreamToStreamRunnerTests"));
		_exifToolExePosix = Path.Combine(new CreateAnImage().BasePath,
			"ExifToolStreamToStreamRunnerTests", "exiftool");

		var exifToolExe = new AppSettings().IsWindows ? exifToolExeWindows : _exifToolExePosix;
		_appSettingsWithExifTool = new AppSettings { ExifToolPath = exifToolExe };
	}

	[ClassCleanup(ClassCleanupBehavior.EndOfClass)]
	public static void CleanUp()
	{
		var folder = Path.Combine(new CreateAnImage().BasePath,
			"ExifToolStreamToStreamRunnerTests");
		if ( Directory.Exists(folder) )
		{
			Directory.Delete(folder, true);
		}
	}

	private async Task CreateStubFile(string path, string content)
	{
		var stream = StringToStreamHelper.StringToStream(content);
		await _hostFullPathFilesystem.WriteStreamAsync(stream, path);
	}

	private async Task SetupFakeExifToolExecutable()
	{
		await CreateStubFile(_exifToolExePosix,
			"#!/bin/bash\necho Fake Executable");

		await new FfMpegChmod(new FakeSelectorStorage(_hostFullPathFilesystem),
				new FakeIWebLogger())
			.Chmod(
				_exifToolExePosix);
	}

	[TestMethod]
	public async Task StreamToStreamRunner_ArgumentNullException()
	{
		await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
			await new ExifToolStreamToStreamRunner(new AppSettings(),
				new FakeIWebLogger()).RunProcessAsync(null!, "test / unit test"));
	}

	[TestMethod]
	public async Task RunProcessAsync_RunChildObject_UnixOnly()
	{
		// Unix only
		var appSettings = new AppSettings { Verbose = true, ExifToolPath = "/bin/ls" };
		if ( appSettings.IsWindows || !File.Exists("/bin/ls") )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		var runner = new ExifToolStreamToStreamRunner(appSettings, new FakeIWebLogger());
		var streamResult = await runner.RunProcessAsync(new MemoryStream([]),
			string.Empty, "test / unit test");

		await StreamToStringHelper.StreamToStringAsync(streamResult, false);

		Assert.AreEqual(0, streamResult.Length);

		await streamResult.DisposeAsync();
	}

	[TestMethod]
	[DataRow("file.txt && dir")]
	[DataRow("file.txt | ipconfig")]
	[DataRow("file.txt && ipconfig")]
	[DataRow("file.txt & powershell -Command \"Get-Process | Out-File output.txt\"")]
	[DataRow("file.txt && curl https://qdraw.nl")]
	[DataRow("\"file.txt\" && ipconfig")]
	public async Task ExifTool_RunProcessAsync_Fuzzing(string argument)
	{
		var appSettings = new AppSettings { Verbose = true, ExifToolPath = "/bin/sh" };
		if ( appSettings.IsWindows || !File.Exists("/bin/sh") )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		var runner = new ExifToolStreamToStreamRunner(appSettings,
			new FakeIWebLogger());
		var streamResult = await runner.RunProcessAsync(new MemoryStream([]),
			argument, "test / unit test");

		var stringResult = await StreamToStringHelper.StreamToStringAsync(streamResult);

		Assert.AreEqual(0, stringResult.Length);
		Assert.AreEqual(string.Empty, stringResult);
	}

	[TestMethod]
	public async Task ExifTool_RunProcessAsync_HappyFlow()
	{
		await SetupFakeExifToolExecutable();

		var sourceStream = new MemoryStream([0x01, 0x02, 0x03]);

		var sut = new ExifToolStreamToStreamRunner(_appSettingsWithExifTool,
			new FakeIWebLogger());

		var stream = await sut.RunProcessAsync(sourceStream, "arg1",
			"reference");

		await sourceStream.DisposeAsync();

		Assert.IsNotNull(stream);

		await stream.DisposeAsync();
	}

	[TestMethod]
	public async Task ExifTool_RunProcessAsync_ExitCode()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test is only applicable on Unix-based systems.");
			return;
		}

		await SetupFakeExifToolExecutable();
		var sourceStream = new MemoryStream([0x01, 0x02, 0x03]);

		// overwrite the file with a bash script that will exit with code 1
		await CreateStubFile(_exifToolExePosix,
			"#!/bin/bash\ntest_content\n exit 1");

		var sut = new ExifToolStreamToStreamRunner(_appSettingsWithExifTool,
			new FakeIWebLogger());

		var stream = await sut.RunProcessAsync(sourceStream, "arg1",
			"reference");

		Assert.IsNotNull(stream);

		await sourceStream.DisposeAsync();
		await stream.DisposeAsync();
	}

	[TestMethod]
	public async Task ExifTool_RunProcessAsync_WithInvalidFfmpegPath()
	{
		await SetupFakeExifToolExecutable();
		var sourceStream = new MemoryStream([0x01, 0x02, 0x03]);

		var sut = new ExifToolStreamToStreamRunner(new AppSettings { ExifToolPath = "invalid" },
			new FakeIWebLogger());

		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await sut.RunProcessAsync(sourceStream, "-1",
				"image2"));

		await sourceStream.DisposeAsync();
	}

	[TestMethod]
	public async Task ExifTool_RunProcessAsync_WithInvalidPath_WithInvalidStream()
	{
		var sut = new ExifToolStreamToStreamRunner(new AppSettings { ExifToolPath = "invalid" },
			new FakeIWebLogger());

		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await sut.RunProcessAsync(Stream.Null,
				"-1", "image2"));
	}
}

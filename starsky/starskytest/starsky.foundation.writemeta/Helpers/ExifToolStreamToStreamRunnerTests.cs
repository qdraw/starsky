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
	private readonly string _exifToolExe;
	private readonly string _exifToolExePosix;
	private readonly string _exifToolExeWindows;
	private readonly StorageHostFullPathFilesystem _hostFullPathFilesystem;

	public ExifToolStreamToStreamRunnerTests()
	{
		_hostFullPathFilesystem = new StorageHostFullPathFilesystem(new FakeIWebLogger());


		_exifToolExeWindows = new CreateFakeExifToolWindows().ExifToolPath;

		_hostFullPathFilesystem.CreateDirectory(Path.Combine(new CreateAnImage().BasePath,
			"ExifToolStreamToStreamRunnerTests"));
		_exifToolExePosix = Path.Combine(new CreateAnImage().BasePath,
			"ExifToolStreamToStreamRunnerTests", "exiftool");

		_exifToolExe = new AppSettings().IsWindows ? _exifToolExeWindows : _exifToolExePosix;
		_appSettingsWithExifTool = new AppSettings { ExifToolPath = _exifToolExe };
	}

	[ClassCleanup]
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

	private async Task<string> SetupFakeExifToolExecutable(int i)
	{
		var readFile = Path.Combine(new CreateAnImage().BasePath,
			"FfmpegStreamToStreamRunnerTests", "read_file.");

		await CreateStubFile(_exifToolExePosix,
			"#!/bin/bash\necho Fake Executable");
		await CreateStubFile(readFile + i, "test_content");

		await new FfMpegChmod(new FakeSelectorStorage(_hostFullPathFilesystem),
				new FakeIWebLogger())
			.Chmod(
				_exifToolExePosix);
		return readFile + i;
	}

	[TestMethod]
	public void StreamToStreamRunner_ArgumentNullException()
	{
		Assert.ThrowsException<ArgumentNullException>(() =>
			new ExifToolStreamToStreamRunner(new AppSettings(), null!,
				new FakeIWebLogger()));
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

		var runner = new ExifToolStreamToStreamRunner(appSettings,
			new MemoryStream([]), new FakeIWebLogger());
		var streamResult = await runner.RunProcessAsync(
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
	public async Task RunProcessAsync_Fuzzing(string argument)
	{
		var appSettings = new AppSettings { Verbose = true, ExifToolPath = "/bin/sh" };
		if ( appSettings.IsWindows || !File.Exists("/bin/sh") )
		{
			Assert.Inconclusive("This test if for Unix Only");
			return;
		}

		var runner = new ExifToolStreamToStreamRunner(appSettings,
			new MemoryStream([]), new FakeIWebLogger());
		var streamResult = await runner.RunProcessAsync(
			argument, "test / unit test");

		var stringResult = await StreamToStringHelper.StreamToStringAsync(streamResult);

		Assert.AreEqual(0, stringResult.Length);
		Assert.AreEqual(string.Empty, stringResult);
	}

	[TestMethod]
	public async Task RunProcessAsync_HappyFlow()
	{
		var readFile = await SetupFakeExifToolExecutable(0);

		var hostStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		var sourceStream = hostStorage.ReadStream(readFile);
		var sut = new ExifToolStreamToStreamRunner(_appSettingsWithExifTool, sourceStream,
			new FakeIWebLogger());

		var stream = await sut.RunProcessAsync("arg1",
			"reference");

		await sourceStream.DisposeAsync();

		Assert.IsNotNull(stream);

		await stream.DisposeAsync();
	}

	[TestMethod]
	public async Task RunProcessAsync_ExitCode()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test is only applicable on Unix-based systems.");
			return;
		}

		var readFile = await SetupFakeExifToolExecutable(1);

		// overwrite the file with a bash script that will exit with code 1
		await CreateStubFile(_exifToolExePosix,
			"#!/bin/bash\ntest_content\n exit 1");

		var file = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(readFile);
		var sut = new ExifToolStreamToStreamRunner(_appSettingsWithExifTool, file,
			new FakeIWebLogger());

		var stream = await sut.RunProcessAsync("arg1",
			"reference");

		Assert.IsNotNull(stream);

		await file.DisposeAsync();
		await stream.DisposeAsync();
	}

	[TestMethod]
	public async Task RunProcessAsync_WithInvalidFfmpegPath()
	{
		var readFile = await SetupFakeExifToolExecutable(2);

		var file = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(readFile);
		var sut = new ExifToolStreamToStreamRunner(new AppSettings { ExifToolPath = "invalid" },
			file,
			new FakeIWebLogger());

		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await sut.RunProcessAsync("-1",
				"image2"));

		await file.DisposeAsync();
	}

	[TestMethod]
	public async Task RunProcessAsync_WithInvalidFfmpegPath_WithInvalidStream()
	{
		var sut = new ExifToolStreamToStreamRunner(new AppSettings { ExifToolPath = "invalid" },
			Stream.Null,
			new FakeIWebLogger());

		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await sut.RunProcessAsync("-1", "image2"));
	}
}

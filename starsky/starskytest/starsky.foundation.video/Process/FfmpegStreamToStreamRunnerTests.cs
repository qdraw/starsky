using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.video.Process;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnZipfileFakeFFMpeg;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.Process;

[TestClass]
public class FfmpegStreamToStreamRunnerTests
{
	private readonly string _ffmpegExe;
	private readonly string _ffmpegExePosix;
	private readonly string _ffmpegExeWindows;
	private readonly StorageHostFullPathFilesystem _hostFullPathFilesystem;
	private readonly string _readFile;

	public FfmpegStreamToStreamRunnerTests()
	{
		_hostFullPathFilesystem = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		_hostFullPathFilesystem.CreateDirectory(Path.Combine(new CreateAnImage().BasePath,
			"FfmpegStreamToStreamRunnerTests"));

		_ffmpegExeWindows = Path.Combine(new CreateAnImage().BasePath,
			"FfmpegStreamToStreamRunnerTests", "ffmpeg.exe");
		_ffmpegExePosix = Path.Combine(new CreateAnImage().BasePath,
			"FfmpegStreamToStreamRunnerTests", "ffmpeg");

		_ffmpegExe = new AppSettings().IsWindows ? _ffmpegExeWindows : _ffmpegExePosix;

		_readFile = Path.Combine(new CreateAnImage().BasePath,
			"FfmpegStreamToStreamRunnerTests", "read_file");
	}

	private void CreateStubFile(string path, string content)
	{
		var stream = StringToStreamHelper.StringToStream(content);
		_hostFullPathFilesystem.WriteStream(stream, path);
	}

	private async Task SetupFakeFfmpegExecutable()
	{
		var zipper = Zipper.ExtractZip([
			..
			new CreateAnZipfileFakeFfMpeg().Bytes
		]);

		CreateStubFile(_ffmpegExePosix,
			"#!/bin/bash\necho Fake Executable");
		CreateStubFile(_readFile, "test_content");

		var ffmpegExe = new MemoryStream(zipper.FirstOrDefault(p =>
			p.Key == "ffmpeg.exe").Value);
		await _hostFullPathFilesystem.WriteStreamAsync(ffmpegExe,
			_ffmpegExeWindows);

		await new FfMpegChmod(new FakeSelectorStorage(_hostFullPathFilesystem),
				new FakeIWebLogger())
			.Chmod(
				_ffmpegExePosix);
	}

	[ClassCleanup]
	public static void CleanUp()
	{
		File.Delete(Path.Combine(new CreateAnImage().BasePath, "FfmpegStreamToStreamRunnerTests",
			"ffmpeg"));
		File.Delete(Path.Combine(new CreateAnImage().BasePath, "FfmpegStreamToStreamRunnerTests",
			"ffmpeg.exe"));
		File.Delete(Path.Combine(new CreateAnImage().BasePath, "FfmpegStreamToStreamRunnerTests",
			"read_file"));
	}

	[TestMethod]
	public async Task RunProcessAsync_HappyFlow()
	{
		await SetupFakeFfmpegExecutable();
		var file = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(_readFile);
		var sut = new FfmpegStreamToStreamRunner(_ffmpegExe, file, new FakeIWebLogger());

		var (stream, result) = await sut.RunProcessAsync("-1",
			"image2", "test");

		Assert.IsNotNull(stream);
		Assert.IsTrue(result);

		await file.DisposeAsync();
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

		await SetupFakeFfmpegExecutable();

		// overwrite the file with a bash script that will exit with code 1
		CreateStubFile(_ffmpegExePosix,
			"#!/bin/bash\ntest_content\n exit 1");

		var file = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(_readFile);
		var sut = new FfmpegStreamToStreamRunner(_ffmpegExe, file, new FakeIWebLogger());

		var (stream, result) = await sut.RunProcessAsync("-1",
			"image2", "test");

		Assert.IsNotNull(stream);
		Assert.IsFalse(result);

		await file.DisposeAsync();
		await stream.DisposeAsync();
	}

	[TestMethod]
	public async Task RunProcessAsync_WithInvalidFfmpegPath()
	{
		await SetupFakeFfmpegExecutable();

		var file = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(_readFile);
		var sut = new FfmpegStreamToStreamRunner("invalid", file,
			new FakeIWebLogger());

		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await sut.RunProcessAsync("-1",
				"image2", "test"));

		await file.DisposeAsync();
	}

	[TestMethod]
	public async Task RunProcessAsync_WithInvalidFfmpegPath_WithInvalidStream()
	{
		var sut = new FfmpegStreamToStreamRunner("invalid", Stream.Null,
			new FakeIWebLogger());

		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await sut.RunProcessAsync("-1", "image2",
				"test"));
	}

	[TestMethod]
	public void FfmpegStreamToStreamRunner_Null()
	{
		Assert.ThrowsException<ArgumentNullException>(() =>
			new FfmpegStreamToStreamRunner(null!, null!,
				new FakeIWebLogger()));
	}
}

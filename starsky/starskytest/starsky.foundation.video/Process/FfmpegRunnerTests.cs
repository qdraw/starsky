using System;
using System.Diagnostics.CodeAnalysis;
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
	}

	[SuppressMessage("Usage", "S2325: Make 'CreateStubFile' a static method.",
		Justification = "Uses _hostFullPathFilesystem")]
	private async Task CreateStubFile(string path, string content)
	{
		var stream = StringToStreamHelper.StringToStream(content);
		await _hostFullPathFilesystem.WriteStreamAsync(stream, path);
	}

	[SuppressMessage("Usage", "S2325: Make 'SetupFakeFfmpegExecutable' a static method.",
		Justification = "Uses _hostFullPathFilesystem")]
	private async Task<string> SetupFakeFfmpegExecutable(int i)
	{
		var readFile = Path.Combine(new CreateAnImage().BasePath,
			"FfmpegStreamToStreamRunnerTests", "read_file.");

		var zipper = Zipper.ExtractZip([
			..
			new CreateAnZipfileFakeFfMpeg().Bytes
		]);

		await CreateStubFile(_ffmpegExePosix,
			"#!/bin/bash\necho Fake Executable");
		await CreateStubFile(readFile + i, "test_content");

		var ffmpegExe = new MemoryStream(zipper.FirstOrDefault(p =>
			p.Key == "ffmpeg.exe").Value);
		await _hostFullPathFilesystem.WriteStreamAsync(ffmpegExe,
			_ffmpegExeWindows);

		await new FfMpegChmod(new FakeSelectorStorage(_hostFullPathFilesystem),
				new FakeIWebLogger())
			.Chmod(
				_ffmpegExePosix);
		return readFile + i;
	}

	[ClassCleanup]
	public static void Ffmpeg_CleanUp()
	{
		File.Delete(Path.Combine(new CreateAnImage().BasePath, "FfmpegStreamToStreamRunnerTests",
			"ffmpeg"));
		File.Delete(Path.Combine(new CreateAnImage().BasePath, "FfmpegStreamToStreamRunnerTests",
			"ffmpeg.exe"));
		File.Delete(Path.Combine(new CreateAnImage().BasePath, "FfmpegStreamToStreamRunnerTests",
			"read_file.0"));
		File.Delete(Path.Combine(new CreateAnImage().BasePath, "FfmpegStreamToStreamRunnerTests",
			"read_file.1"));
		File.Delete(Path.Combine(new CreateAnImage().BasePath, "FfmpegStreamToStreamRunnerTests",
			"read_file.2"));
		Directory.Delete(Path.Combine(new CreateAnImage().BasePath,
			"FfmpegStreamToStreamRunnerTests"));
	}

	[TestMethod]
	public async Task Ffmpeg_RunProcessAsync_HappyFlow()
	{
		await SetupFakeFfmpegExecutable(0);
		var sourceStream = new MemoryStream([0x01, 0x02, 0x03]);

		var sut = new FfmpegRunner(_ffmpegExe, new FakeIWebLogger());

		var (stream, result) = await sut.RunProcessAsync("/test.jpg", "-1",
			"image2");

		await sourceStream.DisposeAsync();

		Assert.IsNotNull(stream);
		Assert.IsTrue(result);

		await stream.DisposeAsync();
	}

	[TestMethod]
	public async Task Ffmpeg_RunProcessAsync_ExitCode()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test is only applicable on Unix-based systems.");
			return;
		}

		var readFile = await SetupFakeFfmpegExecutable(1);

		// overwrite the file with a bash script that will exit with code 1
		await CreateStubFile(_ffmpegExePosix,
			"#!/bin/bash\ntest_content\n exit 1");

		var sut = new FfmpegRunner(_ffmpegExe, new FakeIWebLogger());

		var (stream, result) = await sut.RunProcessAsync(readFile, "-1",
			"image2");

		Assert.IsNotNull(stream);
		Assert.IsFalse(result);

		await stream.DisposeAsync();
	}

	[TestMethod]
	public async Task Ffmpeg_RunProcessAsync_WithInvalidFfmpegPath()
	{
		var readFile = await SetupFakeFfmpegExecutable(2);

		var sut = new FfmpegRunner("invalid",
			new FakeIWebLogger());

		await Assert.ThrowsExactlyAsync<ArgumentException>(async () =>
			await sut.RunProcessAsync(readFile, "-1",
				"image2"));
	}

	[TestMethod]
	public async Task Ffmpeg_StreamToStreamRunner_Null()
	{
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
			await new FfmpegRunner(null!,
				new FakeIWebLogger()).RunProcessAsync(null!,
				null!, null!));
	}
}

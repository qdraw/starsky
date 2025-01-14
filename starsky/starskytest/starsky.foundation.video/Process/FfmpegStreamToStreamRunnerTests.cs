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
	private readonly StorageHostFullPathFilesystem _hostFullPathFilesystem;
	private string _ffmpegExe;
	private string _readFile;

	public FfmpegStreamToStreamRunnerTests()
	{
		_hostFullPathFilesystem = new StorageHostFullPathFilesystem(new FakeIWebLogger());
	}

	private void CreateStubFile(string path, string content)
	{
		var stream = StringToStreamHelper.StringToStream(content);
		_hostFullPathFilesystem.WriteStream(stream, path);
	}

	private async Task SetupFakeFfmpegExecutable()
	{
		var zipper = Zipper.ExtractZip([.. new CreateAnZipfileFakeFfMpeg().Bytes]);

		CreateStubFile(new CreateAnImage().BasePath + "ffmpeg",
			"#!/bin/bash\necho Fake Executable");
		_readFile = new CreateAnImage().BasePath + "read_file";
		CreateStubFile(_readFile, "test_content");

		var ffmpegExe = new MemoryStream(zipper.FirstOrDefault(p => p.Key == "ffmpeg.exe").Value);
		await _hostFullPathFilesystem.WriteStreamAsync(ffmpegExe,
			new CreateAnImage().BasePath + "ffmpeg.exe");

		await new FfMpegChmod(new FakeSelectorStorage(_hostFullPathFilesystem),
				new FakeIWebLogger())
			.Chmod(
				new CreateAnImage().BasePath + "ffmpeg");

		if ( new AppSettings().IsWindows )
		{
			_ffmpegExe = new CreateAnImage().BasePath + "ffmpeg.exe";
		}
		else
		{
			_ffmpegExe = new CreateAnImage().BasePath + "ffmpeg";
		}
	}

	[ClassCleanup]
	public static void CleanUp()
	{
		File.Delete(new CreateAnImage().BasePath + "ffmpeg");
		File.Delete(new CreateAnImage().BasePath + "ffmpeg.exe");
		File.Delete(new CreateAnImage().BasePath + "read_file");
	}

	[TestMethod]
	public async Task RunProcessAsync()
	{
		await SetupFakeFfmpegExecutable();
		var file = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(_readFile);
		var sut = new FfmpegStreamToStreamRunner(_ffmpegExe, file, new FakeIWebLogger());

		var (stream, result) = await sut.RunProcessAsync("-1", "image2", "test");

		Assert.IsNotNull(stream);
		Assert.IsTrue(result);

		await file.DisposeAsync();
		await stream.DisposeAsync();
	}

	[TestMethod]
	public async Task RunProcessAsync_WithInvalidFfmpegPath()
	{
		await SetupFakeFfmpegExecutable();

		var file = new StorageHostFullPathFilesystem(new FakeIWebLogger()).ReadStream(_readFile);
		var sut = new FfmpegStreamToStreamRunner("invalid", file, new FakeIWebLogger());

		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await sut.RunProcessAsync("-1", "image2", "test"));

		await file.DisposeAsync();
	}

	[TestMethod]
	public async Task RunProcessAsync_WithInvalidFfmpegPath_WithInvalidStream()
	{
		var sut = new FfmpegStreamToStreamRunner("invalid", Stream.Null, new FakeIWebLogger());

		await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
			await sut.RunProcessAsync("-1", "image2", "test"));
	}
}

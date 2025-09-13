using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Medallion.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class FfMpegChmodTests
{
	private readonly FfMpegChmod _ffMpegChmod;
	private readonly FfmpegExePath _ffmpegExePath;
	private readonly StorageHostFullPathFilesystem _hostFileSystemStorage;
	private readonly bool _isWindows;
	private readonly IWebLogger _logger;
	private readonly string _parentFolder;

	public FfMpegChmodTests()
	{
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		_logger = new FakeIWebLogger();
		_ffMpegChmod = new FfMpegChmod(new FakeSelectorStorage(_hostFileSystemStorage), _logger);

		_parentFolder = Path.Combine(new CreateAnImage().BasePath, "FfmpegChmodTests");

		_ffmpegExePath = new FfmpegExePath(new AppSettings { DependenciesFolder = _parentFolder });
		_isWindows = new AppSettings().IsWindows;
	}

	private void CreateFile()
	{
		_hostFileSystemStorage.CreateDirectory(_ffmpegExePath.GetExeParentFolder("linux-x64"));
		var stream = StringToStreamHelper.StringToStream("#!/bin/bash\necho Fake Ffmpeg");
		_hostFileSystemStorage.WriteStream(stream,
			_ffmpegExePath.GetExePath("linux-x64"));
		stream.Dispose();

		var result = Zipper.ExtractZip([.. CreateAnExifToolWindows.Bytes]);
		var (_, item) = result.FirstOrDefault(p => p.Key.Contains("exiftool"));

		_hostFileSystemStorage.CreateDirectory(_ffmpegExePath.GetExeParentFolder("win-x64"));

		_hostFileSystemStorage.WriteStream(new MemoryStream(item),
			Path.Combine(_ffmpegExePath.GetExeParentFolder("win-x64"), "chmod.exe"));
	}

	private void DeleteFile()
	{
		_hostFileSystemStorage.FileDelete(_ffmpegExePath.GetExePath("win-x64"));

		try
		{
			_hostFileSystemStorage.FolderDelete(_parentFolder);
		}
		catch ( UnauthorizedAccessException )
		{
			// do nothing
		}
	}

	[TestMethod]
	public async Task Chmod_ShouldReturnFalse_WhenChmodDoesNotExist()
	{
		var sut = new FfMpegChmod(new FakeSelectorStorage(), new FakeIWebLogger());
		var result = await sut.Chmod(_ffmpegExePath.GetExePath("linux-x64"));

		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task Chmod_ShouldReturnTrue_WhenCommandSucceeds__UnixOnly()
	{
		if ( _isWindows )
		{
			Assert.Inconclusive("This test is only for Unix-based systems");
			return;
		}

		CreateFile();

		var result = await _ffMpegChmod.Chmod(_ffmpegExePath.GetExePath("linux-x64"));

		var lsLah = await Command.Run("ls", "-lah",
			_ffmpegExePath.GetExePath("linux-x64")).Task;

		DeleteFile();

		Assert.IsTrue(result);
		Assert.StartsWith("-rwxr-xr-x", lsLah.StandardOutput);
	}

	[TestMethod]
	public async Task Chmod_ShouldReturnFalse_WhenCommandFails__UnixOnly()
	{
		if ( _isWindows )
		{
			Assert.Inconclusive("This test is only for Unix-based systems");
			return;
		}

		var sut = new FfMpegChmod(new FakeSelectorStorage(new FakeIStorage([],
				["/bin/chmod"])),
			new FakeIWebLogger());

		var result = await sut.Chmod("/_not_found_path/to/ffmpeg");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task Chmod_ShouldReturnTrue_WhenCommandSucceed__WindowsOnly()
	{
		if ( !_isWindows )
		{
			Assert.Inconclusive("This test is only for Windows systems");
			return;
		}

		CreateFile();

		var path = Path.Combine(_ffmpegExePath.GetExeParentFolder("win-x64"), "chmod.exe");
		
		Console.WriteLine("test> " + path);

		var sut = new FfMpegChmod(new FakeSelectorStorage(new FakeIStorage([],
				[path])),
			new FakeIWebLogger()) { CmdPath = path };

		var result = await sut.Chmod(path);
		Assert.IsTrue(result);

		DeleteFile();
	}

	[TestMethod]
	public async Task Chmod_ShouldLogError_WhenCommandFails__UnixOnly()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("This test is only applicable on Unix-based systems.");
			return;
		}

		var result = await _ffMpegChmod.Chmod("/_not_found_path/to/ffmpeg");
		Assert.IsFalse(result);

		Assert.IsTrue(( ( FakeIWebLogger ) _logger ).TrackedExceptions.Exists(entry =>
			entry.Item2?.Contains("command failed with exit code") == true));
	}
}

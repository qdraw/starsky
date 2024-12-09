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
public class FfmpegChmodTests
{
	private readonly FfmpegChmod _ffmpegChmod;
	private readonly FfmpegExePath _ffmpegExePath;
	private readonly StorageHostFullPathFilesystem _hostFileSystemStorage;
	private readonly bool _isWindows;
	private readonly IWebLogger _logger;
	private readonly string _parentFolder;

	public FfmpegChmodTests()
	{
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		_logger = new FakeIWebLogger();
		_ffmpegChmod = new FfmpegChmod(_hostFileSystemStorage, _logger);

		_parentFolder = Path.Combine(new CreateAnImage().BasePath, "FfmpegChmodTests");

		_ffmpegExePath = new FfmpegExePath(new AppSettings { DependenciesFolder = _parentFolder });
		_isWindows = new AppSettings().IsWindows;
	}

	private void CreateFile()
	{
		_hostFileSystemStorage.CreateDirectory(_ffmpegExePath.GetExeParentFolder());
		var stream = StringToStreamHelper.StringToStream("#!/bin/bash\necho Fake Ffmpeg");
		_hostFileSystemStorage.WriteStream(stream,
			_ffmpegExePath.GetExePath("linux-x64"));

		var result = Zipper.ExtractZip([.. CreateAnExifToolWindows.Bytes]);
		var (_, item) = result.FirstOrDefault(p => p.Key.Contains("exiftool"));

		_hostFileSystemStorage.WriteStream(new MemoryStream(item),
			Path.Combine(_ffmpegExePath.GetExeParentFolder(), "chmod.exe"));
	}

	private void DeleteFile()
	{
		_hostFileSystemStorage.FolderDelete(_parentFolder);
	}

	[TestMethod]
	public async Task Chmod_ShouldReturnFalse_WhenChmodDoesNotExist()
	{
		var sut = new FfmpegChmod(new FakeIStorage(), new FakeIWebLogger());
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

		var result = await _ffmpegChmod.Chmod(_ffmpegExePath.GetExePath("linux-x64"));

		var lsLah = await Command.Run("ls", "-lah",
			_ffmpegExePath.GetExePath("linux-x64")).Task;

		DeleteFile();

		Assert.IsTrue(result);
		Assert.IsTrue(lsLah.StandardOutput.StartsWith("-rwxr-xr-x"));
	}

	[TestMethod]
	public async Task Chmod_ShouldReturnFalse_WhenCommandFails__UnixOnly()
	{
		if ( _isWindows )
		{
			Assert.Inconclusive("This test is only for Unix-based systems");
			return;
		}

		var sut = new FfmpegChmod(new FakeIStorage([],
				["/bin/chmod"]),
			new FakeIWebLogger());

		var result = await sut.Chmod("/_not_found_path/to/ffmpeg");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public async Task Chmod_ShouldReturnFalse_WhenCommandSucceed__WindowsOnly()
	{
		if ( !_isWindows )
		{
			Assert.Inconclusive("This test is only for Windows systems");
			return;
		}

		CreateFile();

		var path = Path.Combine(_ffmpegExePath.GetExeParentFolder(), "chmod.exe");
		var sut = new FfmpegChmod(new FakeIStorage([],
				[path]),
			new FakeIWebLogger()) { CmdPath = path };

		var result = await sut.Chmod("/_not_found_path/to/ffmpeg");
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

		var result = await _ffmpegChmod.Chmod("/_not_found_path/to/ffmpeg");
		Assert.IsFalse(result);

		Assert.IsTrue(( ( FakeIWebLogger ) _logger ).TrackedExceptions.Exists(entry =>
			entry.Item2?.Contains("command failed with exit code") == true));
	}
}

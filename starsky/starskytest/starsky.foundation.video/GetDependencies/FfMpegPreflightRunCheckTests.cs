using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnZipfileFakeFFMpeg;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class FfMpegPreflightRunCheckTests
{
	private readonly AppSettings _appSettings;
	private readonly string _currentArchitecture;
	private readonly FfMpegChmod _ffMpegChmod;
	private readonly FfmpegExePath _ffmpegExePath;
	private readonly StorageHostFullPathFilesystem _hostFileSystemStorage;
	private readonly bool _isWindows;
	private readonly IWebLogger _logger;

	public FfMpegPreflightRunCheckTests()
	{
		_hostFileSystemStorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		_logger = new FakeIWebLogger();
		_ffMpegChmod = new FfMpegChmod(new FakeSelectorStorage(_hostFileSystemStorage), _logger);

		var parentFolder =
			Path.Combine(new CreateAnImage().BasePath, "FfMpegPreflightRunCheckTests");
		_appSettings = new AppSettings { DependenciesFolder = parentFolder };

		_ffmpegExePath = new FfmpegExePath(_appSettings);
		_currentArchitecture = CurrentArchitecture
			.GetCurrentRuntimeIdentifier();
		_isWindows = new AppSettings().IsWindows;
	}

	private async Task CreateFile(int exitCode, string echoName, bool enableChmod = true)
	{
		if ( _hostFileSystemStorage.ExistFolder(
			    _ffmpegExePath.GetExeParentFolder(_currentArchitecture)) )
		{
			_hostFileSystemStorage.FolderDelete(
				_ffmpegExePath.GetExeParentFolder(_currentArchitecture));
		}

		_hostFileSystemStorage.CreateDirectory(
			_ffmpegExePath.GetExeParentFolder(_currentArchitecture));
		var stream =
			StringToStreamHelper.StringToStream(
				$"#!/bin/bash\necho Fake {echoName}\nexit {exitCode}");
		await _hostFileSystemStorage.WriteStreamAsync(stream,
			_ffmpegExePath.GetExePath(_currentArchitecture));
		if ( enableChmod )
		{
			await _ffMpegChmod.Chmod(_ffmpegExePath.GetExePath());
		}

		var result = Zipper.ExtractZip([.. new CreateAnZipfileFakeFfMpeg().Bytes]);
		var (_, item) = result.FirstOrDefault(p => p.Key.Contains("ffmpeg.exe"));

		await _hostFileSystemStorage.WriteStreamAsync(new MemoryStream(item),
			Path.Combine(_ffmpegExePath.GetExeParentFolder(_currentArchitecture), "ffmpeg.exe"));
	}

	[TestMethod]
	public async Task TryRun_StatusCodeHappyFlow()
	{
		await CreateFile(0, "ffmpeg");

		// Arrange
		var ffMpegPreflightRunCheck = new FfMpegPreflightRunCheck(_appSettings, _logger);

		// Act
		var result = await ffMpegPreflightRunCheck.TryRun();

		// Assert
		Assert.IsTrue(result);
	}

	[DataTestMethod]
	[DataRow(0, "ffmpeg", true, true)]
	[DataRow(1, "ffmpeg", true, false)]
	[DataRow(0, "other_process", true, false)]
	[DataRow(0, "ffmpeg", false, false)]
	public async Task TryRun_StatusCode(int exitCode, string echoName, bool enableChmod,
		bool expectedResult)
	{
		if ( _isWindows )
		{
			Assert.Inconclusive("exit code 1 is not supported on windows");
			return;
		}

		await CreateFile(exitCode, echoName, enableChmod);

		// Arrange
		var ffMpegPreflightRunCheck = new FfMpegPreflightRunCheck(_appSettings, _logger);

		// Act
		var result = await ffMpegPreflightRunCheck.TryRun();

		// Assert
		Assert.AreEqual(expectedResult, result);
	}
}

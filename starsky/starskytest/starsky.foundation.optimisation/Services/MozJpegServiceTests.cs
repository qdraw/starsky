using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.optimisation.Interfaces;
using starsky.foundation.optimisation.Models;
using starsky.foundation.optimisation.Services;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.optimisation.Services;

[TestClass]
public sealed class MozJpegServiceTests
{
	private const string TestFolderName = "MozJpegServiceTests";
	private readonly string _basePath;
	private readonly StorageHostFullPathFilesystem _hostFileSystem;

	public MozJpegServiceTests()
	{
		_hostFileSystem = new StorageHostFullPathFilesystem(new FakeIWebLogger());
		_basePath = Path.Combine(new CreateAnImage().BasePath, TestFolderName);
		_hostFileSystem.CreateDirectory(_basePath);
	}

	[ClassCleanup]
	public static void CleanUp()
	{
		var basePath = Path.Combine(new CreateAnImage().BasePath, TestFolderName);
		if ( Directory.Exists(basePath) )
		{
			Directory.Delete(basePath, true);
		}
	}

	[TestMethod]
	public async Task RunMozJpeg_DownloadFails_LogsErrorAndReturns()
	{
		var appSettings = CreateAppSettings();
		var fakeLogger = new FakeIWebLogger();
		var download = new FakeMozJpegDownload(
			ImageOptimisationDownloadStatus.DownloadBinariesFailed);
		var sut = CreateService(appSettings, fakeLogger, download);

		await sut.RunMozJpeg(CreateOptimizer(),
		[
			new ImageOptimisationItem
			{
				InputPath = Path.Combine(_basePath, "input.jpg"),
				OutputPath = Path.Combine(_basePath, "input.jpg")
			}
		]);

		Assert.Contains(entry =>
			entry.Item2?.Contains("MozJPEG download failed") == true, fakeLogger.TrackedExceptions);
	}

	[TestMethod]
	public async Task RunMozJpeg_CJpegMissing_LogsErrorAndReturns()
	{
		var appSettings = CreateAppSettings();
		var fakeLogger = new FakeIWebLogger();
		var download = new FakeMozJpegDownload(ImageOptimisationDownloadStatus.Ok);
		var sut = CreateService(appSettings, fakeLogger, download);

		await sut.RunMozJpeg(CreateOptimizer(),
		[
			new ImageOptimisationItem
			{
				InputPath = Path.Combine(_basePath, "input.jpg"),
				OutputPath = Path.Combine(_basePath, "input.jpg")
			}
		]);

		Assert.Contains(entry =>
			entry.Item2?.Contains("[ImageOptimisationService] MozJPEG not found at") == true, fakeLogger.TrackedExceptions);
	}

	[TestMethod]
	public async Task RunMozJpeg_NonJpegOutput_SkipsProcessing()
	{
		var appSettings = CreateAppSettings();
		var cJpegPath = await CreateCjpegFile(appSettings, "");
		WriteBytes(Path.Combine(_basePath, "image.png"), CreateAnPng.Bytes.ToArray());

		var fakeLogger = new FakeIWebLogger();
		var download = new FakeMozJpegDownload(ImageOptimisationDownloadStatus.Ok);
		var sut = CreateService(appSettings, fakeLogger, download);

		await sut.RunMozJpeg(CreateOptimizer(),
		[
			new ImageOptimisationItem
			{
				InputPath = Path.Combine(_basePath, "image.png"),
				OutputPath = Path.Combine(_basePath, "image.png")
			}
		]);

		var tempFilePath = Path.Combine(_basePath, "image.png.optimizing");
		Assert.IsFalse(_hostFileSystem.ExistFile(tempFilePath));
		Assert.IsTrue(_hostFileSystem.ExistFile(cJpegPath));
	}

	[TestMethod]
	public async Task RunMozJpeg_JpegOutput_Success_ReplacesFile()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("Requires a shell script stub on Unix-based systems.");
			return;
		}

		var appSettings = CreateAppSettings();
		await CreateCjpegFile(appSettings, "#!/bin/bash\ncat \"$4\"");

		var outputPath = Path.Combine(_basePath, "photo.jpg");
		WriteBytes(outputPath, CreateAnImage.Bytes.ToArray());

		var fakeLogger = new FakeIWebLogger();
		var download = new FakeMozJpegDownload(ImageOptimisationDownloadStatus.Ok);
		var sut = CreateService(appSettings, fakeLogger, download);

		await sut.RunMozJpeg(CreateOptimizer(),
		[
			new ImageOptimisationItem { InputPath = outputPath, OutputPath = outputPath }
		]);

		var tempFilePath = outputPath + ".optimizing";
		Assert.IsFalse(_hostFileSystem.ExistFile(tempFilePath));
		Assert.IsTrue(_hostFileSystem.ExistFile(outputPath));
	}

	[TestMethod]
	public async Task RunMozJpeg_JpegOutput_CommandFails_LogsErrorAndKeepsOriginal()
	{
		if ( new AppSettings().IsWindows )
		{
			Assert.Inconclusive("Requires a shell script stub on Unix-based systems.");
			return;
		}

		var appSettings = CreateAppSettings();
		await CreateCjpegFile(appSettings, "#!/bin/bash\nexit 1");

		var outputPath = Path.Combine(_basePath, "photo-fail.jpg");
		var originalBytes = CreateAnImage.Bytes.ToArray();
		WriteBytes(outputPath, originalBytes);

		var fakeLogger = new FakeIWebLogger();
		var download = new FakeMozJpegDownload(ImageOptimisationDownloadStatus.Ok);
		var sut = CreateService(appSettings, fakeLogger, download);

		await sut.RunMozJpeg(CreateOptimizer(),
		[
			new ImageOptimisationItem { InputPath = outputPath, OutputPath = outputPath }
		]);

		var tempFilePath = outputPath + ".optimizing";
		Assert.IsFalse(_hostFileSystem.ExistFile(tempFilePath));
		Assert.IsTrue(_hostFileSystem.ExistFile(outputPath));
		Assert.Contains(entry =>
			entry.Item2?.Contains("cjpeg failed") == true, fakeLogger.TrackedExceptions);
	}

	private AppSettings CreateAppSettings()
	{
		return new AppSettings { DependenciesFolder = Path.Combine(_basePath, "deps") };
	}

	private static Optimizer CreateOptimizer()
	{
		return new Optimizer
		{
			Enabled = true,
			Id = "mozjpeg",
			ImageFormats = [ExtensionRolesHelper.ImageFormat.jpg],
			Options = new OptimizerOptions { Quality = 80 }
		};
	}

	private MozJpegService CreateService(AppSettings appSettings, FakeIWebLogger logger,
		IMozJpegDownload download)
	{
		return new MozJpegService(appSettings,
			new FakeSelectorStorage(_hostFileSystem), logger, download);
	}

	private async Task<string> CreateCjpegFile(AppSettings appSettings, string content)
	{
		var architecture = CurrentArchitecture.GetCurrentRuntimeIdentifier();
		var fileName = appSettings.IsWindows ? "cjpeg.exe" : "cjpeg";
		var dir = Path.Combine(appSettings.DependenciesFolder, "mozjpeg", architecture);
		_hostFileSystem.CreateDirectory(dir);
		var fullPath = Path.Combine(dir, fileName);

		if ( !string.IsNullOrEmpty(content) )
		{
			var stream = StringToStreamHelper.StringToStream(content);
			await _hostFileSystem.WriteStreamAsync(stream, fullPath);
		}
		else
		{
			WriteBytes(fullPath, [0x00]);
		}

		if ( !appSettings.IsWindows )
		{
			await new FfMpegChmod(new FakeSelectorStorage(_hostFileSystem),
					new FakeIWebLogger())
				.Chmod(fullPath);
		}

		return fullPath;
	}

	private void WriteBytes(string path, byte[] bytes)
	{
		using var stream = new MemoryStream(bytes);
		_hostFileSystem.WriteStream(stream, path);
	}

	private sealed class FakeMozJpegDownload : IMozJpegDownload
	{
		private readonly ImageOptimisationDownloadStatus _status;

		public FakeMozJpegDownload(ImageOptimisationDownloadStatus status)
		{
			_status = status;
		}

		public Task<ImageOptimisationDownloadStatus> Download(string? architecture = null,
			int retryInSeconds = 15)
		{
			return Task.FromResult(_status);
		}
	}
}

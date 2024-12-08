using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.video.GetDependencies.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class FfMpegDownloadTest
{
	private const string DependencyFolderName = "FfMpegDownloadTest";
	private readonly FakeIHttpClientHelper _emptyHttpClientHelper;
	private readonly FakeIHttpClientHelper _httpClientHelper;

	private readonly FakeIStorage _storage = new();

	public FfMpegDownloadTest()
	{
		_emptyHttpClientHelper =
			new FakeIHttpClientHelper(_storage,
				new Dictionary<string, KeyValuePair<bool, string>>());

		var example = new FfmpegBinariesIndex
		{
			Binaries = new List<BinaryIndex>
			{
				new()
				{
					Architecture = "win-x64",
					FileName = "test.zip",
					Sha256 = "test-sha256"
				},
				new()
				{
					Architecture = "osx-x64",
					FileName = "test.zip",
					Sha256 = "test-sha256"
				},
				new()
				{
					Architecture = "linux-x64",
					FileName = "test.zip",
					Sha256 = "test-sha256"
				}
			}
		};

		_httpClientHelper =
			new FakeIHttpClientHelper(_storage,
				new Dictionary<string, KeyValuePair<bool, string>>
				{
					{
						FfMpegDownloadIndex.FfMpegApiIndex.ToString(),
						new KeyValuePair<bool, string>(
							true,
							JsonSerializer.Serialize(example, DefaultJsonSerializer.CamelCase))
					}
				});
	}

	// [TestMethod]
	// public async Task DownloadFfMpegTest()
	// {
	// 	var sut = new FfMpegDownload(
	// 		new HttpClientHelper(new HttpProvider(new HttpClient()),
	// 			new StorageHostFullPathFilesystem(new FakeIWebLogger()), new FakeIWebLogger()),
	// 		new AppSettings(), new FakeIWebLogger(),
	// 		new MacCodeSign(
	// 			new FakeSelectorStorage(new StorageHostFullPathFilesystem(new FakeIWebLogger())),
	// 			new FakeIWebLogger()));
	//
	// 	await sut.DownloadFfMpeg();
	//
	// 	Assert.Fail();
	// }

	[TestMethod]
	[DataRow("FfmpegSkipDownloadOnStartup")]
	[DataRow("AddSwaggerExportAndAddSwaggerExportExitAfter")]
	public async Task DownloadFfMpeg_ShouldSkipDueToSettings(string settingName)
	{
		var appSettings = new AppSettings();
		switch ( settingName )
		{
			case "FfmpegSkipDownloadOnStartup":
				appSettings.FfmpegSkipDownloadOnStartup = true;
				break;
			case "AddSwaggerExportAndAddSwaggerExportExitAfter":
				appSettings.AddSwaggerExport = true;
				appSettings.AddSwaggerExportExitAfter = true;
				break;
		}

		var logger = new FakeIWebLogger();
		var macCodeSign = new FakeIMacCodeSign();

		var ffmpegDownload =
			new FfMpegDownload(_emptyHttpClientHelper, appSettings, logger, macCodeSign,
				new FakeIFfMpegDownloadIndex());

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.SettingsDisabled, result);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_MissingIndex()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();
		var macCodeSign = new FakeIMacCodeSign();

		var ffmpegDownload =
			new FfMpegDownload(_emptyHttpClientHelper, appSettings, logger, macCodeSign,
				new FakeIFfMpegDownloadIndex());

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadIndexFailed, result);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_DownloadBinariesFailed111()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();
		var macCodeSign = new FakeIMacCodeSign();

		var ffmpegDownload =
			new FfMpegDownload(_httpClientHelper, appSettings, logger, macCodeSign,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = new FfmpegBinariesIndex { Binaries = new List<BinaryIndex>() }
				}));

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailed, result);
	}
	
	[TestMethod]
	public async Task DownloadFfMpeg_DownloadBinariesFailed()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();
		var macCodeSign = new FakeIMacCodeSign();

		var ffmpegDownload =
			new FfMpegDownload(_httpClientHelper, appSettings, logger, macCodeSign,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = new FfmpegBinariesIndex { Binaries = new List<BinaryIndex>() }
				}));

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailed, result);
	}

	// [TestMethod]
	// public async Task Download_ShouldReturnTrueIfFileExists()
	// {
	// 	var appSettings = new AppSettings { DependenciesFolder = "test-dependencies" };
	// 	var logger = new FakeIWebLogger();
	// 	var macCodeSign = new FakeIMacCodeSign();
	//
	// 	var ffmpegDownload = new FfMpegDownload(_httpClientHelper, appSettings, logger, macCodeSign);
	//
	// 	var binaryIndex = new BinaryIndex { FileName = "test.zip", Sha256 = "test-sha256" };
	// 	var baseUrls = new List<Uri> { new Uri("http://example.com/") };
	//
	// 	var result =
	// 		await ffmpegDownload.Download(
	// 			new KeyValuePair<BinaryIndex?, List<Uri>>(binaryIndex, baseUrls), "win-x64");
	//
	// 	Assert.IsTrue(result);
	// }
	//
	// [TestMethod]
	// public async Task Download_ShouldReturnNullIfBinaryIndexIsNull()
	// {
	// 	var appSettings = new AppSettings { DependenciesFolder = "test-dependencies" };
	// 	var logger = new FakeIWebLogger();
	// 	var httpClientHelper = new FakeHttpClientHelper();
	// 	var macCodeSign = new FakeMacCodeSign();
	// 	var hostFileSystemStorage = new FakeStorageHostFullPathFilesystem(logger);
	//
	// 	var ffmpegDownload = new FfMpegDownload(httpClientHelper, appSettings, logger, macCodeSign);
	//
	// 	var result =
	// 		await ffmpegDownload.Download(
	// 			new KeyValuePair<BinaryIndex?, List<Uri>>(null, new List<Uri>()), "win-x64");
	//
	// 	Assert.IsNull(result);
	// }
	//
	// [TestMethod]
	// public async Task PrepareBeforeRunning_ShouldReturnFalseIfFileDoesNotExist()
	// {
	// 	var appSettings = new AppSettings { DependenciesFolder = "test-dependencies" };
	// 	var logger = new FakeIWebLogger();
	// 	var httpClientHelper = new FakeHttpClientHelper();
	// 	var macCodeSign = new FakeMacCodeSign();
	// 	var hostFileSystemStorage = new FakeStorageHostFullPathFilesystem(logger);
	//
	// 	var ffmpegDownload = new FfMpegDownload(httpClientHelper, appSettings, logger, macCodeSign);
	//
	// 	var result = await ffmpegDownload.PrepareBeforeRunning("win-x64");
	//
	// 	Assert.IsFalse(result);
	// }
	//
	// [TestMethod]
	// public async Task PrepareBeforeRunning_ShouldReturnTrueForWindows()
	// {
	// 	var appSettings = new AppSettings { DependenciesFolder = "test-dependencies" };
	// 	var logger = new FakeIWebLogger();
	// 	var httpClientHelper = new FakeHttpClientHelper();
	// 	var macCodeSign = new FakeMacCodeSign();
	// 	var hostFileSystemStorage = new FakeStorageHostFullPathFilesystem(logger);
	//
	// 	var ffmpegDownload = new FfMpegDownload(httpClientHelper, appSettings, logger, macCodeSign);
	//
	// 	var result = await ffmpegDownload.PrepareBeforeRunning("win-x64");
	//
	// 	Assert.IsTrue(result);
	// }
	//
	// [TestMethod]
	// public async Task PrepareBeforeRunning_ShouldReturnTrueForMac()
	// {
	// 	var appSettings = new AppSettings { DependenciesFolder = "test-dependencies" };
	// 	var logger = new FakeIWebLogger();
	// 	var httpClientHelper = new FakeHttpClientHelper();
	// 	var macCodeSign = new FakeMacCodeSign();
	// 	var hostFileSystemStorage = new FakeStorageHostFullPathFilesystem(logger);
	//
	// 	var ffmpegDownload = new FfMpegDownload(httpClientHelper, appSettings, logger, macCodeSign);
	//
	// 	var result = await ffmpegDownload.PrepareBeforeRunning("osx-x64");
	//
	// 	Assert.IsTrue(result);
	// }
}

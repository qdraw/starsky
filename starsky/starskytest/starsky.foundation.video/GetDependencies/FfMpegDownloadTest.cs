using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Architecture;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.video.GetDependencies.Models;
using starskytest.FakeCreateAn.CreateAnZipFileMacOs;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class FfMpegDownloadTest
{
	private const string DependencyFolderName = "FfMpegDownloadTest";
	private readonly FfmpegBinariesIndex _exampleFfmpegBinariesIndex;
	private readonly FakeIHttpClientHelper _httpClientHelper;

	private readonly FakeIStorage _storage = new();

	public FfMpegDownloadTest()
	{
		_exampleFfmpegBinariesIndex = CreateExampleFile();

		_httpClientHelper =
			new FakeIHttpClientHelper(_storage,
				new Dictionary<string, KeyValuePair<bool, string>>
				{
					{
						FfMpegDownloadIndex.FfMpegApiIndex.ToString(),
						new KeyValuePair<bool, string>(
							true,
							JsonSerializer.Serialize(_exampleFfmpegBinariesIndex,
								DefaultJsonSerializer.CamelCase))
					},
					{
						"https://qdraw.nl/mock_test.zip", new KeyValuePair<bool, string>(
							true,
							"VGVzdENvbnRlbnQ=")
					}
				});
	}

	private static FfmpegBinariesIndex CreateExampleFile(string sha256 = "invalid-sha256")
	{
		return new FfmpegBinariesIndex
		{
			Binaries =
			[
				new BinaryIndex
				{
					Architecture = "win-x64", FileName = "mock_test.zip", Sha256 = sha256
				},
				new BinaryIndex
				{
					Architecture = "osx-x64", FileName = "mock_test.zip", Sha256 = sha256
				},
				new BinaryIndex
				{
					Architecture = "linux-x64", FileName = "mock_test.zip", Sha256 = sha256
				},
				new BinaryIndex
				{
					Architecture = "osx-arm64", FileName = "mock_test.zip", Sha256 = sha256
				},
				new BinaryIndex
				{
					Architecture = "linux-arm64", FileName = "mock_test.zip", Sha256 = sha256
				},
				new BinaryIndex
				{
					Architecture = "linux-arm", FileName = "mock_test.zip", Sha256 = sha256
				}
			]
		};
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

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(), appSettings,
				logger, new FakeIFfMpegDownloadIndex(), new FakeIFfMpegDownloadBinaries(),
				new FakeIFfMpegPrepareBeforeRunning());

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.SettingsDisabled, result);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_MissingIndex()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(), appSettings,
				logger,
				new FakeIFfMpegDownloadIndex(), new FakeIFfMpegDownloadBinaries(),
				new FakeIFfMpegPrepareBeforeRunning());

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadIndexFailed, result);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_DownloadBinariesFail()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(), appSettings, logger,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = new FfmpegBinariesIndex { Binaries = new List<BinaryIndex>() }
				}),
				new FakeIFfMpegDownloadBinaries(FfmpegDownloadStatus
					.DownloadBinariesFailedMissingFileName), new FakeIFfMpegPrepareBeforeRunning());

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedMissingFileName, result);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_FileAlreadyExists()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();

		var storage = new FakeIStorage([],
		[
			new FfmpegExePath(appSettings).GetExePath(CurrentArchitecture
				.GetCurrentRuntimeIdentifier())
		]);

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
				logger,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = new FfmpegBinariesIndex { Binaries = new List<BinaryIndex>() }
				}), new FakeIFfMpegDownloadBinaries(), new FakeIFfMpegPrepareBeforeRunning());

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.Ok, result);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_DownloadFail_InvalidShaHash()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
				logger,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = _exampleFfmpegBinariesIndex,
					BaseUrls = new List<Uri> { new("https://qdraw.nl/") }
				}),
				new FfMpegDownloadBinaries(new FakeSelectorStorage(storage), _httpClientHelper,
					appSettings, logger, new Zipper(new FakeIWebLogger())),
				new FakeIFfMpegPrepareBeforeRunning());

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedSha256Check, result);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_DownloadFail_ZipFileNotFound()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
				logger,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = CreateExampleFile(
						"e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"),
					BaseUrls = new List<Uri> { new("https://qdraw.nl/") }
				}),
				new FfMpegDownloadBinaries(new FakeSelectorStorage(storage), _httpClientHelper,
					appSettings, logger, new Zipper(new FakeIWebLogger())),
				new FakeIFfMpegPrepareBeforeRunning());

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedZipperNotExtracted, result);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_PrepareBeforeRunningFail()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(["/"],
			new List<string> { "FfMpegDownloadTest/mock_test.zip" },
			new List<byte[]?> { CreateAnZipFileMacOs.Bytes.ToArray() });

		var zipper = new FakeIZipper(new List<Tuple<string, byte[]>>
		{
			new("FfMpegDownloadTest/mock_test.zip",
				[.. CreateAnZipFileMacOs.Bytes])
		}, storage);
		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
				logger,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = CreateExampleFile(
						"4d116276a8049b9d914c94f2827126d3c99e3cc97f021d3611ac7901d17f8e73"),
					BaseUrls = new List<Uri> { new("https://qdraw.nl/") }
				}), new FfMpegDownloadBinaries(new FakeSelectorStorage(storage), _httpClientHelper,
					appSettings, logger, zipper),
				new FfMpegPrepareBeforeRunning(new FakeSelectorStorage(storage),
					new FakeIMacCodeSign(), new FfMpegChmod(storage, logger), appSettings, logger));

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.PrepareBeforeRunningFailed, result);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_AllStages()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(["/"],
			new List<string> { "FfMpegDownloadTest/mock_test.zip", "/bin/chmod" },
			new List<byte[]?>
			{
				CreateAnZipFileMacOs.Bytes.ToArray(), CreateAnZipFileMacOs.Bytes.ToArray()
			});
		var zipper = new FakeIZipper(new List<Tuple<string, byte[]>>
		{
			new("FfMpegDownloadTest/mock_test.zip",
				[.. CreateAnZipFileMacOs.Bytes])
		}, storage);
		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
				logger,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = CreateExampleFile(
						"4d116276a8049b9d914c94f2827126d3c99e3cc97f021d3611ac7901d17f8e73"),
					BaseUrls = new List<Uri> { new("https://qdraw.nl/") }
				}), new FfMpegDownloadBinaries(new FakeSelectorStorage(storage), _httpClientHelper,
					appSettings, logger, zipper), new FfMpegPrepareBeforeRunning(
					new FakeSelectorStorage(storage),
					new FakeIMacCodeSign(new Dictionary<string, bool?>
					{
						{ "FfMpegDownloadTest/ffmpeg/ffmpeg", true }
					}), new FakeIFfmpegChmod(storage), appSettings, logger));

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.PrepareBeforeRunningFailed, result);
	}
}

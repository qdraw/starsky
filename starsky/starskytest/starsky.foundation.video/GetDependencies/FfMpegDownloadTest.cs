using System;
using System.Collections.Generic;
using System.IO;
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
using starskytest.FakeCreateAn.CreateAnZipfileFakeFFMpeg;
using starskytest.FakeCreateAn.CreateAnZipFileMacOs;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class FfMpegDownloadTest
{
	private const string DependencyFolderName = "FfMpegDownloadTest";
	private readonly FfmpegBinariesIndex _exampleFfmpegBinariesIndex;
	private readonly FakeIHttpClientHelper _httpClientHelper;
	private readonly bool _isWindows;

	private readonly FakeIStorage _storage = new();

	public FfMpegDownloadTest()
	{
		_exampleFfmpegBinariesIndex = CreateExampleFile();
		_isWindows = new AppSettings().IsWindows;

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
					Architecture = "win-arm64", FileName = "mock_test.zip", Sha256 = sha256
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
				new FakeIFfMpegPrepareBeforeRunning(false), new FakeIFfMpegPreflightRunCheck());

		var result = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.SettingsDisabled, result);
	}

	[DataTestMethod]
	[DataRow("test-1,test-2", 2)]
	[DataRow("", 1)]
	[DataRow("", 1)]
	public async Task DownloadFfMpeg_MultiArg(string arg, int count)
	{
		var storage = new FakeIStorage();
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
				logger,
				new FakeIFfMpegDownloadIndex(), new FakeIFfMpegDownloadBinaries(),
				new FakeIFfMpegPrepareBeforeRunning(false), new FakeIFfMpegPreflightRunCheck());

		var resultMissingIndex = await ffmpegDownload.DownloadFfMpeg(
			[.. arg.Split(",")]);

		Assert.AreEqual(count, resultMissingIndex.Count);
		if ( resultMissingIndex.Count == 2 )
		{
			Assert.AreEqual(FfmpegDownloadStatus.DownloadIndexFailed, resultMissingIndex[0]);
			Assert.AreEqual(FfmpegDownloadStatus.DownloadIndexFailed, resultMissingIndex[1]);
		}
		else
		{
			Assert.AreEqual(FfmpegDownloadStatus.DownloadIndexFailed, resultMissingIndex[0]);
		}
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
				new FakeIFfMpegPrepareBeforeRunning(false), new FakeIFfMpegPreflightRunCheck());

		var resultMissingIndex = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadIndexFailed, resultMissingIndex);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_MissingIndex_DownloadBinariesFailedMissingFileName()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(_storage), appSettings,
				logger,
				// No Index but it says is succeed
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer { Success = true }),
				new FfMpegDownloadBinaries(new FakeSelectorStorage(_storage), _httpClientHelper,
					appSettings, logger, new Zipper(new FakeIWebLogger())),
				new FakeIFfMpegPrepareBeforeRunning(false), new FakeIFfMpegPreflightRunCheck());

		var resultMissingIndex = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedMissingFileName,
			resultMissingIndex);
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
					Success = true, Data = new FfmpegBinariesIndex { Binaries = [] }
				}),
				new FakeIFfMpegDownloadBinaries(FfmpegDownloadStatus
					.DownloadBinariesFailedMissingFileName),
				new FakeIFfMpegPrepareBeforeRunning(false),
				new FakeIFfMpegPreflightRunCheck());

		var resultBinaryFail = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedMissingFileName,
			resultBinaryFail);
	}

	[DataTestMethod]
	[DataRow(false, true, DisplayName = "TryRun False & Ok")]
	[DataRow(false, false, DisplayName = "TryRun False & PrepareBeforeRunningFailed")]
	[DataRow(true, true, DisplayName = "TryRun True & Ok 1")]
	[DataRow(true, false, DisplayName = "TryRun True & Ok 2")]
	public async Task DownloadFfMpeg_FileAlreadyExists(bool tryRun, bool isPrepareBeforeRunning)
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
				}), new FakeIFfMpegDownloadBinaries(),
				new FakeIFfMpegPrepareBeforeRunning(isPrepareBeforeRunning),
				new FakeIFfMpegPreflightRunCheck(tryRun ? storage : null,
					tryRun ? appSettings : null));

		var resultFileAlreadyExists = await ffmpegDownload.DownloadFfMpeg();

		if ( tryRun && !isPrepareBeforeRunning)
		{
			Assert.AreEqual(FfmpegDownloadStatus.Ok, resultFileAlreadyExists);
			return;
		}
		
		Assert.AreEqual(
			isPrepareBeforeRunning
				? FfmpegDownloadStatus.Ok
				: FfmpegDownloadStatus.PrepareBeforeRunningFailed,
			resultFileAlreadyExists);
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
				new FakeIFfMpegPrepareBeforeRunning(false), new FakeIFfMpegPreflightRunCheck());

		var resultInvalidHash = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedSha256Check, resultInvalidHash);
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
				new FakeIFfMpegPrepareBeforeRunning(false), new FakeIFfMpegPreflightRunCheck());

		var resultZipFail = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedZipperNotExtracted,
			resultZipFail);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_PrepareBeforeRunningFail()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(["/"],
			new List<string> { $"FfMpegDownloadTest{Path.DirectorySeparatorChar}mock_test.zip" },
			new List<byte[]?> { new CreateAnZipfileFakeFfMpeg().Bytes.ToArray() });

		var zipper = new FakeIZipper(new List<Tuple<string, byte[]>>
		{
			new($"FfMpegDownloadTest{Path.DirectorySeparatorChar}mock_test.zip",
				[.. new CreateAnZipfileFakeFfMpeg().Bytes])
		}, storage);

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
				logger,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = CreateExampleFile(
						"31852c0b33f35ff16e96d53be370ce86df92db6d4633ab0a8dae38acbf393ead"),
					BaseUrls = new List<Uri> { new("https://qdraw.nl/") }
				}), new FfMpegDownloadBinaries(new FakeSelectorStorage(storage), _httpClientHelper,
					appSettings, logger, zipper),
				new FfMpegPrepareBeforeRunning(new FakeSelectorStorage(storage),
					new FakeIMacCodeSign(),
					new FfMpegChmod(new FakeSelectorStorage(storage), logger), appSettings,
					logger), new FakeIFfMpegPreflightRunCheck(storage, appSettings));

		var resultPrepFail = await ffmpegDownload.DownloadFfMpeg();

		if ( _isWindows )
		{
			Assert.AreEqual(FfmpegDownloadStatus.Ok, resultPrepFail);
			return;
		}

		Assert.AreEqual(FfmpegDownloadStatus.PrepareBeforeRunningFailed, resultPrepFail);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_PreflightRunCheckFailed()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(["/"],
			new List<string>
			{
				$"FfMpegDownloadTest{Path.DirectorySeparatorChar}mock_test.zip", "/bin/chmod"
			},
			new List<byte[]?>
			{
				new CreateAnZipfileFakeFfMpeg().Bytes.ToArray(),
				CreateAnZipFileMacOs.Bytes.ToArray()
			});
		var zipper = new FakeIZipper(new List<Tuple<string, byte[]>>
		{
			new($"FfMpegDownloadTest{Path.DirectorySeparatorChar}mock_test.zip",
				[.. new CreateAnZipfileFakeFfMpeg().Bytes.ToArray()])
		}, storage);

		const string hash = "31852c0b33f35ff16e96d53be370ce86df92db6d4633ab0a8dae38acbf393ead";

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
				logger,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = CreateExampleFile(hash),
					BaseUrls = new List<Uri> { new("https://qdraw.nl/") }
				}), new FfMpegDownloadBinaries(new FakeSelectorStorage(storage),
					_httpClientHelper,
					appSettings, logger, zipper), new FfMpegPrepareBeforeRunning(
					new FakeSelectorStorage(storage),
					new FakeIMacCodeSign(new Dictionary<string, bool?>
					{
						{
							$"FfMpegDownloadTest/ffmpeg-{CurrentArchitecture.GetCurrentRuntimeIdentifier()}/ffmpeg",
							true
						}
					}), new FakeIFfmpegChmod(storage), appSettings, logger),
				new FfMpegPreflightRunCheck(appSettings, new FakeIWebLogger()));

		var resultAllStages = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.PreflightRunCheckFailed, resultAllStages);
	}

	[TestMethod]
	public async Task DownloadFfMpeg_AllStages()
	{
		var appSettings = new AppSettings { DependenciesFolder = DependencyFolderName };
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(["/"],
			new List<string>
			{
				$"FfMpegDownloadTest{Path.DirectorySeparatorChar}mock_test.zip", "/bin/chmod"
			},
			new List<byte[]?>
			{
				new CreateAnZipfileFakeFfMpeg().Bytes.ToArray(),
				CreateAnZipFileMacOs.Bytes.ToArray()
			});
		var zipper = new FakeIZipper(new List<Tuple<string, byte[]>>
		{
			new($"FfMpegDownloadTest{Path.DirectorySeparatorChar}mock_test.zip",
				[.. new CreateAnZipfileFakeFfMpeg().Bytes.ToArray()])
		}, storage);

		const string hash = "31852c0b33f35ff16e96d53be370ce86df92db6d4633ab0a8dae38acbf393ead";

		var ffmpegDownload =
			new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
				logger,
				new FakeIFfMpegDownloadIndex(new FfmpegBinariesContainer
				{
					Success = true,
					Data = CreateExampleFile(hash),
					BaseUrls = new List<Uri> { new("https://qdraw.nl/") }
				}), new FfMpegDownloadBinaries(new FakeSelectorStorage(storage),
					_httpClientHelper,
					appSettings, logger, zipper), new FfMpegPrepareBeforeRunning(
					new FakeSelectorStorage(storage),
					new FakeIMacCodeSign(new Dictionary<string, bool?>
					{
						{
							$"FfMpegDownloadTest/ffmpeg-{CurrentArchitecture.GetCurrentRuntimeIdentifier()}/ffmpeg",
							true
						}
					}), new FakeIFfmpegChmod(storage), appSettings, logger),
				new FakeIFfMpegPreflightRunCheck(storage, appSettings));

		var resultAllStages = await ffmpegDownload.DownloadFfMpeg();

		Assert.AreEqual(FfmpegDownloadStatus.Ok, resultAllStages);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("not-found")]
	[DataRow("test")]
	public void GetSetFfMpegPath_ReturnsCorrectPath_Found(string? dependenciesFolder)
	{
		var appSettings = new AppSettings { DependenciesFolder = dependenciesFolder! };

		var logger = new FakeIWebLogger();
		var exeExtension = string.Empty;
		if ( new AppSettings().IsWindows )
		{
			exeExtension = ".exe";
		}

		var expectedPath = Path.Combine(dependenciesFolder ?? "",
			$"ffmpeg-{CurrentArchitecture.GetCurrentRuntimeIdentifier()}",
			$"ffmpeg{exeExtension}");

		var storage = new FakeIStorage([], [expectedPath]);

		if ( dependenciesFolder == "not-found" )
		{
			storage = new FakeIStorage();
		}

		var ffmpegDownload = new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
			logger,
			new FakeIFfMpegDownloadIndex(), new FakeIFfMpegDownloadBinaries(),
			new FakeIFfMpegPrepareBeforeRunning(false), new FakeIFfMpegPreflightRunCheck());

		var path = ffmpegDownload.GetSetFfMpegPath();

		Assert.AreEqual(expectedPath, path);
		Assert.AreEqual(expectedPath, appSettings.FfmpegPath);
	}

	[TestMethod]
	public void GetSetFfMpegPath_ReturnsCorrectPath_AlreadySet()
	{
		var appSettings = new AppSettings
		{
			DependenciesFolder = "test", FfmpegPath = "/usr/local/bin/ffmpeg"
		};

		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage(new List<string>(),
			new List<string> { "/usr/local/bin/ffmpeg" });

		var ffmpegDownload = new FfMpegDownload(new FakeSelectorStorage(storage), appSettings,
			logger,
			new FakeIFfMpegDownloadIndex(), new FakeIFfMpegDownloadBinaries(),
			new FakeIFfMpegPrepareBeforeRunning(false), new FakeIFfMpegPreflightRunCheck());

		var path = ffmpegDownload.GetSetFfMpegPath();

		Assert.AreEqual("/usr/local/bin/ffmpeg", path);
		Assert.AreEqual("/usr/local/bin/ffmpeg", appSettings.FfmpegPath);
	}
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.ArchiveFormats;
using starsky.foundation.video.GetDependencies;
using starsky.foundation.video.GetDependencies.Models;
using starskytest.FakeCreateAn.CreateAnZipFileMacOs;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.GetDependencies;

[TestClass]
public class FfMpegDownloadBinariesTests
{
	private readonly AppSettings _appSettings = new();
	private readonly FakeIHttpClientHelper _httpClientHelper;
	private readonly FakeIWebLogger _logger = new();
	private readonly FakeIStorage _storage = new();

	public FfMpegDownloadBinariesTests()
	{
		_httpClientHelper =
			new FakeIHttpClientHelper(_storage,
				new Dictionary<string, KeyValuePair<bool, string>>
				{
					{
						"https://qdraw.nl/mock_test.zip", new KeyValuePair<bool, string>(
							true,
							"VGVzdENvbnRlbnQ=")
					}
				});
	}

	[TestMethod]
	public async Task Download_MissingFileName()
	{
		var sut = new FfMpegDownloadBinaries(new FakeSelectorStorage(_storage), _httpClientHelper,
			_appSettings, _logger, new Zipper(new FakeIWebLogger()));

		var baseUrls = new List<Uri> { new("https://qdraw.nl/") };
		var binaryIndexKeyValuePair = new KeyValuePair<BinaryIndex?, List<Uri>>(null, baseUrls);

		var result = await sut.Download(binaryIndexKeyValuePair, string.Empty);

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedMissingFileName, result);
	}

	[TestMethod]
	public async Task Download_InvalidDownload()
	{
		var sut = new FfMpegDownloadBinaries(new FakeSelectorStorage(_storage), _httpClientHelper,
			_appSettings, _logger, new Zipper(new FakeIWebLogger()));

		var binaryIndex = new BinaryIndex
		{
			FileName = "NOT_FOUND.zip", Sha256 = "dummysha256", Architecture = "linux-x64"
		};
		var baseUrls = new List<Uri> { new("https://qdraw.nl/") };
		var binaryIndexKeyValuePair =
			new KeyValuePair<BinaryIndex?, List<Uri>>(binaryIndex, baseUrls);

		var result = await sut.Download(binaryIndexKeyValuePair, "linux-x64");

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailed, result);
	}


	[TestMethod]
	public async Task Download_InvalidShaHash()
	{
		var sut = new FfMpegDownloadBinaries(new FakeSelectorStorage(_storage), _httpClientHelper,
			_appSettings, _logger, new Zipper(new FakeIWebLogger()));

		var binaryIndex = new BinaryIndex
		{
			FileName = "mock_test.zip", Sha256 = "dummysha256", Architecture = "linux-x64"
		};
		var baseUrls = new List<Uri> { new("https://qdraw.nl/") };
		var binaryIndexKeyValuePair =
			new KeyValuePair<BinaryIndex?, List<Uri>>(binaryIndex, baseUrls);

		var result = await sut.Download(binaryIndexKeyValuePair, "linux-x64");

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedSha256Check, result);
	}

	[TestMethod]
	public async Task Download_DownloadBinariesFailedZipperNotExtracted()
	{
		var sut = new FfMpegDownloadBinaries(new FakeSelectorStorage(_storage), _httpClientHelper,
			_appSettings, _logger, new Zipper(new FakeIWebLogger()));

		var binaryIndex = new BinaryIndex
		{
			FileName = "mock_test.zip",
			Sha256 = "b98fc09ac0df3bbc1ee5e79316604f7462fffdf095c1c676e3c2517773645fe9",
			Architecture = "linux-x64"
		};
		var baseUrls = new List<Uri> { new("https://qdraw.nl/") };
		var binaryIndexKeyValuePair =
			new KeyValuePair<BinaryIndex?, List<Uri>>(binaryIndex, baseUrls);

		var result = await sut.Download(binaryIndexKeyValuePair, "linux-x64");

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedZipperNotExtracted, result);
	}

	[TestMethod]
	public async Task Download_DownloadBinariesFailedZipperNotExtracted11()
	{
		// TODO fix

		var storage = new FakeIStorage();
		var zipper = new FakeIZipper(new List<Tuple<string, byte[]>>
		{
			new("FfMpegDownloadTest/mock_test.zip",
				[.. CreateAnZipFileMacOs.Bytes])
		}, storage);
		var sut = new FfMpegDownloadBinaries(new FakeSelectorStorage(storage), _httpClientHelper,
			_appSettings, _logger, zipper);

		var binaryIndex = new BinaryIndex
		{
			FileName = "mock_test.zip",
			Sha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
			Architecture = "linux-x64"
		};
		var baseUrls = new List<Uri> { new("https://qdraw.nl/") };
		var binaryIndexKeyValuePair =
			new KeyValuePair<BinaryIndex?, List<Uri>>(binaryIndex, baseUrls);

		var result = await sut.Download(binaryIndexKeyValuePair, "linux-x64");

		Assert.AreEqual(FfmpegDownloadStatus.DownloadBinariesFailedZipperNotExtracted, result);
	}
}

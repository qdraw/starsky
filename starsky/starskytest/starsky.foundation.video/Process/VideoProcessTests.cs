using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Services;
using starsky.foundation.storage.Storage;
using starsky.foundation.video.GetDependencies.Models;
using starsky.foundation.video.Process;
using starskytest.FakeCreateAn.CreateAnQuickTimeMp4;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.Process;

[TestClass]
public class VideoProcessTests
{
	private readonly FakeIFfMpegDownload _ffMpegDownload;
	private readonly FakeIStorage _storage;
	private readonly VideoProcess _videoProcess;

	public VideoProcessTests()
	{
		var logger = new FakeIWebLogger();
		_storage = new FakeIStorage(["/"],
			["/test.mp4"],
			new List<byte[]?> { CreateAnQuickTimeMp4.Bytes.ToArray() });
		_ffMpegDownload = new FakeIFfMpegDownload();
		var thumbnailPost = new VideoProcessThumbnailPost(new FakeSelectorStorage(_storage));

		var selectorStorage = new FakeSelectorStorage();
		selectorStorage.SetStorage(_storage);

		_videoProcess = new VideoProcess(selectorStorage, _ffMpegDownload,
			thumbnailPost, logger, new AppSettings { StorageFolder = "/" });
	}

	[TestCleanup]
	public void CleanUpVideoProcessTests()
	{
		FakeIFfMpegDownload.CleanUp();
	}

	[TestMethod]
	public async Task RunVideo_Default_False()
	{
		var result = await _videoProcess.RunVideo("/test",
			"beforeFileHash",
			VideoProcessTypesTests.InvalidEnum());
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task RunVideo_Thumbnail_Success()
	{
		// Arrange
		const string subPath = "/test.mp4";
		var fileHashService = new FileHash(_storage, new FakeIWebLogger());
		var beforeFileHash = ( await fileHashService.GetHashCodeAsync(subPath) ).Key;
		var expectedPath = $"{beforeFileHash}." +
		                   $"{new AppSettings().ThumbnailImageFormat}";

		const VideoProcessTypes type = VideoProcessTypes.Thumbnail;

		_ffMpegDownload.SetDownloadStatus(FfmpegDownloadStatus.Ok);
		await _ffMpegDownload.DownloadFfMpeg();

		// Act
		var result = await _videoProcess.RunVideo(subPath, beforeFileHash, type);

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(SelectorStorage.StorageServices.Temporary, result.ResultPathType);
		Assert.AreEqual(expectedPath, result.ResultPath);
	}

	[TestMethod]
	public async Task RunVideo_Thumbnail_111Success()
	{
		// Arrange
		const string subPath = "/test.xmp";
		var fileHashService = new FileHash(_storage, new FakeIWebLogger());
		var beforeFileHash = ( await fileHashService.GetHashCodeAsync(subPath) ).Key;
		var expectedPath = $"{beforeFileHash}." +
		                   $"{new AppSettings().ThumbnailImageFormat}";

		const VideoProcessTypes type = VideoProcessTypes.Thumbnail;

		await _ffMpegDownload.DownloadFfMpeg();

		// Act
		var result = await _videoProcess.RunVideo(subPath, beforeFileHash, type);

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(SelectorStorage.StorageServices.Temporary, result.ResultPathType);
		Assert.AreEqual(expectedPath, result.ResultPath);
	}

	[TestMethod]
	public async Task RunVideo_Thumbnail_FfmpegDownloadFailed()
	{
		// Arrange
		_ffMpegDownload.SetDownloadStatus(FfmpegDownloadStatus.DownloadBinariesFailed);
		const string subPath = "test.mp4";
		const string beforeFileHash = "hash";
		const VideoProcessTypes type = VideoProcessTypes.Thumbnail;

		// Act
		var result = await _videoProcess.RunVideo(subPath, beforeFileHash, type);

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual("FFMpeg download failed", result.ErrorMessage);
	}

	[DataTestMethod]
	[DataRow("test.mp4", SelectorStorage.StorageServices.Temporary, true)]
	[DataRow("not-found", SelectorStorage.StorageServices.Temporary, false)]
	[DataRow("not-found", SelectorStorage.StorageServices.Thumbnail, false)]
	[DataRow("test.mp4", SelectorStorage.StorageServices.Thumbnail, false)]
	public async Task CleanTemporaryFileTest(string subPath,
		SelectorStorage.StorageServices? resultResultPathType, bool expected)
	{
		// Arrange
		await _storage.WriteStreamAsync(
			new MemoryStream([.. CreateAnQuickTimeMp4.Bytes]),
			"test.mp4");

		// Act
		var result = _videoProcess.CleanTemporaryFile(subPath,
			resultResultPathType);

		// Assert
		Assert.AreEqual(expected, result);
	}
}

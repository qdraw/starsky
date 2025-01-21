using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.video.GetDependencies.Models;
using starsky.foundation.video.Process;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.Process;

[TestClass]
public class VideoProcessTests
{
	private readonly FakeIFfMpegDownload _ffMpegDownload;
	private readonly VideoProcess _videoProcess;

	public VideoProcessTests()
	{
		var logger = new FakeIWebLogger();
		var storage = new FakeIStorage();
		_ffMpegDownload = new FakeIFfMpegDownload();
		var thumbnailPost = new VideoProcessThumbnailPost(new FakeSelectorStorage(storage),
			new AppSettings(), new FakeExifTool(storage,
				new AppSettings()), new FakeIWebLogger(),
			new FakeIThumbnailQuery());

		var selectorStorage = new FakeSelectorStorage();
		selectorStorage.SetStorage(storage);

		_videoProcess = new VideoProcess(selectorStorage, _ffMpegDownload,
			thumbnailPost, logger);
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
			"beforeFileHash", VideoProcessTypesTests.InvalidEnum());
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task RunVideo_Thumbnail_Success()
	{
		// Arrange
		const string subPath = "/test.mp4";
		const string expectedPath = "/test.jpg";
		const string beforeFileHash = "hash";
		const VideoProcessTypes type = VideoProcessTypes.Thumbnail;

		await new FakeIFfMpegDownload().DownloadFfMpeg();

		// Act
		var result = await _videoProcess.RunVideo(subPath, beforeFileHash, type);

		// Assert
		Assert.IsTrue(result.IsSuccess);
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
}

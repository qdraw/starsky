using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace starskytest.starsky.foundation.video;

[TestClass]
public class VideoProcessTests
{
	// [TestMethod]
	// [Timeout(25000)]
	// public async Task VideoProcess_WithNullInput_ReturnsFalse()
	// {
	// 	// Arrange
	// 	var hoststorage = new StorageHostFullPathFilesystem(new FakeIWebLogger());
	// 	var fakeFfmpegDownload = new FfMpegDownload(
	// 		new FakeSelectorStorage(new StorageHostFullPathFilesystem(new FakeIWebLogger())),
	// 		new AppSettings(), new FakeIWebLogger(), new FakeIFfMpegDownloadIndex(),
	// 		new FakeIFfMpegDownloadBinaries(),
	// 		new FakeIFfMpegPrepareBeforeRunning(),
	// 		new FfMpegPreflightRunCheck(new AppSettings(), new FakeIWebLogger()));
	//
	// 	var exiftool = new ExifTool(hoststorage, hoststorage, new AppSettings(),
	// 		new FakeIWebLogger());
	//
	// 	var videoProcess =
	// 		new VideoProcess(new FakeSelectorStorage(hoststorage), fakeFfmpegDownload,
	// 			new VideoProcessThumbnailPost(new FakeSelectorStorage(hoststorage),
	// 				new AppSettings(), exiftool, new FakeIWebLogger(), new FakeIThumbnailQuery()),
	// 			new FakeIWebLogger());
	//
	// 	// Act
	// 	var result = await videoProcess.RunVideo(
	// 		"/Users/dion/data/testcontent/deventer_op_stelten_2014-720p.mp4", "/tmp/test.jpg",
	// 		VideoProcessTypes.Thumbnail);
	//
	// 	// Assert
	// 	Assert.IsTrue(result.IsSuccess);
	// }
}

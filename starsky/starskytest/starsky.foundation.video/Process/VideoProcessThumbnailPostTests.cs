using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MetadataExtractor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.readmeta.ReadMetaHelpers;
using starsky.foundation.video.Process;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.video.Process;

[TestClass]
public class VideoProcessThumbnailPostTests
{
	private readonly VideoProcessThumbnailPost _videoProcessThumbnailPost;
	private readonly FakeIStorage _storage;

	public VideoProcessThumbnailPostTests()
	{
		_storage = new FakeIStorage();

		var selectorStorage = new FakeSelectorStorage(_storage);
		var exifTool = new FakeExifTool(_storage, new AppSettings());
		var logger = new FakeIWebLogger();
		var thumbnailQuery = new FakeIThumbnailQuery();

		var appSettings = new AppSettings();
		_videoProcessThumbnailPost = new VideoProcessThumbnailPost(
			selectorStorage, appSettings, exifTool, logger, thumbnailQuery);
	}

	[DataTestMethod]
	[DataRow("/test.mp4", "/test.jpg")]
	[DataRow("/test/test.mov", "/test/test.jpg")]
	public async Task PostPrepThumbnail_Success_ReturnsVideoResult(string subPath,
		string jpegInFolderSubPath)
	{
		// Arrange
		var runResult = new VideoResult(true, subPath);
		var stream = new MemoryStream([.. CreateAnImageNoExif.Bytes]);

		// Act
		var result = await _videoProcessThumbnailPost.PostPrepThumbnail(runResult,
			stream, jpegInFolderSubPath);

		// Assert
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(jpegInFolderSubPath, result.ResultPath);
		
		Assert.IsTrue(_storage.ExistFile(result.ResultPath ?? ""));
		
		var writtenStream = _storage.ReadStream(result.ResultPath ?? "");
		var meta = ImageMetadataReader.ReadMetadata(writtenStream).ToList();
		await writtenStream.DisposeAsync();

		Assert.AreEqual(3, ReadMetaExif.GetImageWidthHeight(meta, true));
		Assert.AreEqual(2, ReadMetaExif.GetImageWidthHeight(meta, false));
	}

	[DataTestMethod]
	[DataRow("/test.mp4")]
	[DataRow("/test/test.mov")]
	public async Task PostPrepThumbnail_Failure_ReturnsOriginalResult(string subPath)
	{
		// Arrange
		var runResult = new VideoResult(false, subPath);
		var stream = new MemoryStream();

		// Act
		var result = await _videoProcessThumbnailPost.PostPrepThumbnail(runResult, stream, subPath);

		// Assert
		Assert.IsFalse(result.IsSuccess);
		Assert.AreEqual(subPath, result.ResultPath);
	}
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;

[TestClass]
public class FfmpegVideoThumbnailGeneratorTests
{
	private readonly FfmpegVideoThumbnailGenerator _generator;
	private readonly FakeIVideoProcess _videoProcess;

	public FfmpegVideoThumbnailGeneratorTests()
	{
		var selectorStorage = new FakeSelectorStorage();
		_videoProcess = new FakeIVideoProcess(selectorStorage);
		var logger = new FakeIWebLogger();
		_generator = new FfmpegVideoThumbnailGenerator(selectorStorage, _videoProcess, logger);
	}

	[TestMethod]
	public async Task GenerateThumbnail_ReturnsGenerationResultModel()
	{
		// Arrange
		const string singleSubPath = "test.mp4";
		const string fileHash = "hash";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Small, ThumbnailSize.Large };

		_videoProcess.SetSuccessResult();

		// Act
		var result =
			await _generator.GenerateThumbnail(singleSubPath, fileHash, imageFormat,
				thumbnailSizes);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsTrue(result.Any());
	}

	[TestMethod]
	public async Task GenerateThumbnail_NotFound()
	{
		// Arrange
		const string singleSubPath = "test.mp4";
		const string fileHash = "hash";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Small };

		// Act
		var result =
			await _generator.GenerateThumbnail(singleSubPath, fileHash, imageFormat,
				thumbnailSizes);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsTrue(result.All(r => r.IsNotFound));
	}

	[TestMethod]
	public async Task GenerateThumbnail_VideoProcessFails()
	{
		// Arrange
		const string singleSubPath = "/test.mp4";
		const string fileHash = "hash";
		const ThumbnailImageFormat imageFormat = ThumbnailImageFormat.jpg;
		var thumbnailSizes = new List<ThumbnailSize> { ThumbnailSize.Small };

		var selectorStorage =
			new FakeSelectorStorage(new FakeIStorage([], ["/test.mp4"]));
		var videoProcess = new FakeIVideoProcess(selectorStorage);
		var sut = new FfmpegVideoThumbnailGenerator(selectorStorage, videoProcess,
			new FakeIWebLogger());
		
		videoProcess.SetFailureResult("Error");

		// Act
		var result =
			(await sut.GenerateThumbnail(singleSubPath, fileHash, imageFormat,
				thumbnailSizes)).ToList();

		// Assert
		Assert.IsNotNull(result);
		Assert.IsFalse(result.All(r => r.IsNotFound));
		Assert.IsFalse(result.All(r => r.Success));

	}
}

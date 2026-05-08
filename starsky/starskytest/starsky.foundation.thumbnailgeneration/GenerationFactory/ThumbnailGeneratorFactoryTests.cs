using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starsky.foundation.thumbnailgeneration.GenerationFactory.EmbeddedRawThumbnail;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;
using starsky.foundation.thumbnailgeneration.GenerationFactory.RawDng;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory;

[TestClass]
public class ThumbnailGeneratorFactoryTests
{
	private readonly ThumbnailGeneratorFactory _factory;

	public ThumbnailGeneratorFactoryTests()
	{
		var selectorStorageMock = new FakeSelectorStorage();
		var loggerMock = new FakeIWebLogger();
		var videoProcessMock = new FakeIVideoProcess(selectorStorageMock);
		_factory = new ThumbnailGeneratorFactory(selectorStorageMock, loggerMock, videoProcessMock,
			new FakeINativePreviewThumbnailGenerator(),
			new EmbeddedRawThumbnailGenerator(selectorStorageMock,
				new FakeEmbeddedRawThumbnailService(selectorStorageMock), loggerMock));
	}

	[TestMethod]
	[Timeout(5000, CooperativeCancellation = true)]
	[DataRow("test.jpg", typeof(CompositeThumbnailGenerator))]
	[DataRow("test.mp4", typeof(CompositeThumbnailGenerator))]
	[DataRow("test.txt", typeof(NotSupportedFallbackThumbnailGenerator))]
	[DataRow("test.dng", typeof(RawDngThumbnailGenerator))]
	[DataRow("test.arw", typeof(CompositeThumbnailGenerator))]
	[DataRow("test.gif", typeof(CompositeThumbnailGenerator))]
	[DataRow("test.heic", typeof(CompositeThumbnailGenerator))]
	public void GetGenerator_ReturnsCorrectGenerator(string filePath, Type expectedType)
	{
		// Act
		var generator = _factory.GetGenerator(filePath);

		// Assert
		Assert.IsInstanceOfType(generator, expectedType);
	}
}

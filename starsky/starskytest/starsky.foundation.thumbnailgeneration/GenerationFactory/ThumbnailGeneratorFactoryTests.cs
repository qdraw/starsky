using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;
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
		_factory = new ThumbnailGeneratorFactory(selectorStorageMock, loggerMock, videoProcessMock);
	}

	[TestMethod]
	[DataRow("test.jpg", typeof(CompositeThumbnailGenerator))]
	[DataRow("test.mp4", typeof(CompositeThumbnailGenerator))]
	[DataRow("test.txt", typeof(NotSupportedFallbackThumbnailGenerator))]
	public void GetGenerator_ReturnsCorrectGenerator(string filePath, Type expectedType)
	{
		// Act
		var generator = _factory.GetGenerator(filePath);

		// Assert
		Assert.IsInstanceOfType(generator, expectedType);
	}
}

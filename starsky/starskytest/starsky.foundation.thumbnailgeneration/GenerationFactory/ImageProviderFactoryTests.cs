using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;
using starsky.foundation.thumbnailgeneration.GenerationFactory;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Generators;
using starsky.foundation.thumbnailgeneration.GenerationFactory.Skia;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory;

[TestClass]
public class ImageProviderFactoryTests
{
	private readonly ImageProviderFactory _sut = new(
		new FakeSelectorStorage(new FakeIStorage(["/"], ["/test.jpg"],
			new List<byte[]> { System.Array.Empty<byte>() })), new FakeIWebLogger());

	[TestMethod]
	public void CreateThumbnailGenerator_IsSkia()
	{
		Assert.IsInstanceOfType<SkiaThumbnailGenerator>(_sut.CreateThumbnailGenerator());
	}

	[TestMethod]
	public void CreateResizeThumbnailFromSourceImageHelper_IsSkia()
	{
		Assert.IsInstanceOfType<ResizeThumbnailFromSourceImageHelper>(
			_sut.CreateResizeThumbnailFromSourceImageHelper());
	}

	[TestMethod]
	public void CreateResizeThumbnailFromThumbnailImageHelper_IsSkia()
	{
		Assert.IsInstanceOfType<ResizeThumbnailFromThumbnailImageHelper>(
			_sut.CreateResizeThumbnailFromThumbnailImageHelper());
	}

	[TestMethod]
	public void CreateRotateThumbnailHelper_IsSkia()
	{
		Assert.IsInstanceOfType<RotateThumbnailHelper>(
			_sut.CreateRotateThumbnailHelper(new AppSettings()));
	}
}

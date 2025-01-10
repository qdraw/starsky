using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Enums;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.thumbnailgeneration.GenerationFactory.ImageSharp;

[TestClass]
public class ResizeThumbnailFromThumbnailImageHelperTests
{
	[TestMethod]
	public async Task ResizeThumbnailFromThumbnailImage_CorruptInput()
	{
		var storage = new FakeIStorage(
			new List<string> { "/" },
			new List<string> { "test.jpg" },
			new List<byte[]> { Array.Empty<byte>() });

		var sut = new ResizeThumbnailFromThumbnailImageHelper(new FakeSelectorStorage(storage),
			new FakeIWebLogger());

		var result = await sut.ResizeThumbnailFromThumbnailImage("test",
			ThumbnailSize.Large, 1, null, null, true, ThumbnailImageFormat.jpg);

		Assert.IsNull(result.Item1);
		Assert.IsFalse(result.Item2.Success);
		Assert.IsFalse(result.Item2.IsNotFound);
		Assert.AreEqual("Image cannot be loaded", result.Item2.ErrorMessage);
	}
}

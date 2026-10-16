using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

/// <summary>
///     The entry point that delegates to the image library chosen by the factory
/// </summary>
[TestClass]
public sealed class OverlayImageTest
{
	private readonly AppSettings _appSettings = new();

	private static FakeIStorage Storage()
	{
		return new FakeIStorage(["/"], ["/test.jpg", "/test.webp"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });
	}

	private OverlayImage Overlay(FakeIStorage storage)
	{
		return new OverlayImage(
			new OverlayImageFactory(new FakeSelectorStorage(storage), _appSettings));
	}

	[TestMethod]
	public void FilePathOverlayImage_OutputParentFullFilePathFolder_IsDelegated()
	{
		var path = Overlay(Storage()).FilePathOverlayImage("/output", "TesT.Jpg",
			new AppSettingsPublishProfiles { Append = "_a" });
		Assert.AreEqual("/output/test_a.jpg", path);
	}

	[TestMethod]
	public async Task ResizeOverlayImageLarge_IsDelegated()
	{
		var storage = Storage();
		var written = await Overlay(storage).ResizeOverlayImageLarge("/test.jpg", "/out_large.jpg",
			new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100, OverlayMaxWidth = 1, Path = "/test.jpg"
			});

		Assert.IsTrue(written);
		Assert.IsTrue(storage.ExistFile("/out_large.jpg"));
	}

	[TestMethod]
	public async Task ResizeOverlayImageThumbnails_IsDelegated()
	{
		var storage = Storage();
		var written = await Overlay(storage).ResizeOverlayImageThumbnails("/test",
			ThumbnailSize.Large, "/out_thumb.jpg",
			new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100, OverlayMaxWidth = 1, Path = "/test.jpg"
			});

		Assert.IsTrue(written);
		Assert.IsTrue(storage.ExistFile("/out_thumb.jpg"));
	}
}

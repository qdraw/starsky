using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Thumbnails;
using starsky.foundation.storage.Interfaces;
using starsky.foundation.storage.Storage;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services;

[TestClass]
public sealed class OverlayImageTest
{
	private readonly AppSettings _appSettings;
	private readonly ISelectorStorage _selectorStorage;
	private readonly FakeIStorage _storage;

	public OverlayImageTest()
	{
		_storage = new FakeIStorage(["/"],
			["/test.jpg", "/test.webp"],
			new List<byte[]> { CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray() });
		_selectorStorage = new FakeSelectorStorage(_storage);
		_appSettings = new AppSettings();
	}

	[TestMethod]
	public void FilePathOverlayImage_Case()
	{
		var image =
			new OverlayImage(_selectorStorage, _appSettings).FilePathOverlayImage("TesT.Jpg",
				new AppSettingsPublishProfiles());
		Assert.AreEqual("test.jpg", image);
	}

	[TestMethod]
	public void FilePathOverlayImage_Append()
	{
		var image =
			new OverlayImage(_selectorStorage, _appSettings).FilePathOverlayImage("Img.Jpg",
				new AppSettingsPublishProfiles { Append = "_test" });
		Assert.AreEqual("img_test.jpg", image);
	}

	[TestMethod]
	public void FilePathOverlayImage_outputParentFullFilePathFolder()
	{
		var image =
			new OverlayImage(_selectorStorage, _appSettings).FilePathOverlayImage(
				string.Empty, "TesT.Jpg",
				new AppSettingsPublishProfiles());
		Assert.AreEqual(PathHelper.AddBackslash(string.Empty) + "test.jpg", image);
	}

	[TestMethod]
	public async Task ResizeOverlayImageThumbnails_null()
	{
		var overlayImage = new OverlayImage(_selectorStorage, _appSettings);
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
			await overlayImage.ResizeOverlayImageThumbnails(null!, ThumbnailSize.Large, null!,
				new AppSettingsPublishProfiles()));
	}

	[TestMethod]
	public async Task ResizeOverlayImageLarge_null_exception()
	{
		var overlayImage = new OverlayImage(_selectorStorage, _appSettings);
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
			await overlayImage.ResizeOverlayImageLarge(null!, null!,
				new AppSettingsPublishProfiles()));
	}

	[TestMethod]
	public async Task ResizeOverlayImageThumbnails_itemFileHash_Not_Found()
	{
		var overlayImage = new OverlayImage(_selectorStorage, _appSettings);
		await Assert.ThrowsExactlyAsync<FileNotFoundException>(async () =>
			await overlayImage.ResizeOverlayImageThumbnails("non-exist.jpg", ThumbnailSize.Large,
				"/out.jpg",
				new AppSettingsPublishProfiles { SourceMaxWidth = 100, OverlayMaxWidth = 1 }));
	}

	[TestMethod]
	public async Task ResizeOverlayImageThumbnails_overlay_image_missing()
	{
		var overlayImage = new OverlayImage(_selectorStorage, _appSettings);
		await Assert.ThrowsExactlyAsync<FileNotFoundException>(async () =>
			await overlayImage.ResizeOverlayImageThumbnails("test.jpg", ThumbnailSize.Large,
				"/out.jpg",
				new AppSettingsPublishProfiles { SourceMaxWidth = 100, OverlayMaxWidth = 1 }));
	}

	[TestMethod]
	public async Task ResizeOverlayImageLarge_File_Not_Found()
	{
		var overlayImage = new OverlayImage(_selectorStorage, _appSettings);
		await Assert.ThrowsExactlyAsync<FileNotFoundException>(async () =>
			await overlayImage.ResizeOverlayImageLarge("non-exist.jpg", "/out.jpg",
				new AppSettingsPublishProfiles { SourceMaxWidth = 100, OverlayMaxWidth = 1 }));
	}

	[TestMethod]
	public async Task ResizeOverlayImageLarge_overlay_image_missing()
	{
		var overlayImage = new OverlayImage(_selectorStorage, _appSettings);
		await Assert.ThrowsExactlyAsync<FileNotFoundException>(async () =>
			await overlayImage.ResizeOverlayImageLarge("/test.jpg", "/out.jpg",
				new AppSettingsPublishProfiles { SourceMaxWidth = 100, OverlayMaxWidth = 1 }));
	}

	[TestMethod]
	public async Task ResizeOverlayImageLarge_Ignore_If_Exist()
	{
		var overlayImage =
			new OverlayImage(_selectorStorage, _appSettings);

		await overlayImage.ResizeOverlayImageLarge("/test.jpg", "/test.jpg",
			new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100, OverlayMaxWidth = 1, Path = "/test.jpg"
			});

		// Should return nothing
		Assert.IsTrue(_storage.ExistFile("/test.jpg"));
	}

	[TestMethod]
	public void ResizeOverlayImageThumbnails_Ignore_If_Exist()
	{
		var overlayImage =
			new OverlayImage(_selectorStorage, _appSettings);

		overlayImage.ResizeOverlayImageThumbnails("/test" /* no extension */, ThumbnailSize.Large,
			"/test.jpg",
			new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100, OverlayMaxWidth = 1, Path = "/test.jpg"
			});

		// Should return nothing
		Assert.IsTrue(_storage.ExistFile("/test.jpg"));
	}

	[TestMethod]
	public async Task ResizeOverlayImageLarge_Done()
	{
		var overlayImage =
			new OverlayImage(_selectorStorage, _appSettings);

		await overlayImage.ResizeOverlayImageLarge("/test.jpg", "/out_large.jpg",
			new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100, OverlayMaxWidth = 1, Path = "/test.jpg"
			});

		Assert.IsTrue(_storage.ExistFile("/out_large.jpg"));
	}

	[TestMethod]
	public async Task ResizeOverlayImageThumbnails_Done()
	{
		var overlayImage =
			new OverlayImage(_selectorStorage, _appSettings);

		await overlayImage.ResizeOverlayImageThumbnails("/test" /* no extension */,
			ThumbnailSize.Large, "/out_thumb.jpg",
			new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100, OverlayMaxWidth = 1, Path = "/test.jpg"
			});

		Assert.IsTrue(_storage.ExistFile("/out_thumb.jpg"));
	}

	[TestMethod]
	public async Task ResizeOverlayImageThumbnails_ExtraLarge_Not_Found()
	{
		// ExtraLarge thumbnail name is hash@2000.ext — should NOT be confused with hash.ext
		var overlayImage = new OverlayImage(_selectorStorage, _appSettings);
		await Assert.ThrowsExactlyAsync<FileNotFoundException>(async () =>
			await overlayImage.ResizeOverlayImageThumbnails("/test", ThumbnailSize.ExtraLarge,
				"/out_xl.jpg",
				new AppSettingsPublishProfiles { SourceMaxWidth = 2000, OverlayMaxWidth = 1 }));
	}

	[TestMethod]
	public async Task ResizeOverlayImageThumbnails_ExtraLarge_Done()
	{
		// Regression: passing the already-combined name caused a double .webp.webp extension.
		// The method must receive a bare hash and the ThumbnailSize; it builds the name itself.
		var extraLargeName = ThumbnailNameHelper.Combine("/test", ThumbnailSize.ExtraLarge,
			_appSettings.ThumbnailImageFormat);
		var storage = new FakeIStorage(["/"],
			[extraLargeName, "/test.jpg"],
			new List<byte[]>
			{
				CreateAnImage.Bytes.ToArray(), CreateAnImage.Bytes.ToArray()
			});
		var selectorStorage = new FakeSelectorStorage(storage);
		var overlayImage = new OverlayImage(selectorStorage, _appSettings);

		await overlayImage.ResizeOverlayImageThumbnails("/test", ThumbnailSize.ExtraLarge,
			"/out_xl.jpg",
			new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 2000, OverlayMaxWidth = 1, Path = "/test.jpg"
			});

		Assert.IsTrue(storage.ExistFile("/out_xl.jpg"));
	}
}

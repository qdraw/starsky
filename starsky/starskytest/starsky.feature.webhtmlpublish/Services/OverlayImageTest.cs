using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.webhtmlpublish.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Interfaces;
using starskytest.FakeCreateAn;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.webhtmlpublish.Services
{
	[TestClass]
	public class OverlayImageTest
	{
		private readonly IStorage _storage;
		private readonly ISelectorStorage _selectorStorage;

		public OverlayImageTest()
		{
			_storage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},new List<byte[]>{CreateAnImage.Bytes});
			_selectorStorage = new FakeSelectorStorage(_storage);
		}
		
		[TestMethod]
		public void FilePathOverlayImage_Case()
		{
			var image =
				new OverlayImage(_selectorStorage, new AppSettings()).FilePathOverlayImage("TesT.Jpg",
					new AppSettingsPublishProfiles());
			Assert.AreEqual("test.jpg",image);
		}
		
		[TestMethod]
		public void FilePathOverlayImage_Append()
		{
			var image =
				new OverlayImage(_selectorStorage, new AppSettings()).FilePathOverlayImage("Img.Jpg",
					new AppSettingsPublishProfiles{Append = "_test"});
			Assert.AreEqual("img_test.jpg",image);
		}
		
		[TestMethod]
		public void FilePathOverlayImage_outputParentFullFilePathFolder()
		{
			var image =
				new OverlayImage(_selectorStorage, new AppSettings()).FilePathOverlayImage(string.Empty,"TesT.Jpg",
					new AppSettingsPublishProfiles());
			Assert.AreEqual(PathHelper.AddBackslash(string.Empty) + "test.jpg",image);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ResizeOverlayImageThumbnails_null()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings());
			
			overlayImage.ResizeOverlayImageThumbnails(null,null, new AppSettingsPublishProfiles());
			// > ArgumentNullException
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ResizeOverlayImageLarge_null_exception()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings());
			
			overlayImage.ResizeOverlayImageLarge(null,null, new AppSettingsPublishProfiles());
			// > ArgumentNullException
		}

		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public void ResizeOverlayImageThumbnails_overlay_image_missing()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings{ ThumbnailTempFolder = "/"});
			
			overlayImage.ResizeOverlayImageThumbnails("test.jpg", "/out.jpg", new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100,
				OverlayMaxWidth = 1
			});
			// > overlay image missing
		}
		
		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public void ResizeOverlayImageLarge_overlay_image_missing()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings{ ThumbnailTempFolder = "/"});
			
			overlayImage.ResizeOverlayImageLarge("test.jpg", "/out.jpg", new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100,
				OverlayMaxWidth = 1
			});
			// > overlay image missing
		}
	}
}

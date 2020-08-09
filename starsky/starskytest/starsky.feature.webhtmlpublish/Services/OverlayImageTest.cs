using System;
using System.Collections.Generic;
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
			_selectorStorage = new FakeSelectorStorage();
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
	}
}

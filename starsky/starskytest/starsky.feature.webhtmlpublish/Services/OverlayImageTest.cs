using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
	public sealed class OverlayImageTest
	{
		private readonly IStorage _storage;
		private readonly ISelectorStorage _selectorStorage;

		public OverlayImageTest()
		{
			_storage = new FakeIStorage(new List<string>{"/"},
				new List<string>{"/test.jpg"},new List<byte[]>{CreateAnImage.Bytes.ToArray()});
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
				new OverlayImage(_selectorStorage, new AppSettings()).FilePathOverlayImage(
					string.Empty,"TesT.Jpg",
					new AppSettingsPublishProfiles());
			Assert.AreEqual(PathHelper.AddBackslash(string.Empty) + "test.jpg",image);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ResizeOverlayImageThumbnails_null()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings());
			
			await overlayImage.ResizeOverlayImageThumbnails(null,null, new AppSettingsPublishProfiles());
			// > ArgumentNullException
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task ResizeOverlayImageLarge_null_exception()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings());
			
			await overlayImage.ResizeOverlayImageLarge(null,null, new AppSettingsPublishProfiles());
			// > ArgumentNullException
		}
		
		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public async Task ResizeOverlayImageThumbnails_itemFileHash_Not_Found()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings{ ThumbnailTempFolder = "/"});
			
			await overlayImage.ResizeOverlayImageThumbnails("non-exist.jpg", "/out.jpg", new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100,
				OverlayMaxWidth = 1
			});
			// itemFileHash not found
		}

		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public async Task ResizeOverlayImageThumbnails_overlay_image_missing()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings{ ThumbnailTempFolder = "/"});
			
			await overlayImage.ResizeOverlayImageThumbnails("test.jpg", "/out.jpg", new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100,
				OverlayMaxWidth = 1
			});
			// > overlay image missing
		}
		
		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public async Task ResizeOverlayImageLarge_File_Not_Found()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings{ ThumbnailTempFolder = "/"});
			
			await overlayImage.ResizeOverlayImageLarge("non-exist.jpg", 
				"/out.jpg", new AppSettingsPublishProfiles
				{
					SourceMaxWidth = 100,
					OverlayMaxWidth = 1
				});
			// itemFileHash not found
		}
		
		[TestMethod]
		[ExpectedException(typeof(FileNotFoundException))]
		public async Task ResizeOverlayImageLarge_overlay_image_missing()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings{ ThumbnailTempFolder = "/"});
			
			await overlayImage.ResizeOverlayImageLarge("/test.jpg", "/out.jpg", new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100,
				OverlayMaxWidth = 1
			});
			// > overlay image missing
		}
		
		[TestMethod]
		public async Task ResizeOverlayImageLarge_Ignore_If_Exist()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings{ ThumbnailTempFolder = "/"});
			
			await overlayImage.ResizeOverlayImageLarge("/test.jpg", "/test.jpg", new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100,
				OverlayMaxWidth = 1,
				Path = "/test.jpg"
			});
			
			// Should return nothing
			Assert.IsTrue(_storage.ExistFile("/test.jpg"));
		}
		
		[TestMethod]
		public void ResizeOverlayImageThumbnails_Ignore_If_Exist()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings{ ThumbnailTempFolder = "/"});
			
			overlayImage.ResizeOverlayImageThumbnails("/test.jpg", "/test.jpg", new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100,
				OverlayMaxWidth = 1,
				Path = "/test.jpg"
			});
			
			// Should return nothing
			Assert.IsTrue(_storage.ExistFile("/test.jpg"));
		}
		
		[TestMethod]
		public async Task ResizeOverlayImageLarge_Done()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings{ ThumbnailTempFolder = "/"});
			
			await overlayImage.ResizeOverlayImageLarge("/test.jpg", "/out_large.jpg", new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100,
				OverlayMaxWidth = 1,
				Path = "/test.jpg"
			});
			
			Assert.IsTrue(_storage.ExistFile("/out_large.jpg"));
		}
		
		[TestMethod]
		public async Task ResizeOverlayImageThumbnails_Done()
		{
			var overlayImage =
				new OverlayImage(_selectorStorage, new AppSettings{ ThumbnailTempFolder = "/"});
			
			await overlayImage.ResizeOverlayImageThumbnails("/test.jpg", "/out_thumb.jpg", new AppSettingsPublishProfiles
			{
				SourceMaxWidth = 100,
				OverlayMaxWidth = 1,
				Path = "/test.jpg"
			});
			
			Assert.IsTrue(_storage.ExistFile("/out_thumb.jpg"));
		}
	}
}

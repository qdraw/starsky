using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models;
using starsky.foundation.metathumbnail.Services;
using starskytest.FakeCreateAn;
using starskytest.FakeCreateAn.CreateAnImageWithThumbnail;
using starskytest.FakeMocks;


namespace starskytest.starsky.foundation.readmeta.Services
{
	[TestClass]
	public class OffsetDataMetaExifThumbnailTest
	{
		[TestMethod]
		public void ParseMetaThumbnail_Null()
		{
			var (exifThumbnailDirectory,width,height,rotation) = new OffsetDataMetaExifThumbnail(new FakeSelectorStorage(),
				new FakeIWebLogger()).ParseMetaThumbnail(null, null);
			Assert.AreEqual(null, exifThumbnailDirectory);
			Assert.AreEqual(0, width);
			Assert.AreEqual(0, height);
			Assert.AreEqual(rotation, FileIndexItem.Rotation.DoNotChange);
		}
		
		[TestMethod]
		public void ParseMetaThumbnail_ParseMetaThumbnail_NoValidSize_DueWrongFormat_Png()
		{
			var storage = new FakeIStorage(
				new List<string>{"/"}, 
				new List<string>{"/test.png","/test.jpg"},
				new List<byte[]>{CreateAnPng.Bytes, new CreateAnImageWithThumbnail().Bytes});
			
			var (allExifItems,  _) = new OffsetDataMetaExifThumbnail(new FakeSelectorStorage(storage),
				new FakeIWebLogger()).ReadExifMetaDirectories("/test.png");
			var (_,  thumbnailDirectory) = new OffsetDataMetaExifThumbnail(new FakeSelectorStorage(storage),
				new FakeIWebLogger()).ReadExifMetaDirectories("/test.jpg");
			
			// Switch around to get a situation where the image has no size but an valid thumbnail
			var (exifThumbnailDirectory,width,height,rotation) = new OffsetDataMetaExifThumbnail(new FakeSelectorStorage(storage),
				new FakeIWebLogger()).ParseMetaThumbnail(allExifItems, thumbnailDirectory);
			
			Assert.AreEqual(thumbnailDirectory,exifThumbnailDirectory);
			Assert.AreEqual(0, width);
			Assert.AreEqual(0, height);
			Assert.AreEqual(rotation, FileIndexItem.Rotation.DoNotChange);
		}
	}
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.metathumbnail.Helpers;

namespace starskytest.starsky.foundation.thumbnailmeta.Helpers
{
	[TestClass]
	public class NewImageSizeTest
	{
	
		[TestMethod]
		public void NewImageSize_Horizontal()
		{
			var sourceWidth = 4240;
			var sourceHeight = 2832;

			var smallWidth = 160;
			var smallHeight = 120;

			var rNewImageSizeCalc = NewImageSize.NewImageSizeCalc(smallWidth, 
				smallHeight, sourceWidth, sourceHeight);
			
			Assert.AreEqual(106, rNewImageSizeCalc.DestHeight);
			Assert.AreEqual(160, rNewImageSizeCalc.DestWidth);
			
			Assert.AreEqual(0, rNewImageSizeCalc.DestX);
			Assert.AreEqual(7, rNewImageSizeCalc.DestY);
		}
		
		[TestMethod]
		public void NewImageSize_Portrait()
		{
			var sourceWidth = 2832 ;
			var sourceHeight = 4240;

			var smallWidth = 120 ;
			var smallHeight = 160;

			var rNewImageSizeCalc = NewImageSize.NewImageSizeCalc(smallWidth, 
				smallHeight, sourceWidth, sourceHeight);
			
			Assert.AreEqual(160 , rNewImageSizeCalc.DestHeight);
			Assert.AreEqual(106, rNewImageSizeCalc.DestWidth);
			
			Assert.AreEqual(7, rNewImageSizeCalc.DestX);
			Assert.AreEqual(0, rNewImageSizeCalc.DestY);
		}
	}
}

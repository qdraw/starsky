using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.metathumbnail.Helpers;

namespace starskytest.starsky.foundation.thumbnailmeta.Helpers
{
	[TestClass]
	public class NewImageSizeTest
	{
	
		[TestMethod]
		public void Test1()
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
	}
}

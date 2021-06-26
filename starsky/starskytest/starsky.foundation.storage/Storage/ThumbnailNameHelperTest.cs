using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Storage;

namespace starskytest.starsky.foundation.storage.Storage
{
	[TestClass]
	public class ThumbnailNameHelperTest
	{
		[TestMethod]
		public void GetSize_TinyMeta_Enum()
		{
			var result = ThumbnailNameHelper.GetSize(ThumbnailSize.TinyMeta);
			Assert.AreEqual(150, result);
		}

		[TestMethod]
		public void GetSize_TinyMeta_Int()
		{
			var result =ThumbnailNameHelper.GetSize(150);
			Assert.AreEqual(ThumbnailSize.TinyMeta, result);
		}

		[TestMethod]
		public void GetSize_Small_Int()
		{
			var result =ThumbnailNameHelper.GetSize(300);
			Assert.AreEqual(ThumbnailSize.Small, result);
		}
		
		[TestMethod]
		public void GetSize_Large_Int()
		{
			var result =ThumbnailNameHelper.GetSize(1000);
			Assert.AreEqual(ThumbnailSize.Large, result);
		}
				
		[TestMethod]
		public void GetSize_ExtraLarge_Int()
		{
			var result = ThumbnailNameHelper.GetSize(2000);
			Assert.AreEqual(ThumbnailSize.ExtraLarge, result);
		}

		[TestMethod]
		public void Combine_Compare()
		{
			var result = ThumbnailNameHelper.Combine("test_hash",2000);
			var result2 = ThumbnailNameHelper.Combine("test_hash",ThumbnailSize.ExtraLarge);

			Assert.AreEqual(result,result2);
		}
	}
}

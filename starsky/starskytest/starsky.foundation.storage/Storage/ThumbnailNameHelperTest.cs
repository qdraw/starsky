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
		
		[TestMethod]
		public void GetSize_Name_ExtraLarge()
		{
			var input = ThumbnailNameHelper.Combine("01234567890123456789123456",2000);
			var result2 = ThumbnailNameHelper.GetSize(input);
			Assert.AreEqual(ThumbnailSize.ExtraLarge, result2); 
		}
		
		[TestMethod]
		public void GetSize_Name_Large()
		{
			var input = ThumbnailNameHelper.Combine("01234567890123456789123456",ThumbnailSize.Large);
			var result2 = ThumbnailNameHelper.GetSize(input);
			Assert.AreEqual(ThumbnailSize.Large, result2);
		}
		
		[TestMethod]
		public void GetSize_Name_NonValidLength()
		{
			var input = "01234567890123456789123456@859693845";
			var result2 = ThumbnailNameHelper.GetSize(input);
			Assert.AreEqual(ThumbnailSize.Unknown, result2);
		}
		
		[TestMethod]
		public void GetSize_Name_Large_NonValidLength()
		{
			var input = ThumbnailNameHelper.Combine("non_valid_length",ThumbnailSize.Large);
			var result2 = ThumbnailNameHelper.GetSize(input);
			Assert.AreEqual(ThumbnailSize.Unknown, result2);
		}
		
		[TestMethod]
		public void GetSize_Name_UnknownSize()
		{
			var input = "test_hash@4789358";
			var result2 = ThumbnailNameHelper.GetSize(input);
			Assert.AreEqual(ThumbnailSize.Unknown, result2);
		}
	}
}

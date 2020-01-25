using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;

namespace starskytest.Helpers
{
	[TestClass]
	public class FilenameHelpersTest
	{
		[TestMethod]
		public void FilenamesHelper_ValidFileName()
		{
			var result = FilenamesHelper.IsValidFileName("test.jpg");
			Assert.AreEqual(true, result);
		}
				
		[TestMethod]
		public void FilenamesHelper_ValidFileName_StartWithUnderscore()
		{
			var result = FilenamesHelper.IsValidFileName("_.com");
			Assert.AreEqual(true, result);
		}
		
		[TestMethod]
		public void FilenamesHelper_NonValidFileName()
		{
			var result = FilenamesHelper.IsValidFileName(".jpg");
			Assert.AreEqual(false, result);
		}
		
		[TestMethod]
		public void FilenamesHelper_GetFilePath()
		{
			var result = FilenamesHelper.GetFileName("sdfsdf/test.jpg");
			Assert.AreEqual("test.jpg", result);
		}
				
		
		[TestMethod]
		public void FilenamesHelper_GetParentPath()
		{
			var result = FilenamesHelper.GetParentPath("/yes.jpg");
			Assert.AreEqual("/", result);
		}
		
		[TestMethod]
		public void FilenamesHelper_GetParentPathSubDir()
		{
			var result = FilenamesHelper.GetParentPath("/sub/yes.jpg");
			Assert.AreEqual("/sub/", result);
		}



	}
	
}

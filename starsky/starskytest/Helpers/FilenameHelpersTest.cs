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
			var result = new FilenamesHelper().IsValidFileName("test.jpg");
			Assert.AreEqual(true, result);
		}
				
		[TestMethod]
		public void FilenamesHelper_ValidFileName_StartWithUnderscore()
		{
			var result = new FilenamesHelper().IsValidFileName("_.com");
			Assert.AreEqual(true, result);
		}
		
		[TestMethod]
		public void FilenamesHelper_NonValidFileName()
		{
			var result = new FilenamesHelper().IsValidFileName(".jpg");
			Assert.AreEqual(false, result);
		}
		
		[TestMethod]
		public void FilenamesHelper_GetFilePath()
		{
			var result = new FilenamesHelper().GetFileName("sdfsdf/test.jpg");
			Assert.AreEqual("test.jpg", result);
		}
				

	}
	
}

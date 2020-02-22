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

		[TestMethod]
		public void FilenamesHelper_GetFileNameWithoutExtension()
		{
			var result = FilenamesHelper.GetFileNameWithoutExtension("/te_st/test.com");
			Assert.AreEqual("test", result);
		}
		
		[TestMethod]
		public void FilenamesHelper_GetFileNameWithoutExtension_example2()
		{
			var result = FilenamesHelper.GetFileNameWithoutExtension("http://path/Lists/Test/Attachments/1/Document Test.docx");
			Assert.AreEqual("Document Test", result);
		}

		[TestMethod]
		public void FilenamesHelper_GetFileNameWithoutExtension_example3()
		{
			var result = FilenamesHelper.GetFileNameWithoutExtension("/0000000000aaaaa__exifreadingtest00.jpg");
			Assert.AreEqual("0000000000aaaaa__exifreadingtest00", result);
		}
		
		[TestMethod]
		public void FilenamesHelper_GetFileNameWithoutExtension_example4()
		{
			var result = FilenamesHelper.GetFileNameWithoutExtension("/0000000000aaaaa__exifreadingtest00");
			Assert.AreEqual("0000000000aaaaa__exifreadingtest00", result);
		}
		
	}
	
}

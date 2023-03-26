using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;

namespace starskytest.Helpers
{
	[TestClass]
	public sealed class MimeHelperTest
	{
		[TestMethod]
		public void GetMimeTypeByFileNameTest()
		{
			Assert.AreEqual("application/octet-stream",MimeHelper.GetMimeTypeByFileName("test.unknown"));
			Assert.AreEqual("image/jpeg",MimeHelper.GetMimeTypeByFileName("test.jpg"));
		}
		
		[TestMethod]
		public void GetMimeTypeByExtensionTest_NoExtension()
		{
			Assert.AreEqual("application/octet-stream",MimeHelper.GetMimeTypeByFileName(string.Empty));
		}
	}
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.project.web.Helpers;

namespace starskytest.starsky.project.web.Helpers
{
	[TestClass]
	public sealed class MimeHelperTest
	{
		[TestMethod]
		public void GetMimeTypeByFileNameTestUnknown()
		{
			Assert.AreEqual("application/octet-stream",
				MimeHelper.GetMimeTypeByFileName("test.unknown"));
		}

		[TestMethod]
		public void GetMimeTypeByFileNameTestJpg()
		{
			Assert.AreEqual("image/jpeg", MimeHelper.GetMimeTypeByFileName("test.jpg"));
		}

		[TestMethod]
		public void GetMimeTypeByFileNameTestJpeg()
		{
			Assert.AreEqual("image/jpeg", MimeHelper.GetMimeTypeByFileName("test.jpeg"));
		}

		[TestMethod]
		public void GetMimeTypeByExtensionTest_NoExtension()
		{
			Assert.AreEqual("application/octet-stream",
				MimeHelper.GetMimeTypeByFileName(string.Empty));
		}

		[TestMethod]
		public void GetMimeType_NoExtension()
		{
			Assert.AreEqual("application/octet-stream", MimeHelper.GetMimeType(string.Empty));
		}


		[TestMethod]
		public void GetMimeType_Jpeg()
		{
			Assert.AreEqual("image/jpeg", MimeHelper.GetMimeType("jpg"));
		}
	}
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Helpers;

namespace starskytest.Helpers
{
    [TestClass]
    public class MimeHelperTest
    {
        [TestMethod]
        public void GetMimeTypeByFileNameTest()
        {
            Assert.AreEqual("unknown/unknown",MimeHelper.GetMimeTypeByFileName("test.unknown"));
            Assert.AreEqual("image/jpeg",MimeHelper.GetMimeTypeByFileName("test.jpg"));
        }
    }
}
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;

namespace starskytests.Helpers
{
    [TestClass]
    public class Base64HelperTest
    {
        [TestMethod]
        public void Base64HelperTest_ToBase64()
        {
            var base64 = Base64Helper.ToBase64(new MemoryStream());
            Assert.AreEqual(string.Empty,base64);
        }
            
    }
}
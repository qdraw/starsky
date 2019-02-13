using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Middleware;

namespace starskytests.Middleware
{
    [TestClass]
    public class MiddlewareBasicAuthenticationHeaderValueTest
    {
        [TestMethod]
        public void MiddlewareBasicAuthenticationHeaderValueCtorTest()
        {
            var bto = new BasicAuthenticationHeaderValue("Short");
            Assert.AreEqual(false,bto.IsValidBasicAuthenticationHeaderValue);
        }
        
        [TestMethod]
        public void MiddlewareBasicAuthenticationHeaderValueCtor_WithUser_Test_Test()
        {
            // dGVzdDp0ZXN0 == user: test pass: test
            var bto = new BasicAuthenticationHeaderValue("Basic dGVzdDp0ZXN0");
            Assert.AreEqual(true,bto.IsValidBasicAuthenticationHeaderValue);
        }
        
        [TestMethod]
        public void MiddlewareBasicAuthenticationHeaderValueCtor_ReadUserPassword_Test()
        {
            // dGVzdDp0ZXN0 == user: test pass: test
            var bto = new BasicAuthenticationHeaderValue("Basic dGVzdDp0ZXN0");
            Assert.AreEqual("test",bto.UserIdentifier);
            Assert.AreEqual("test",bto.UserPassword);
        }  
        
        [TestMethod]
        public void MiddlewareBasicAuthenticationHeaderValueCtor_FormatException_Test()
        {
            // Not a valid imput :(
            var bto = new BasicAuthenticationHeaderValue("00000000000");
            Assert.AreEqual(null,bto.UserIdentifier);
        }  
        
    }
}
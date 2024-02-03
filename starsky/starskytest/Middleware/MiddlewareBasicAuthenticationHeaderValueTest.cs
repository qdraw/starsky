using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Middleware;

namespace starskytest.Middleware
{
	[TestClass]
	public sealed class MiddlewareBasicAuthenticationHeaderValueTest
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
			// Not a valid input :(
			var bto = new BasicAuthenticationHeaderValue("00000000000");
			Assert.AreEqual(string.Empty,bto.UserIdentifier);
		}  
        
	}
}

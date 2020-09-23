using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Middleware;
using starskycore.Middleware;

namespace starskytest.starsky.foundation.platform.Middleware
{
	[TestClass]
	public class ReverseProxyHttpsEnforcerTest
	{
		[TestMethod]
		public async Task ContentSecurityPolicyMiddlewareTest_invoke_testContent()
		{
			// Arrange
			var httpContext = new DefaultHttpContext();
			var httpsEnforcerMiddleware = new ReverseProxyHttpsEnforcer(next: (innerHttpContext) => Task.FromResult(0));

			// Act
			await httpsEnforcerMiddleware.Invoke(httpContext);
			
			//test
			var forwardedProto = httpContext.Response.Headers["X-Forwarded-Proto"].ToString();
			// Assert.AreEqual(true,forwardedProto.Contains("default-src"));
		}
	}
}

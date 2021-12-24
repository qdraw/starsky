using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Extensions;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Middleware;

namespace starskytest.starsky.foundation.platform.Middleware
{
	[TestClass]
	public class ContentSecurityPolicyMiddlewareTest
	{
		[TestMethod]
		public async Task MiddlewareExtensionsTest_CSPBasicSetupTest()
		{
			var host = WebHost.CreateDefaultBuilder()
				.UseUrls("http://localhost:5051")
				.Configure(app =>
				{
					app.UseContentSecurityPolicy();
					app.UseBasicAuthentication();
					app.UseNoAccountLocalhost(false);
					app.UseNoAccountLocalhost(true);
					app.UseCheckIfAccountExist();
				}).Build();

			await host.StartAsync();
			await host.StopAsync();
			Assert.IsNotNull(host);
		}

		[TestMethod]
		public async Task ContentSecurityPolicyMiddlewareTest_invoke_testContent()
		{
			// Arrange
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Scheme = "http";
			var authMiddleware = new ContentSecurityPolicyMiddleware(next: (innerHttpContext) => Task.FromResult(0));

			// Act
			await authMiddleware.Invoke(httpContext);
			//test
			var csp = httpContext.Response.Headers["Content-Security-Policy"].ToString();
			Assert.AreEqual(true,csp.Contains("default-src"));
			Assert.AreEqual(true,csp.Contains("ws://"));
		}
		
		[TestMethod]
		public async Task invoke_httpsTest_websockets()
		{
			// Arrange
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Scheme = "https";
			
			var authMiddleware = new ContentSecurityPolicyMiddleware(next: (innerHttpContext) => Task.FromResult(0));

			// Act
			await authMiddleware.Invoke(httpContext);
			//test
			var csp = httpContext.Response.Headers["Content-Security-Policy"].ToString();
			
			Assert.AreEqual(true,csp.Contains("default-src"));
			Assert.AreEqual(true,csp.Contains("wss://"));
		}
		
		[TestMethod]
		public async Task ContentSecurityPolicyMiddlewareTest_invoke_otherTypes()
		{
			// Arrange
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Scheme = "http";
			var authMiddleware = new ContentSecurityPolicyMiddleware((innerHttpContext) => Task.FromResult(0));

			// Act
			await authMiddleware.Invoke(httpContext);
			
			// test
			var referrerPolicy = httpContext.Response.Headers["Referrer-Policy"].ToString();
			Assert.AreEqual( "no-referrer",referrerPolicy);
			
			var frameOptions = httpContext.Response.Headers["X-Frame-Options"].ToString();
			Assert.AreEqual( "DENY",frameOptions);
			
			// X-Xss-Protection
			var xssProtection = httpContext.Response.Headers["X-Xss-Protection"].ToString();
			Assert.AreEqual( "1; mode=block",xssProtection);

			// X-Content-Type-Options
			var contentTypeOptions = httpContext.Response.Headers["X-Content-Type-Options"].ToString();
			Assert.AreEqual( "nosniff",contentTypeOptions);
		}
	}
}

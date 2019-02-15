using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Middleware;

namespace starskytest.Middleware
{
	[TestClass]
	public class ContentSecurityPolicyMiddlewareTest
	{
		[TestMethod]
		public async Task MiddlewareExtensionsTest_CSPBasicSetupTest()
		{
			var host = WebHost.CreateDefaultBuilder()
				.UseUrls("http://localhost:5050")
				.Configure(app =>
				{
					app.UseContentSecurityPolicy();
					app.UseBasicAuthentication();
				}).Build();

			await host.StartAsync();
			await host.StopAsync();

		}

		[TestMethod]
		public async Task ContentSecurityPolicyMiddlewareTest_invoke_testContent()
		{
			// Arrange
			var httpContext = new DefaultHttpContext();
			var authMiddleware = new ContentSecurityPolicyMiddleware(next: (innerHttpContext) => Task.FromResult(0));

			// Act
			await authMiddleware.Invoke(httpContext);
			//test
			var csp = httpContext.Response.Headers["Content-Security-Policy"].ToString();
			Assert.AreEqual(true,csp.Contains("default-src"));
		}


	}
}

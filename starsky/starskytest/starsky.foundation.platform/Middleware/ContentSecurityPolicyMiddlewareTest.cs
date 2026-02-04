using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Extensions;
using starsky.foundation.platform.Extensions;
using starsky.foundation.platform.Middleware;

namespace starskytest.starsky.foundation.platform.Middleware;

[TestClass]
public sealed class ContentSecurityPolicyMiddlewareTest
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
				app.UseNoAccount(false);
				app.UseNoAccount(true);
				app.UseCheckIfAccountExist();
			}).Build();

		await host.StartAsync(TestContext.CancellationToken);
		await host.StopAsync(TestContext.CancellationToken);
		Assert.IsNotNull(host);
	}

	[TestMethod]
	public async Task ContentSecurityPolicyMiddlewareTest_invoke_testContent()
	{
		// Arrange
		var httpContext = new DefaultHttpContext { Request = { Scheme = "http" } };
		var authMiddleware =
			new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask);

		// Act
		await authMiddleware.Invoke(httpContext);
		//test
		var csp = httpContext.Response.Headers.ContentSecurityPolicy.ToString();
		Assert.Contains("default-src", csp);
		Assert.Contains("ws://", csp);
	}

	[TestMethod]
	public async Task invoke_httpsTest_websockets()
	{
		// Arrange
		var httpContext = new DefaultHttpContext { Request = { Scheme = "https" } };

		var authMiddleware =
			new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask);

		// Act
		await authMiddleware.Invoke(httpContext);
		//test
		var csp = httpContext.Response.Headers.ContentSecurityPolicy.ToString();

		Assert.Contains("default-src", csp);
		Assert.Contains("wss://", csp);
	}

	[TestMethod]
	public async Task invoke_httpsTest_websockets_localhostWithPort9000()
	{
		// Arrange
		var httpContext = new DefaultHttpContext
		{
			Request = { Scheme = "https", Host = new HostString("localhost", 9000) }
		};

		var authMiddleware =
			new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask);

		// Act
		await authMiddleware.Invoke(httpContext);
		//test
		var csp = httpContext.Response.Headers.ContentSecurityPolicy.ToString();

		Assert.Contains("default-src", csp);
		Assert.Contains("wss://localhost", csp);
		Assert.Contains("wss://localhost:9000", csp);
	}

	[TestMethod]
	public async Task invoke_httpsTest_websockets_localhostWithNoPort()
	{
		// Arrange
		var httpContext = new DefaultHttpContext
		{
			Request = { Scheme = "https", Host = new HostString("localhost") }
		};

		var authMiddleware =
			new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask);

		// Act
		await authMiddleware.Invoke(httpContext);
		//test
		var csp = httpContext.Response.Headers.ContentSecurityPolicy.ToString();

		Assert.Contains("default-src", csp);
		Assert.Contains("wss://localhost", csp);
		Assert.DoesNotContain("wss://localhost:", csp);
	}

	[TestMethod]
	public async Task ContentSecurityPolicyMiddlewareTest_invoke_otherTypes()
	{
		// Arrange
		var httpContext = new DefaultHttpContext { Request = { Scheme = "http" } };
		var authMiddleware =
			new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask);

		// Act
		await authMiddleware.Invoke(httpContext);

		// test
		var referrerPolicy = httpContext.Response.Headers["Referrer-Policy"].ToString();
		Assert.AreEqual("no-referrer", referrerPolicy);

		var frameOptions = httpContext.Response.Headers.XFrameOptions.ToString();
		Assert.AreEqual("DENY", frameOptions);

		// X-Xss-Protection
		var xssProtection = httpContext.Response.Headers.XXSSProtection.ToString();
		Assert.AreEqual("1; mode=block", xssProtection);

		// X-Content-Type-Options
		var contentTypeOptions = httpContext.Response.Headers.XContentTypeOptions.ToString();
		Assert.AreEqual("nosniff", contentTypeOptions);
	}


	[TestMethod]
	public async Task
		ContentSecurityPolicyMiddlewareTest_invoke_Chrome()
	{
		// Arrange
		var httpContext = new DefaultHttpContext { Request = { Scheme = "http" } };
		httpContext.Request.Headers.Append("User-Agent", "Chrome");
		var authMiddleware =
			new ContentSecurityPolicyMiddleware(_ => Task.CompletedTask);

		// Act
		await authMiddleware.Invoke(httpContext);

		var csp = httpContext.Response.Headers.ContentSecurityPolicy.ToString();

		Assert.Contains("require-trusted-types-for", csp);
	}

	public TestContext TestContext { get; set; }
}

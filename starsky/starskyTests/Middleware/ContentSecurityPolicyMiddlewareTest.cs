using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;
using starsky.Controllers;
using starsky.Data;
using starsky.Helpers;
using starsky.Middleware;
using starsky.Models;
using starsky.Services;
using starskytests.FakeMocks;

namespace starskytests.Middleware
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
			var csp = httpContext.Response.Headers["Content-Security-Policy"];
			Assert.AreEqual(true,csp.Contains("{default-src"));
		}


	}
}

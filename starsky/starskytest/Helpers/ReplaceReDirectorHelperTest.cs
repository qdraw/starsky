using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.storage.Helpers;
using starsky.Helpers;

namespace starskytest.Helpers;

[TestClass]
public class ReplaceReDirectorHelperTest
{
	private static RedirectContext<CookieAuthenticationOptions> Create(string path)
	{
		var context = new DefaultHttpContext { Request = { Path = path }, 
			Response = { Body = new MemoryStream() }
		};

		return new RedirectContext<CookieAuthenticationOptions>(
			context,
			new AuthenticationScheme("TestScheme", 
				"TestDisplayName", typeof(CookieAuthenticationHandler)),
			new CookieAuthenticationOptions(),
			null!,
			null!
		);
	}
	
	[DataTestMethod]
	[DataRow("/api/test", "{\"errors\": [{\"status\": \"401\" }]}", 
		HttpStatusCode.Unauthorized, "application/json")]
	[DataRow("/test", "", HttpStatusCode.OK, null)]
	public async Task ReplaceReDirector_ShouldReturnJsonResponse_WhenPathStartsWithApi(string path, 
		string expectedJson, HttpStatusCode statusCode, string? contentType)
	{
		// Arrange
		var fakeContext = Create(path);
		var existingReDirector = new Func<RedirectContext<CookieAuthenticationOptions>, Task>(_ => 
			Task.CompletedTask);

		// Act
		var redirectFunc = ReplaceReDirectorHelper.ReplaceReDirector(statusCode, existingReDirector);
		await redirectFunc(fakeContext);

		// Assert
		Assert.AreEqual((int)statusCode, fakeContext.Response.StatusCode);
		Assert.AreEqual(contentType, fakeContext.Response.ContentType);

		fakeContext.Response.Body.Seek(0, SeekOrigin.Begin);
		var result = await StreamToStringHelper.StreamToStringAsync(fakeContext.Response.Body);
		Assert.AreEqual(expectedJson, result);
	}
}

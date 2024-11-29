using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;

namespace starskytest.starsky.Helpers;

[TestClass]
public class CacheControlOverwriteTests
{
	[TestMethod]
	public void SetNoCacheResponseHeaders_ShouldSetCorrectHeaders()
	{
		var context = new DefaultHttpContext();
		var request = context.Request;

		CacheControlOverwrite.SetNoCacheResponseHeaders(request);

		Assert.AreEqual("no-store, no-cache, must-revalidate",
			request.HttpContext.Response.Headers.CacheControl);
	}

	[TestMethod]
	public void SetExpiresResponseHeaders_ShouldSetCorrectHeaders()
	{
		var context = new DefaultHttpContext();
		var request = context.Request;
		const int time = 29030401;

		CacheControlOverwrite.SetExpiresResponseHeaders(request, time);

		Assert.AreEqual($"private,max-age={time}",
			request.HttpContext.Response.Headers.CacheControl);
		Assert.AreEqual(time.ToString(), request.HttpContext.Response.Headers.Expires);
	}

	[TestMethod]
	public void SetExpiresResponseHeaders_ShouldSetDefaultTimeIfNotProvided()
	{
		var context = new DefaultHttpContext();
		var request = context.Request;

		CacheControlOverwrite.SetExpiresResponseHeaders(request);

		Assert.AreEqual("private,max-age=29030400",
			request.HttpContext.Response.Headers.CacheControl);
		Assert.AreEqual("29030400", request.HttpContext.Response.Headers.Expires);
	}

	[TestMethod]
	public void SetNoCacheResponseHeaders_ShouldRemoveExistingCacheControlHeader()
	{
		var context = new DefaultHttpContext();
		var request = context.Request;
		request.HttpContext.Response.Headers.Append("Cache-Control", "existing-value");

		CacheControlOverwrite.SetNoCacheResponseHeaders(request);

		Assert.AreEqual("no-store, no-cache, must-revalidate",
			request.HttpContext.Response.Headers.CacheControl);
	}

	[TestMethod]
	public void SetExpiresResponseHeaders_ShouldRemoveExistingCacheControlAndExpiresHeaders()
	{
		var context = new DefaultHttpContext();
		var request = context.Request;
		request.HttpContext.Response.Headers.Append("Cache-Control", "existing-value");
		request.HttpContext.Response.Headers.Append("Expires", "existing-value");

		CacheControlOverwrite.SetExpiresResponseHeaders(request);

		Assert.AreEqual("private,max-age=29030400",
			request.HttpContext.Response.Headers.CacheControl);
		Assert.AreEqual("29030400", request.HttpContext.Response.Headers.Expires);
	}
}

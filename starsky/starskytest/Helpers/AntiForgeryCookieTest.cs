using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Helpers;
using starskytest.FakeMocks;

namespace starskytest.Helpers
{
	[TestClass]
	public class AntiForgeryCookieTest
	{
		private string GetCookieValueFromResponse(HttpResponse response, string cookieName)
		{
			foreach (var headers in response.Headers)
			{
				if (headers.Key != "Set-Cookie")
					continue;
				string header = headers.Value;
				if (header.StartsWith($"{cookieName}="))
				{
					var p1 = header.IndexOf('=');
					var p2 = header.IndexOf(';');
					return header.Substring(p1 + 1, p2 - p1 - 1);
				}
			}
			return null;
		}

		
		[TestMethod]
		public void AntiForgeryCookie_SetRequestToken()
		{
			var httpContext = new DefaultHttpContext();
			new AntiForgeryCookie(new FakeAntiforgery()).SetAntiForgeryCookie(httpContext);
			var requestToken = GetCookieValueFromResponse(httpContext.Response, "X-XSRF-TOKEN");
			Assert.AreEqual(requestToken,"requestToken");
		}
	}
}

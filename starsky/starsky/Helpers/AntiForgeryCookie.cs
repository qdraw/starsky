using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace starsky.Helpers
{
	public class AntiForgeryCookie
	{
		private readonly IAntiforgery _antiForgery;

		public AntiForgeryCookie(IAntiforgery antiForgery)
		{
			_antiForgery = antiForgery;
		}
		
		/// <summary>
		/// The request token can be sent as a JavaScript-readable cookie, 
		/// </summary>
		/// <param name="httpContext">current context</param>
		public void SetAntiForgeryCookie(HttpContext httpContext)
		{
			if ( httpContext == null ) return;
			var tokens = _antiForgery.GetAndStoreTokens(httpContext);

			httpContext.Response.Cookies.Append(
				"X-XSRF-TOKEN", tokens.RequestToken, 
				new CookieOptions()
				{
					HttpOnly = false, // need to be false, is needed by the javascript front-end
					SameSite = SameSiteMode.Lax,
					Secure = httpContext.Request.IsHttps
				});
		}
	}
}

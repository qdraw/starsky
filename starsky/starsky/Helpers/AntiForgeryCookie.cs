using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace starsky.Helpers;

[SuppressMessage("Usage", "S3330:Make sure creating " +
                          "this cookie without the HttpOnly flag is safe",
	Justification = "As Designed used by front-end")]
[SuppressMessage("Usage", "S2092:Make sure creating " +
                          "without setting the 'Secure' property is safe here.",
	Justification = "As Designed used by front-end")]
public sealed class AntiForgeryCookie(IAntiforgery antiForgery)
{
	/// <summary>
	///     The request token can be sent as a JavaScript-readable cookie,
	/// </summary>
	/// <param name="httpContext">current context</param>
	public void SetAntiForgeryCookie(HttpContext httpContext)
	{
		if ( httpContext == null )
		{
			return;
		}

		var tokens = antiForgery.GetAndStoreTokens(httpContext).RequestToken;
		if ( tokens == null )
		{
			return;
		}

		httpContext.Response.Cookies.Append(
			"X-XSRF-TOKEN", tokens,
			new CookieOptions
			{
				HttpOnly = false, // need to be false, is needed by the javascript front-end
				SameSite = SameSiteMode.Lax,
				Secure = httpContext.Request.IsHttps,
				IsEssential = true
			});
	}
}

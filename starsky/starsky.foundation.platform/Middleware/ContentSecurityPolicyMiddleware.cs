using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace starsky.foundation.platform.Middleware
{
	public sealed class ContentSecurityPolicyMiddleware
	{
		private readonly RequestDelegate _next;

		public ContentSecurityPolicyMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		// IMyScopedService is injected into Invoke
		public async Task Invoke(HttpContext httpContext)
		{
			// For Error pages (for example 404) this middleware will be executed double,
			// so Adding a Header that already exist give an Error 500			
			if ( string.IsNullOrEmpty(httpContext.Response.Headers.ContentSecurityPolicy) )
			{
				// only needed for safari and old firefox
				var socketUrl = httpContext.Request.Scheme == "http"
					? $"ws://{httpContext.Request.Host.Host}"
					: $"wss://{httpContext.Request.Host.Host}";

				// For Safari localhost
				var socketUrlWithPort = string.Empty;
				if ( httpContext.Request.Host.Port != null )
				{
					socketUrlWithPort =
						$"{socketUrl}:{httpContext.Request.Host.Port}";
				}

				// When change also update in Electron
				var cspHeader =
					"default-src 'none'; img-src 'self' https://a.tile.openstreetmap.org/ " +
					"https://b.tile.openstreetmap.org/ " +
					"https://c.tile.openstreetmap.org/; script-src 'self'; " +
					$"connect-src 'self' {socketUrl} {socketUrlWithPort};" +
					"style-src 'self'; " +
					"font-src 'self'; " +
					"frame-ancestors 'none'; " +
					"base-uri 'none'; " +
					"form-action 'self'; " +
					"object-src 'none'; " +
					"media-src 'self'; " +
					"frame-src 'none'; " +
					"manifest-src 'self'; " +
					"block-all-mixed-content; ";

				// Currently not supported in Firefox and Safari (Edge user agent also includes the word Chrome)
				if ( httpContext.Request.Headers.UserAgent.Contains("Chrome") ||
					 httpContext.Request.Headers.UserAgent.Contains("csp-evaluator") )
				{
					cspHeader += "require-trusted-types-for 'script'; ";
				}

				// When change also update in Electron
				httpContext.Response.Headers
					.Append("Content-Security-Policy", cspHeader);
			}

			// @see: https://www.permissionspolicy.com/
			if ( string.IsNullOrEmpty(
					httpContext.Response.Headers["Permissions-Policy"]) )
			{
				httpContext.Response.Headers
					.Append("Permissions-Policy", "autoplay=(self), " +
												  "fullscreen=(self), " +
												  "geolocation=(self), " +
												  "picture-in-picture=(self), " +
												  "clipboard-read=(self), " +
												  "clipboard-write=(self), " +
												  "window-placement=(self)");
			}

			if ( string.IsNullOrEmpty(httpContext.Response.Headers["Referrer-Policy"]) )
			{
				httpContext.Response.Headers
					.Append("Referrer-Policy", "no-referrer");
			}

			if ( string.IsNullOrEmpty(httpContext.Response.Headers.XFrameOptions) )
			{
				httpContext.Response.Headers
					.Append("X-Frame-Options", "DENY");
			}

			if ( string.IsNullOrEmpty(httpContext.Response.Headers.XXSSProtection) )
			{
				httpContext.Response.Headers
					.Append("X-Xss-Protection", "1; mode=block");
			}

			if ( string.IsNullOrEmpty(httpContext.Response.Headers.XContentTypeOptions) )
			{
				httpContext.Response.Headers
					.Append("X-Content-Type-Options", "nosniff");
			}

			await _next(httpContext);
		}
	}
}

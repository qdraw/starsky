using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace starsky.foundation.platform.Middleware
{
	public class ContentSecurityPolicyMiddleware
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
			if (string.IsNullOrEmpty(httpContext.Response.Headers["Content-Security-Policy"]) )
			{
				// CSP 2.0 nonce // used in ApplicationInsightsJsHelper
				var nonce = Guid.NewGuid().ToString("N");
				httpContext.Items["csp-nonce"] = nonce;

				// only needed for safari and old firefox
				var socketUrl = httpContext.Request.Scheme == "http" 
					? $"ws://{httpContext.Request.Host.Host}" : $"wss://{httpContext.Request.Host.Host}";

				// For Safari localhost
				var socketUrlWithPort = string.Empty;
				if ( httpContext.Request.Host.Port != null )
				{
					socketUrlWithPort =
						$"{socketUrl}:{httpContext.Request.Host.Port}";
				}

				var cspHeader =
					$"default-src 'none'; img-src 'self' https://*.tile.openstreetmap.org; script-src 'self' " +
					$"https://js.monitor.azure.com/scripts/b/ai.2.min.js https://az416426.vo.msecnd.net \'nonce-{nonce}\'; " +
					$"connect-src 'self' {socketUrl} {socketUrlWithPort} " +
					$"https://*.in.applicationinsights.azure.com https://dc.services.visualstudio.com/v2/track; " +
					"style-src 'self'; " +
					"font-src 'self'; " +
					"frame-ancestors 'none'; " +
					"base-uri 'none'; " +
					"form-action 'self'; " +
					"object-src 'none'; " +
					"manifest-src 'self'; " +
					"block-all-mixed-content; ";

				// Currently not supported in Firefox and Safari (Edge user agent also includes the word Chrome)
				if (httpContext.Request.Headers.UserAgent.Contains("Chrome"))
				{
					cspHeader += "require-trusted-types-for 'script'; ";
				}
								                
				// When change also update in Electron
				httpContext.Response.Headers
					.Add("Content-Security-Policy",cspHeader);
			}

			// @see: https://www.permissionspolicy.com/
			if ( string.IsNullOrEmpty(
				    httpContext.Response.Headers["Permissions-Policy"]) )
			{
				httpContext.Response.Headers
					.Add("Permissions-Policy", "autoplay=(self), " +
					                           "fullscreen=(self), " +
					                           "geolocation=(self), " +
					                           "picture-in-picture=(self), " +
					                           "clipboard-read=(self), " +
					                           "clipboard-write=(self), " +
					                           "window-placement=(self)");
			}

			if (string.IsNullOrEmpty(httpContext.Response.Headers["Referrer-Policy"]) )
			{
				httpContext.Response.Headers
					.Add("Referrer-Policy", "no-referrer");
			}
			
			if (string.IsNullOrEmpty(httpContext.Response.Headers["X-Frame-Options"]) )
			{
				httpContext.Response.Headers
					.Add("X-Frame-Options", "DENY");
			}
			
			if (string.IsNullOrEmpty(httpContext.Response.Headers["X-Xss-Protection"]) )
			{
				httpContext.Response.Headers
					.Add("X-Xss-Protection", "1; mode=block");
			}
			
			if (string.IsNullOrEmpty(httpContext.Response.Headers["X-Content-Type-Options"]) )
			{
				httpContext.Response.Headers
					.Add("X-Content-Type-Options", "nosniff");
			}
				
			await _next(httpContext);
		}
	}
}

using System;
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
			// For Error pages (for example 404) this middleware will be executed double, so Adding a Header that already exist give an Error 500			
			if (string.IsNullOrEmpty(httpContext.Response.Headers["Content-Security-Policy"]) )
			{
				// CSP 2.0 nonce
				var nonce = Guid.NewGuid().ToString("N");
				httpContext.Items["csp-nonce"] = nonce;

				// only needed for safari and old firefox
				var socketUrl = httpContext.Request.Scheme == "http" 
					? $"ws://{httpContext.Request.Host.Host}" : $"wss://{httpContext.Request.Host.Host}";

				// When change also update in Electron
				httpContext.Response.Headers
					.Add("Content-Security-Policy",
						$"default-src 'self'; img-src 'self' https://*.tile.openstreetmap.org; script-src 'self' " +
						$"https://az416426.vo.msecnd.net \'nonce-{nonce}\'; " +
						$"connect-src 'self' {socketUrl} " +
						$"https://*.in.applicationinsights.azure.com https://dc.services.visualstudio.com/v2/track; " +
						$"style-src 'self'; " +
						$"font-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'; object-src 'none' ");
			}
				
			await _next(httpContext);
		}
	}
}

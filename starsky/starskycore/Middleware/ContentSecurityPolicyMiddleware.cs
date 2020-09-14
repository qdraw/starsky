using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace starskycore.Middleware
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
				// When change also update in Electron
				httpContext.Response.Headers
					.Add("Content-Security-Policy",
						$"default-src 'self'; img-src 'self' https://*.tile.openstreetmap.org; script-src 'self' https://az416426.vo.msecnd.net; " +
						$"connect-src 'self' {SocketUrl(httpContext)} https://dc.services.visualstudio.com; style-src 'self'; " +
						$"font-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'; object-src 'none' ");
			}
			await _next(httpContext);
		}

		/// <summary>
		/// only needed for Safari. 'self' is supported by Chrome/Firefox
		/// </summary>
		/// <param name="httpContext">context to know the domain</param>
		/// <returns>wss://domain.tld or ws://localhost:5000</returns>
		private static string SocketUrl(HttpContext httpContext)
		{
			var socketScheme = httpContext.Request.Scheme == "http" ? "ws://" : "wss://";
			return socketScheme + httpContext.Request.Host;
		}
	}
}

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace starsky.foundation.platform.Middleware;

public sealed class ContentSecurityPolicyMiddleware
{
	private readonly RequestDelegate _next;

	public ContentSecurityPolicyMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	/// <summary>
	///     Host name, IPv4 or bracketed IPv6 only (or empty); no spaces, semicolons or quotes
	/// </summary>
	internal static bool IsSafeCspHost(string? host)
	{
		return string.IsNullOrEmpty(host) || ( host.Length <= 255 &&
		                                       host.All(c => char.IsAsciiLetterOrDigit(c) ||
		                                                     c is '.' or '-' or ':' or '[' or ']') );
	}

	// IMyScopedService is injected into Invoke
	public async Task Invoke(HttpContext httpContext)
	{
		// For Error pages (for example 404) this middleware will be executed double,
		// so Adding a Header that already exist give an Error 500			
		if ( string.IsNullOrEmpty(httpContext.Response.Headers.ContentSecurityPolicy) )
		{
			// only needed for safari and old firefox
			// The Host header is client input and ends up in a header value: without this check
			// a Host like "x; script-src *" adds directives (and can be cached by a proxy)
			var socketUrl = string.Empty;
			var socketUrlWithPort = string.Empty;
			if ( IsSafeCspHost(httpContext.Request.Host.Host) )
			{
				socketUrl = httpContext.Request.Scheme == "http"
					? $"ws://{httpContext.Request.Host.Host}"
					: $"wss://{httpContext.Request.Host.Host}";

				// For Safari localhost
				if ( httpContext.Request.Host.Port != null )
				{
					socketUrlWithPort =
						$"{socketUrl}:{httpContext.Request.Host.Port}";
				}
			}

			// When change also update in Electron
			var cspHeader =
				"default-src 'none'; img-src 'self' " +
				"https://a.tile.openstreetmap.org/ " +
				"https://b.tile.openstreetmap.org/ " +
				"https://c.tile.openstreetmap.org/ " +
				"https://a.tile.openstreetmap.fr/ " +
				"https://b.tile.openstreetmap.fr/ " +
				"https://c.tile.openstreetmap.fr/ " +
				"; script-src 'self'; " +
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
				.Append("Referrer-Policy", "strict-origin-when-cross-origin");
		}

		if ( string.IsNullOrEmpty(httpContext.Response.Headers.XFrameOptions) )
		{
			httpContext.Response.Headers
				.Append("X-Frame-Options", "DENY");
		}

		if ( string.IsNullOrEmpty(httpContext.Response.Headers.XXSSProtection) )
		{
			httpContext.Response.Headers
				// the legacy XSS auditor is removed from browsers and was itself exploitable; CSP does the work
				.Append("X-Xss-Protection", "0");
		}

		if ( string.IsNullOrEmpty(httpContext.Response.Headers.XContentTypeOptions) )
		{
			httpContext.Response.Headers
				.Append("X-Content-Type-Options", "nosniff");
		}

		await _next(httpContext);
	}
}

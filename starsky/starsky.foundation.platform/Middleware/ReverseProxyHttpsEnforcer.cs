using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace starsky.foundation.platform.Middleware
{
	public class ReverseProxyHttpsEnforcer {
		
		private const string ForwardedProtoHeader = "X-Forwarded-Proto";
		private readonly RequestDelegate _next;

		public ReverseProxyHttpsEnforcer(RequestDelegate next) {
			_next = next;
		}

		/// <summary>
		/// When the header `X-Forwarded-Proto` is http this will be redirected to https
		/// Its does nothing when this header is not present
		/// </summary>
		/// <param name="context">user http context</param>
		/// <returns>Nothing, its a middleware Invoke</returns>
		public async Task Invoke(HttpContext context) {
			var headers = context.Request.Headers;

			if ( context.Request.Method.ToLowerInvariant() != "get" || 
			     string.IsNullOrEmpty(headers[ForwardedProtoHeader]) || 
			     headers[ForwardedProtoHeader] == "https" )
			{
				await _next(context);
				return;
			}
			
			var withHttps = string.Concat(
				"https://",
				context.Request.Host.ToUriComponent(),
				context.Request.PathBase.ToUriComponent(),
				context.Request.Path.ToUriComponent(),
				context.Request.QueryString.ToUriComponent());
			
			context.Response.Redirect(withHttps);
		}
	}
}

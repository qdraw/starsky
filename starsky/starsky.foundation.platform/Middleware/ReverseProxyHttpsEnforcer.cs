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

		public async Task Invoke(HttpContext ctx) {
			var headers = ctx.Request.Headers;
			if (headers[ForwardedProtoHeader] == string.Empty || headers[ForwardedProtoHeader] == "https") {
				await _next(ctx);
				return;
			} 
			
			var withHttps = $"https://{ctx.Request.Host}{ctx.Request.Path}{ctx.Request.QueryString}";
			ctx.Response.Redirect(withHttps);
		}
	}
}

using Microsoft.AspNetCore.Builder;
using starsky.foundation.platform.Middleware;

namespace starsky.foundation.platform.Extensions
{
	public static class ReverseProxyHttpsEnforcerExtensions {
		public static IApplicationBuilder UseReverseProxyHttpsEnforcer(this IApplicationBuilder builder) {
			return builder.UseMiddleware<ReverseProxyHttpsEnforcer>();
		}
	}
}

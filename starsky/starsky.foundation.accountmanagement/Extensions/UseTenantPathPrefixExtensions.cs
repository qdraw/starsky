using Microsoft.AspNetCore.Builder;
using starsky.foundation.accountmanagement.Middleware;

namespace starsky.foundation.accountmanagement.Extensions;

public static class UseTenantPathPrefixExtensions
{
	public static IApplicationBuilder UseTenantPathPrefix(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<TenantPathPrefixMiddleware>();
	}
}

using Microsoft.AspNetCore.Builder;
using starsky.foundation.accountmanagement.Middleware;

namespace starsky.foundation.accountmanagement.Extensions;

public static class UseTenantSessionAuthenticationExtensions
{
	public static IApplicationBuilder UseTenantSessionAuthentication(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<TenantSessionAuthenticationMiddleware>();
	}
}

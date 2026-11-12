using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace starsky.foundation.platform.Tenancy;

/// <summary>
///     Reads /t/{tenant}/ from the url (never from headers or bodies) and moves it to the
///     PathBase so routing and controllers see the same paths as before.
///     No prefix means the default tenant.
/// </summary>
public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
	internal const string Prefix = "/t";

	public async Task Invoke(HttpContext httpContext)
	{
		var tenantId = TenantIdHelper.DefaultTenantId;

		if ( httpContext.Request.Path.StartsWithSegments(Prefix, out var afterPrefix) )
		{
			var segments = afterPrefix.Value?.Split('/', 3) ?? [];
			// segments[0] is empty because the remainder starts with a slash
			var candidate = segments.Length > 1 ? segments[1] : null;
			var registry = httpContext.RequestServices.GetRequiredService<ITenantRegistry>();

			// the default tenant has no prefix: /t/default would be a second url for it
			if ( !TenantIdHelper.IsValid(candidate) ||
			     candidate == TenantIdHelper.DefaultTenantId ||
			     !registry.IsActive(candidate!) )
			{
				// the same answer for unknown, invalid and inactive tenants. That an existing tenant
				// answers 401/403 (and an unknown one 404) is accepted: ids are not secret, the data
				// is protected by the membership check
				httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
				return;
			}

			tenantId = candidate!;
			httpContext.Request.Path.StartsWithSegments($"{Prefix}/{tenantId}",
				out var remaining);
			httpContext.Request.PathBase =
				httpContext.Request.PathBase.Add($"{Prefix}/{tenantId}");
			httpContext.Request.Path = remaining;
		}

		var tenantContext = httpContext.RequestServices.GetRequiredService<ITenantContext>();
		( ( TenantContext ) tenantContext ).SetTenant(tenantId);

		// every log line of this request names the tenant
		var logger = httpContext.RequestServices.GetService<ILogger<TenantResolutionMiddleware>>();
		using ( logger?.BeginScope(new Dictionary<string, object> { ["TenantId"] = tenantId }) )
		{
			await next(httpContext);
		}
	}
}

public static class TenantResolutionMiddlewareExtensions
{
	public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<TenantResolutionMiddleware>();
	}
}

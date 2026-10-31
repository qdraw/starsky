using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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

			if ( !TenantIdHelper.IsValid(candidate) || !registry.IsActive(candidate!) )
			{
				// same answer for unknown and invalid: do not reveal which tenants exist
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

		await next(httpContext);
	}
}

public static class TenantResolutionMiddlewareExtensions
{
	public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<TenantResolutionMiddleware>();
	}
}

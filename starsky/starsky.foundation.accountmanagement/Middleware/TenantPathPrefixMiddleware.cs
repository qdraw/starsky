using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;

namespace starsky.foundation.accountmanagement.Middleware;

public sealed class TenantPathPrefixMiddleware(RequestDelegate next)
{
	private static readonly string[] GlobalPrefixes =
	[
		"/-/",
		"/assets/",
		"/api/health",
		"/api/open-telemetry",
		"/api/tenants/mine",
		"/error",
		"/swagger",
		"/realtime"
	];

	private static readonly string[] TenantRequiredPrefixes =
	[
		"/api/",
		"/account/",
		"/search",
		"/trash",
		"/import",
		"/preferences"
	];

	public async Task Invoke(HttpContext context)
	{
		var validator = (ITenantSlugValidator)context.RequestServices.GetRequiredService(typeof(ITenantSlugValidator));
		var path = context.Request.Path.Value ?? string.Empty;

		if (path == "/")
		{
			context.Response.StatusCode = StatusCodes.Status404NotFound;
			await context.Response.WriteAsync("Tenant prefix is required");
			return;
		}

		if (IsGlobalPath(path))
		{
			await next(context);
			return;
		}

		var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (segments.Length >= 2 && validator.IsValid(segments[0]))
		{
			var tenantSlug = segments[0];
			var rewritten = "/" + string.Join('/', segments.Skip(1));
			context.Items[TenantAuthenticationConstants.TenantSlugItemKey] = tenantSlug;
			context.Request.Path = string.IsNullOrWhiteSpace(rewritten) ? "/" : rewritten;
			await next(context);
			return;
		}

		if (RequiresTenant(path))
		{
			context.Response.StatusCode = StatusCodes.Status404NotFound;
			await context.Response.WriteAsync("Tenant prefix is required");
			return;
		}

		await next(context);
	}

	private static bool IsGlobalPath(string path)
	{
		if (path == "/-/tenants" || path == "/api/tenants/mine")
		{
			return true;
		}

		return GlobalPrefixes.Any(path.StartsWith);
	}

	private static bool RequiresTenant(string path)
	{
		return TenantRequiredPrefixes.Any(path.StartsWith);
	}
}

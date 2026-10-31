using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.accountmanagement.Tenancy;
using starsky.foundation.platform.Tenancy;

namespace starsky.foundation.accountmanagement.Middleware;

/// <summary>
///     Roles and permissions are per tenant, so they are not taken from the cookie (it is valid
///     for 60 days and was made for one tenant). Every request gets the claims of the membership
///     of the user in the tenant of the url, and a user that is no member gets 403 on the API.
///     Runs after authentication, so also for Basic authentication
/// </summary>
public sealed class TenantMembershipMiddleware(RequestDelegate next)
{
	public const string PlatformAdminClaim = "PlatformAdmin";
	public const string PermissionClaim = "Permission";

	// these decide for themselves: log in, ask for the own tenants, manage tenants
	private static readonly string[] NoMembershipNeeded =
		["/api/account", "/api/tenants", "/api/health"];

	internal static bool RequiresMembership(PathString path)
	{
		var value = path.Value;
		var isTenantApi = !string.IsNullOrEmpty(value) &&
		                  ( value.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
		                    value.StartsWith("/realtime", StringComparison.OrdinalIgnoreCase) );
		// the folder itself or below it: /api/tenantsx is not /api/tenants
		return isTenantApi && !NoMembershipNeeded.Any(p =>
			value!.Equals(p, StringComparison.OrdinalIgnoreCase) ||
			value.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase));
	}

	public async Task Invoke(HttpContext context)
	{
		if ( context.User.Identity?.IsAuthenticated == true )
		{
			var idAsString = context.User.Claims
				.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			if ( int.TryParse(idAsString, out var userId) && userId > 0 )
			{
				var tenantId = context.RequestServices.GetRequiredService<ITenantContext>().TenantId;
				var service = context.RequestServices
					.GetRequiredService<ITenantMembershipService>();
				var access = await service.GetAccessAsync(userId, tenantId);

				context.User = Rebuild(context.User, access);

				if ( access.Membership == null && RequiresMembership(context.Request.Path) )
				{
					context.Response.StatusCode = StatusCodes.Status403Forbidden;
					await context.Response.WriteAsync("No access to this tenant",
						context.RequestAborted);
					return;
				}
			}
		}

		await next.Invoke(context);
	}

	/// <summary>
	///     Same identity, with the role and permission claims of this tenant only
	/// </summary>
	internal static ClaimsPrincipal Rebuild(ClaimsPrincipal principal, TenantAccess access)
	{
		var original = principal.Identities.First();
		var claims = original.Claims.Where(c => c.Type != ClaimTypes.Role &&
		                                        c.Type != PermissionClaim &&
		                                        c.Type != PlatformAdminClaim).ToList();

		if ( access.Membership != null )
		{
			claims.Add(new Claim(ClaimTypes.Role, access.Membership.RoleCode));
			claims.AddRange(access.Membership.Permissions
				.Select(p => new Claim(PermissionClaim, p)));
		}

		if ( access.IsPlatformAdmin )
		{
			claims.Add(new Claim(PlatformAdminClaim, "true"));
		}

		return new ClaimsPrincipal(new ClaimsIdentity(claims, original.AuthenticationType));
	}
}

public static class TenantMembershipMiddlewareExtensions
{
	public static IApplicationBuilder UseTenantMembership(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<TenantMembershipMiddleware>();
	}
}

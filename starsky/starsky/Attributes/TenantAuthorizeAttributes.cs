using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.platform.Models;

namespace starsky.Attributes;

/// <summary>
///     Administrator of the tenant in the url (the role claim is set per request from the
///     membership, see TenantMembershipMiddleware)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class TenantAdminAttribute : AuthorizeAttribute, IAuthorizationFilter
{
	public void OnAuthorization(AuthorizationFilterContext context)
	{
		var user = context.HttpContext.User;
		if ( user.Identity?.IsAuthenticated != true )
		{
			context.Result = TenantAuthorizeResults.Unauthorized;
			return;
		}

		var administrator = AccountRoles.AppAccountRoles.Administrator.ToString();
		if ( !user.HasClaim(ClaimTypes.Role, administrator) )
		{
			context.Result = TenantAuthorizeResults.Forbidden;
		}
	}
}

/// <summary>
///     Platform administrator: creates and disables tenants. Gives no access to the photos
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PlatformAdminAttribute : AuthorizeAttribute, IAuthorizationFilter
{
	public void OnAuthorization(AuthorizationFilterContext context)
	{
		var user = context.HttpContext.User;
		if ( user.Identity?.IsAuthenticated != true )
		{
			context.Result = TenantAuthorizeResults.Unauthorized;
			return;
		}

		if ( !user.Claims.Any(c => c.Type == TenantMembershipMiddleware.PlatformAdminClaim) )
		{
			context.Result = TenantAuthorizeResults.Forbidden;
		}
	}
}

/// <summary>
///     With a body: a response without a body is replaced by the status code pages middleware,
///     which re-executes the request on /Error with the same method (a POST then answers 405)
/// </summary>
internal static class TenantAuthorizeResults
{
	internal static ContentResult Unauthorized =>
		new() { StatusCode = StatusCodes.Status401Unauthorized, Content = "Unauthorized" };

	internal static ContentResult Forbidden =>
		new() { StatusCode = StatusCodes.Status403Forbidden, Content = "Forbidden" };
}

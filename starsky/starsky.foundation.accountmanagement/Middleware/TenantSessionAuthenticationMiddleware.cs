using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;

namespace starsky.foundation.accountmanagement.Middleware;

public sealed class TenantSessionAuthenticationMiddleware(RequestDelegate next)
{
	public async Task Invoke(HttpContext context)
	{
		var dbContext = (ApplicationDbContext)context.RequestServices.GetRequiredService(typeof(ApplicationDbContext));
		var sessionStore = (ITenantSessionStore)context.RequestServices.GetRequiredService(typeof(ITenantSessionStore));
		var tenantSlug = context.Items[TenantAuthenticationConstants.TenantSlugItemKey] as string;
		var path = context.Request.Path.Value ?? string.Empty;
		var endpoint = context.GetEndpoint();
		var requiresAuthorization = endpoint?.Metadata.GetMetadata<IAuthorizeData>() != null;
		var isApiCall = path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
		var isGlobalTenantsMine = path.Equals("/api/tenants/mine", StringComparison.OrdinalIgnoreCase);

		if (string.IsNullOrWhiteSpace(tenantSlug) && !isGlobalTenantsMine)
		{
			await next(context);
			return;
		}

		if (context.User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(tenantSlug))
		{
			if (await TrySetTenantClaimsForExistingPrincipalAsync(context, dbContext, tenantSlug))
			{
				await next(context);
				return;
			}

			if (requiresAuthorization && isApiCall)
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				await context.Response.WriteAsync("Forbidden");
				return;
			}
		}

		if (!context.Request.Cookies.TryGetValue(TenantAuthenticationConstants.SessionCookieName, out var sessionId) ||
			string.IsNullOrWhiteSpace(sessionId))
		{
			if (requiresAuthorization && (isApiCall || isGlobalTenantsMine))
			{
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				await context.Response.WriteAsync("Unauthorized");
				return;
			}

			await next(context);
			return;
		}

		var session = await sessionStore.GetValidSessionAsync(sessionId);
		if (session == null)
		{
			if (requiresAuthorization && (isApiCall || isGlobalTenantsMine))
			{
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				await context.Response.WriteAsync("Unauthorized");
				return;
			}

			await next(context);
			return;
		}

		if (isGlobalTenantsMine)
		{
			var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
			if (user == null)
			{
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				await context.Response.WriteAsync("Unauthorized");
				return;
			}

			context.User = CreateClaimsPrincipal(user.Id, user.IsGlobalAdmin, null, null);
			await next(context);
			return;
		}

		var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == tenantSlug);
		if (tenant == null || !tenant.IsEnabled)
		{
			if (requiresAuthorization && isApiCall)
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				await context.Response.WriteAsync("Forbidden");
				return;
			}

			await next(context);
			return;
		}

		if (!await sessionStore.IsTenantActivatedAsync(session.Id, tenant.Id))
		{
			if (requiresAuthorization && isApiCall)
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				await context.Response.WriteAsync("Forbidden");
				return;
			}

			await next(context);
			return;
		}

		var membership = await dbContext.TenantUsers
			.FirstOrDefaultAsync(m => m.TenantId == tenant.Id && m.UserId == session.UserId);
		if (membership == null)
		{
			if (requiresAuthorization && isApiCall)
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				await context.Response.WriteAsync("Forbidden");
				return;
			}

			await next(context);
			return;
		}

		var account = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == session.UserId);
		if (account == null)
		{
			if (requiresAuthorization && isApiCall)
			{
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				await context.Response.WriteAsync("Unauthorized");
				return;
			}

			await next(context);
			return;
		}

		context.User = CreateClaimsPrincipal(account.Id, account.IsGlobalAdmin, tenant, membership.Role);
		await sessionStore.TouchSessionAsync(session.Id, tenant.Id);
		await next(context);
	}

	private static async Task<bool> TrySetTenantClaimsForExistingPrincipalAsync(HttpContext context,
		ApplicationDbContext dbContext, string tenantSlug)
	{
		var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (!int.TryParse(userIdValue, out var userId))
		{
			return false;
		}

		var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == tenantSlug && t.IsEnabled);
		if (tenant == null)
		{
			return false;
		}

		var membership = await dbContext.TenantUsers
			.FirstOrDefaultAsync(m => m.TenantId == tenant.Id && m.UserId == userId);
		if (membership == null)
		{
			return false;
		}

		var account = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
		if (account == null)
		{
			return false;
		}

		context.User = CreateClaimsPrincipal(account.Id, account.IsGlobalAdmin, tenant, membership.Role);
		return true;
	}

	private static ClaimsPrincipal CreateClaimsPrincipal(int userId, bool isGlobalAdmin,
		Tenant? tenant, TenantRole? tenantRole)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, userId.ToString()),
			new(TenantAuthenticationConstants.GlobalAdminClaimType, isGlobalAdmin.ToString().ToLowerInvariant())
		};

		if (tenant != null)
		{
			claims.Add(new Claim(TenantAuthenticationConstants.TenantSlugClaimType, tenant.Slug));
			claims.Add(new Claim(TenantAuthenticationConstants.TenantIdClaimType, tenant.Id.ToString()));
		}

		if (tenantRole.HasValue)
		{
			claims.Add(new Claim(TenantAuthenticationConstants.TenantRoleClaimType, tenantRole.Value.ToString()));
		}

		var identity = new ClaimsIdentity(claims, "StarskyTenantSession");
		return new ClaimsPrincipal(identity);
	}
}

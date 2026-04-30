using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;

// ReSharper disable once IdentifierTypo
[assembly: InternalsVisibleTo("starskytest")]

namespace starsky.foundation.accountmanagement.Middleware;

/// <summary>
///     Auto login when use is on localhost
/// </summary>
public sealed class NoAccountMiddleware
{
	/// <summary>
	///     Default email address or Default User
	/// </summary>
	internal const string Identifier = "mail@localhost";

	private const string CredentialType = "email";
	private readonly AppSettings? _appSettings;

	private readonly RequestDelegate _next;

	public NoAccountMiddleware(RequestDelegate next, AppSettings? appSettings = null)
	{
		_next = next;
		_appSettings = appSettings;
	}

	/// <summary>
	///     Enable: app__NoAccountLocalhost
	/// </summary>
	/// <param name="context"></param>
	public async Task Invoke(HttpContext context)
	{
		var isHostAllowed = IsLocalhost.IsHostLocalHost(context.Connection.LocalIpAddress,
								context.Connection.RemoteIpAddress) ||
							_appSettings?.DemoUnsafeDeleteStorageFolder == true;

		var isApiCall = context.Request.Path.HasValue &&
						( context.Request.Path.Value.StartsWith("/api") ||
						  context.Request.Path.Value.StartsWith("/realtime") );

		var isFromLogoutCall = context.Request.QueryString.HasValue &&
							   context.Request.QueryString.Value!.Contains("fromLogout");

		if ( isHostAllowed && context.User.Identity?.IsAuthenticated == false && !isApiCall &&
			 !isFromLogoutCall )
		{
			var userManager =
				( IUserManager ) context.RequestServices.GetRequiredService(typeof(IUserManager));
			var dbContext =
				( ApplicationDbContext ) context.RequestServices.GetRequiredService(typeof(ApplicationDbContext));
			var sessionStore =
				( ITenantSessionStore ) context.RequestServices.GetRequiredService(typeof(ITenantSessionStore));
			var user = await CreateOrUpdateNewUsers(userManager, dbContext, sessionStore);
			await userManager.SignIn(context, user, true);
		}

		await _next.Invoke(context);
	}

	internal static async Task<User?> CreateOrUpdateNewUsers(IUserManager userManager, ApplicationDbContext dbContext, ITenantSessionStore sessionStore)
	{
		var user = userManager.GetUser(CredentialType, Identifier);
		if ( user == null )
		{
			var newPassword = Convert.ToBase64String(
				Pbkdf2Hasher.GenerateRandomSalt());
			await userManager.SignUpAsync(string.Empty,
				CredentialType, Identifier, newPassword + newPassword);
			user = userManager.GetUser(CredentialType, Identifier);
		}
		else
		{
			// Upgrade Path to phase out the old sha1 / iteration count
			var credentialType = userManager.GetCachedCredentialType(CredentialType);
			var credential =
				user.Credentials!.FirstOrDefault(p => p.CredentialTypeId == credentialType?.Id);
			if ( credential!.IterationCount == IterationCountType.Iterate100KSha256 )
			{
				// Credential is up-to-date, continue
			}
			else
			{
				var newPassword = Convert.ToBase64String(
					Pbkdf2Hasher.GenerateRandomSalt());
				userManager.ChangeSecret(CredentialType, Identifier, newPassword + newPassword);
			}
		}

		// Handle multi-tenancy: ensure user is in main tenant and session is activated
		if ( user != null )
		{
			await EnsureUserInMainTenant(user, dbContext, sessionStore);
		}

		return user;
	}

	private static async Task EnsureUserInMainTenant(User user, ApplicationDbContext dbContext, ITenantSessionStore sessionStore)
	{
		// Guard: user must be persisted (have a valid ID) to be assigned to a tenant
		if ( user.Id <= 0 )
		{
			return;
		}

		// Ensure main tenant exists
		var mainTenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == "main");
		if ( mainTenant == null )
		{
			mainTenant = new Tenant
			{
				Slug = "main",
				Name = "main",
				IsEnabled = true
			};
			dbContext.Tenants.Add(mainTenant);
			await dbContext.SaveChangesAsync();
		}

		// Check if user is already a member of main tenant
		var existingMembership = await dbContext.TenantUsers
			.FirstOrDefaultAsync(tu => tu.TenantId == mainTenant.Id && tu.UserId == user.Id);

		if ( existingMembership == null )
		{
			// Add user to main tenant as admin for localhost development
			var tenantUser = new TenantUser
			{
				TenantId = mainTenant.Id,
				UserId = user.Id,
				Role = TenantRole.Admin
			};
			dbContext.TenantUsers.Add(tenantUser);
			await dbContext.SaveChangesAsync();
		}

		// Create/get a session for the user and activate the main tenant
		var session = await sessionStore.CreateOrRefreshSessionAsync(user.Id);
		await sessionStore.ActivateTenantAsync(session.Id, mainTenant.Id);
	}
}

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;
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
				( IUserManager )context.RequestServices.GetRequiredService(typeof(IUserManager));
			var user = await CreateOrUpdateNewUsers(userManager);
			await userManager.SignIn(context, user, true);
		}

		await _next.Invoke(context);
	}

	internal static async Task<User?> CreateOrUpdateNewUsers(IUserManager userManager)
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
			if ( credential!.IterationCount == IterationCountType.Iterate100KSha256 ) return user;

			var newPassword = Convert.ToBase64String(
				Pbkdf2Hasher.GenerateRandomSalt());
			userManager.ChangeSecret(CredentialType, Identifier, newPassword + newPassword);
		}

		return user;
	}
}

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.accountmanagement.Interfaces;

// ReSharper disable once IdentifierTypo
namespace starsky.foundation.accountmanagement.Middleware;

/// <summary>
///     Check if login has entity in database
/// </summary>
public sealed class CheckIfAccountExistMiddleware(RequestDelegate next)
{
	internal static int GetUserTableIdFromClaims(HttpContext httpContext)
	{
		var idAsString = httpContext.User.Claims
			.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
			?.Value;
		return int.TryParse(idAsString, out var id) ? id : 0;
	}

	public async Task Invoke(HttpContext context)
	{
		// The cookie is valid for 60 days and has no server side session, so check on every API
		// call that the user still exists. Sign-in/out calls are skipped to let a stale cookie
		// be replaced by a new login.
		var path = context.Request.Path.Value;
		var isApiCall = !string.IsNullOrEmpty(path) &&
		                path.Contains("api/", StringComparison.OrdinalIgnoreCase) &&
		                !path.EndsWith("api/account/login", StringComparison.OrdinalIgnoreCase) &&
		                !path.EndsWith("api/account/logout", StringComparison.OrdinalIgnoreCase) &&
		                !path.EndsWith("api/account/register", StringComparison.OrdinalIgnoreCase);

		if ( context.User.Identity?.IsAuthenticated == true && isApiCall )
		{
			var userManager =
				( IUserManager ) context.RequestServices.GetRequiredService(typeof(IUserManager));

			var id = GetUserTableIdFromClaims(context);
			var result = await userManager.ExistAsync(id);
			if ( result == null )
			{
				userManager.SignOut(context);
				context.Response.StatusCode = 401;
				await context.Response.WriteAsync("User is deleted", context.RequestAborted);
				return;
			}
		}

		await next.Invoke(context);
	}
}

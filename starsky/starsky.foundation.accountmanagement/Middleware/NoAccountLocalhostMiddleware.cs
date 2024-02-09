using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.platform.Models;

// ReSharper disable once IdentifierTypo
[assembly: InternalsVisibleTo("starskytest")]
namespace starsky.foundation.accountmanagement.Middleware
{
	/// <summary>
	/// Auto login when use is on localhost
	/// </summary>
	public sealed class NoAccountMiddleware
	{
		public NoAccountMiddleware(RequestDelegate next, AppSettings? appSettings = null)
		{
			_next = next;
			_appSettings = appSettings;
		}

		private readonly RequestDelegate _next;
		private readonly AppSettings? _appSettings;

		internal const string Identifier = "mail@localhost";

		/// <summary>
		/// Enable: app__NoAccountLocalhost
		/// </summary>
		/// <param name="context"></param>
		public async Task Invoke(HttpContext context)
		{
			var isHostAllowed = IsLocalhost.IsHostLocalHost(context.Connection.LocalIpAddress,
				context.Connection.RemoteIpAddress) || _appSettings?.DemoUnsafeDeleteStorageFolder == true;

			var isApiCall = context.Request.Path.HasValue && ( context.Request.Path.Value.StartsWith("/api") ||
															  context.Request.Path.Value.StartsWith("/realtime") );

			var isFromLogoutCall = context.Request.QueryString.HasValue &&
								   context.Request.QueryString.Value!.Contains("fromLogout");

			if ( isHostAllowed && context.User.Identity?.IsAuthenticated == false && !isApiCall && !isFromLogoutCall )
			{
				var userManager = ( IUserManager )context.RequestServices.GetRequiredService(typeof(IUserManager));

				var user = userManager.GetUser("email", Identifier);
				if ( user == null )
				{
					var newPassword = Convert.ToBase64String(
							Pbkdf2Hasher.GenerateRandomSalt());
					await userManager.SignUpAsync(string.Empty,
						"email", Identifier, newPassword + newPassword);
					user = userManager.GetUser("email", Identifier);
				}

				await userManager.SignIn(context, user, true);
			}
			await _next.Invoke(context);
		}
	}
}

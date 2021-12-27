using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.accountmanagement.Helpers;
using starsky.foundation.accountmanagement.Interfaces;

// ReSharper disable once IdentifierTypo
namespace starsky.foundation.accountmanagement.Middleware
{
	/// <summary>
	/// Check if login has entity in database
	/// </summary>
	public class CheckIfAccountExistMiddleware
	{
       
		public CheckIfAccountExistMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		private readonly RequestDelegate _next;

		internal static int GetUserTableIdFromClaims(HttpContext httpContext)
		{
			var idAsString = httpContext.User.Claims
				.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
				?.Value;
			int.TryParse(idAsString, out var id);
			return id;
		}

		public async Task Invoke(HttpContext context)
		{
			var isApiCall = context.Request.Path.HasValue && (
				context.Request.Path.Value.EndsWith("api/health/details") || 
				context.Request.Path.Value.EndsWith("api/index") ||
				context.Request.Path.Value.EndsWith("api/search") ||
				context.Request.Path.Value.EndsWith("api/account/status") ||
				context.Request.Path.Value.EndsWith("api/env/"));

			if (context.User.Identity.IsAuthenticated && isApiCall)
			{
				var userManager = (IUserManager) context.RequestServices.GetService(typeof(IUserManager));

				var id = GetUserTableIdFromClaims(context);
				var result = await userManager.Exist(id);
				if ( result == null)
				{
					userManager.SignOut(context);
					context.Response.StatusCode = 401;
					await context.Response.WriteAsync("User is deleted");
					return;
				}
			}
			await _next.Invoke(context);
		}
	}
}

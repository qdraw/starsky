using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace starsky.Helpers;

internal static class ReplaceReDirectorHelper
{
	/// <summary>
	///     Does the current user get a redirect or 401 page
	/// </summary>
	/// <param name="statusCode">current status code</param>
	/// <param name="existingReDirector">func of RedirectContext</param>
	/// <returns></returns>
	internal static Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceReDirector(
		HttpStatusCode statusCode,
		Func<RedirectContext<CookieAuthenticationOptions>, Task> existingReDirector)
	{
		return context =>
		{
			if ( !context.Request.Path.StartsWithSegments("/api") )
			{
				return existingReDirector(context);
			}

			context.Response.StatusCode = ( int ) statusCode;
			var jsonString = "{\"errors\": [{\"status\": \"" + ( int ) statusCode + "\" }]}";

			context.Response.ContentType = "application/json";
			var data = Encoding.UTF8.GetBytes(jsonString);
			return context.Response.Body.WriteAsync(data, 0, data.Length);
		};
	}
}

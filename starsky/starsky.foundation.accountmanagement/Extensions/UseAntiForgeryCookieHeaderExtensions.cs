using Microsoft.AspNetCore.Builder;
using starsky.foundation.accountmanagement.Middleware;

// ReSharper disable once IdentifierTypo
namespace starsky.foundation.accountmanagement.Extensions
{
	public static class UseAntiForgeryCookieHeaderExtensions
	{
		public static IApplicationBuilder UseAntiForgeryCookieHeader(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<AntiForgeryCookieHeaderMiddleware>();
		}
	}
}

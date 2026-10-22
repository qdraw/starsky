using System;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.Helpers;

/// <summary>
///     Names of the rate limit policies that are registered in AddRateLimiting
/// </summary>
public static class RateLimitPolicies
{
	/// <summary>
	///     Endpoints that can be used without a login and do work on the server
	/// </summary>
	public const string Anonymous = "anonymous";

	/// <summary>
	///     Login and register: limits password guessing per client
	/// </summary>
	public const string Login = "login";

	/// <summary>
	///     Per client IP limits for the endpoints that work without a login, the policies are used
	///     with [EnableRateLimiting]. Client IP is the forwarded one when behind a trusted proxy.
	/// </summary>
	internal static void AddRateLimiting(IServiceCollection services)
	{
		services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
			options.AddPolicy(Anonymous, context =>
				RateLimitPartition.GetFixedWindowLimiter(ClientKey(context),
					_ => FixedWindow(60)));
			options.AddPolicy(Login, context =>
				RateLimitPartition.GetFixedWindowLimiter(ClientKey(context),
					_ => FixedWindow(30)));
		});
	}

	private static string ClientKey(HttpContext context)
	{
		return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
	}

	private static FixedWindowRateLimiterOptions FixedWindow(int permitLimit)
	{
		return new FixedWindowRateLimiterOptions
		{
			PermitLimit = permitLimit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
		};
	}
}

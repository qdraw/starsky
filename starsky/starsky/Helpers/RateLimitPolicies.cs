using System;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.platform.Models;

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
	///     The MCP endpoint for AI agents: needs a login, but one agent call can fan out into many
	/// </summary>
	public const string Mcp = "mcp";

	/// <summary>
	///     Per client IP limits for the endpoints that work without a login, the policies are used
	///     with [EnableRateLimiting]. Client IP is the forwarded one when behind a trusted proxy.
	/// </summary>
	internal static void AddRateLimiting(IServiceCollection services)
	{
		services.AddRateLimiter(options =>
		{
			options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
			options.AddPolicy(Anonymous, context => Partition(context, 60));
			options.AddPolicy(Login, context => Partition(context, 30));
			options.AddPolicy(Mcp, context => Partition(context, 120));
		});
	}

	/// <summary>
	///     The setting is read per request, so it can be switched at runtime (/api/env),
	///     for example by the end-to-end tests that log in before every test
	/// </summary>
	private static RateLimitPartition<string> Partition(HttpContext context, int permitLimit)
	{
		if ( context.RequestServices.GetService<AppSettings>()?.UseRateLimiting == false )
		{
			return RateLimitPartition.GetNoLimiter("disabled");
		}

		return RateLimitPartition.GetFixedWindowLimiter(ClientKey(context),
			_ => FixedWindow(permitLimit));
	}

	private static string ClientKey(HttpContext context)
	{
		return context.Connection.RemoteIpAddress is { } address ? address.ToString() : "unknown";
	}

	private static FixedWindowRateLimiterOptions FixedWindow(int permitLimit)
	{
		return new FixedWindowRateLimiterOptions
		{
			PermitLimit = permitLimit, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
		};
	}
}

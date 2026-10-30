using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using starsky.project.mcp.Tools;
using starsky.foundation.platform.Models;

namespace starsky.project.mcp;

public static class McpServiceExtensions
{
	public const string Route = "/mcp";

	/// <summary>
	///     Register the MCP server, only when McpEnabled is true.
	///     Stateless: the tools use scoped services (database), so every request gets its own scope
	/// </summary>
	public static IServiceCollection AddStarskyMcp(this IServiceCollection services,
		AppSettings appSettings)
	{
		if ( appSettings.McpEnabled != true )
		{
			return services;
		}

		var builder = services.AddMcpServer()
			.WithHttpTransport(options => options.Stateless = true)
			.WithTools<StarskyReadTools>();

		if ( appSettings.McpAllowWrite == true )
		{
			builder.WithTools<StarskyWriteTools>();
		}

		return services;
	}

	/// <summary>
	///     Map the endpoint, always behind authentication (a login, basic auth or NoAccountLocalhost)
	/// </summary>
	public static void MapStarskyMcp(this IEndpointRouteBuilder endpoints,
		AppSettings? appSettings, string? rateLimitPolicy = null)
	{
		if ( appSettings?.McpEnabled != true )
		{
			return;
		}

		var convention = endpoints.MapMcp(Route).RequireAuthorization();
		if ( rateLimitPolicy != null )
		{
			convention.RequireRateLimiting(rateLimitPolicy);
		}
	}
}

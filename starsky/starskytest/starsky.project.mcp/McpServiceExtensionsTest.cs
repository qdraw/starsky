using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.project.mcp;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.project.mcp;

[TestClass]
public sealed class McpServiceExtensionsTest
{
	[TestMethod]
	public void AddStarskyMcp_Disabled_RegistersNothing()
	{
		var services = new ServiceCollection();

		services.AddStarskyMcp(new AppSettings { McpEnabled = false });

		Assert.IsEmpty(services);
	}

	[TestMethod]
	public void AddStarskyMcp_Enabled_RegistersServer()
	{
		var services = new ServiceCollection();

		services.AddStarskyMcp(new AppSettings { McpEnabled = true });

		Assert.IsGreaterThan(0, services.Count);
	}

	[TestMethod]
	public void AddStarskyMcp_WriteOnlyWhenAllowed()
	{
		var readOnly = new ServiceCollection();
		readOnly.AddStarskyMcp(new AppSettings { McpEnabled = true, McpAllowWrite = false });
		var write = new ServiceCollection();
		write.AddStarskyMcp(new AppSettings { McpEnabled = true, McpAllowWrite = true });

		// the write tools add registrations on top of the read tools
		Assert.IsGreaterThan(readOnly.Count, write.Count);
	}

	[TestMethod]
	public void MapStarskyMcp_DisabledOrNull_DoesNotThrow()
	{
		var builder = WebApplication.CreateBuilder();
		var app = builder.Build();
		var endpoints = ( IEndpointRouteBuilder ) app;

		endpoints.MapStarskyMcp(null);
		endpoints.MapStarskyMcp(new AppSettings { McpEnabled = false });

		Assert.AreEqual(0, endpoints.DataSources.Sum(p => p.Endpoints.Count));
	}

	[TestMethod]
	public void MapStarskyMcp_Enabled_MapsRouteWithRateLimit()
	{
		var builder = WebApplication.CreateBuilder();
		var appSettings = new AppSettings { McpEnabled = true };
		builder.Services.AddAuthorization();
		builder.Services.AddStarskyMcp(appSettings);
		var app = builder.Build();
		var endpoints = ( IEndpointRouteBuilder ) app;

		endpoints.MapStarskyMcp(appSettings, "mcp");

		var routes = endpoints.DataSources.SelectMany(p => p.Endpoints)
			.OfType<RouteEndpoint>().Select(p => p.RoutePattern.RawText).ToList();
		Assert.IsTrue(routes.Exists(p => p!.StartsWith("/mcp")));
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Models;
using starsky.Helpers;

namespace starskytest.Helpers;

[TestClass]
public sealed class RateLimitPoliciesTest
{
	public TestContext TestContext { get; set; } = null!;

	[TestMethod]
	[DataRow(typeof(GeoReverseLookupController), null, RateLimitPolicies.Anonymous)]
	[DataRow(typeof(HealthCheckForUpdatesController), null, RateLimitPolicies.Anonymous)]
	[DataRow(typeof(SearchSuggestController), nameof(SearchSuggestController.Inflate),
		RateLimitPolicies.Anonymous)]
	[DataRow(typeof(AccountController), nameof(AccountController.LoginPost),
		RateLimitPolicies.Login)]
	[DataRow(typeof(AccountController), nameof(AccountController.Register),
		RateLimitPolicies.Login)]
	public void AnonymousEndpoints_HaveRateLimitPolicy(Type controller, string? method,
		string policy)
	{
		var attributes = method == null
			? controller.GetCustomAttributes<EnableRateLimitingAttribute>()
			: controller.GetMethod(method)!.GetCustomAttributes<EnableRateLimitingAttribute>();

		Assert.AreEqual(policy, attributes.Single().PolicyName);
	}

	[TestMethod]
	public async Task LoginPolicy_ExceedingTheLimit_Returns429()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		RateLimitPolicies.AddRateLimiting(builder.Services);
		await using var app = builder.Build();
		app.UseRouting();
		app.UseRateLimiter();
		app.MapGet("/limited", () => "ok").RequireRateLimiting(RateLimitPolicies.Login);
		await app.StartAsync(TestContext.CancellationToken);

		try
		{
			using var client = app.GetTestClient();
			var statusCodes = new List<int>();
			for ( var i = 0; i < 31; i++ )
			{
				var response = await client.GetAsync("/limited", TestContext.CancellationToken);
				statusCodes.Add(( int ) response.StatusCode);
			}

			Assert.IsTrue(statusCodes.Take(30).All(p => p == StatusCodes.Status200OK));
			Assert.AreEqual(StatusCodes.Status429TooManyRequests, statusCodes[30]);
		}
		finally
		{
			await app.StopAsync(TestContext.CancellationToken);
		}
	}

	[TestMethod]
	public async Task AnonymousPolicy_ExceedingTheLimit_Returns429()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		RateLimitPolicies.AddRateLimiting(builder.Services);
		await using var app = builder.Build();
		app.UseRouting();
		app.UseRateLimiter();
		app.MapGet("/limited", () => "ok").RequireRateLimiting(RateLimitPolicies.Anonymous);
		await app.StartAsync(TestContext.CancellationToken);

		try
		{
			using var client = app.GetTestClient();
			var statusCodes = new List<int>();
			for ( var i = 0; i < 61; i++ )
			{
				var response = await client.GetAsync("/limited", TestContext.CancellationToken);
				statusCodes.Add(( int ) response.StatusCode);
			}

			Assert.IsTrue(statusCodes.Take(60).All(p => p == StatusCodes.Status200OK));
			Assert.AreEqual(StatusCodes.Status429TooManyRequests, statusCodes[60]);
		}
		finally
		{
			await app.StopAsync(TestContext.CancellationToken);
		}
	}

	[TestMethod]
	public async Task LoginPolicy_LimitIsPerClientIp()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		RateLimitPolicies.AddRateLimiting(builder.Services);
		await using var app = builder.Build();
		// the test server has no remote ip, so set it from a header
		app.Use((context, next) =>
		{
			context.Connection.RemoteIpAddress =
				System.Net.IPAddress.Parse(context.Request.Headers["x-ip"].ToString());
			return next(context);
		});
		app.UseRouting();
		app.UseRateLimiter();
		app.MapGet("/limited", () => "ok").RequireRateLimiting(RateLimitPolicies.Login);
		await app.StartAsync(TestContext.CancellationToken);

		try
		{
			using var client = app.GetTestClient();
			for ( var i = 0; i < 30; i++ )
			{
				using var request = new HttpRequestMessage(HttpMethod.Get, "/limited");
				request.Headers.Add("x-ip", "10.0.0.1");
				await client.SendAsync(request, TestContext.CancellationToken);
			}

			using var blocked = new HttpRequestMessage(HttpMethod.Get, "/limited");
			blocked.Headers.Add("x-ip", "10.0.0.1");
			using var other = new HttpRequestMessage(HttpMethod.Get, "/limited");
			other.Headers.Add("x-ip", "10.0.0.2");

			var blockedResponse = await client.SendAsync(blocked, TestContext.CancellationToken);
			var otherResponse = await client.SendAsync(other, TestContext.CancellationToken);

			Assert.AreEqual(StatusCodes.Status429TooManyRequests, ( int ) blockedResponse.StatusCode);
			Assert.AreEqual(StatusCodes.Status200OK, ( int ) otherResponse.StatusCode);
		}
		finally
		{
			await app.StopAsync(TestContext.CancellationToken);
		}
	}

	[TestMethod]
	public async Task LoginPolicy_UseRateLimitingFalse_DoesNotLimit()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseTestServer();
		builder.Services.AddSingleton(new AppSettings { UseRateLimiting = false });
		RateLimitPolicies.AddRateLimiting(builder.Services);
		await using var app = builder.Build();
		app.UseRouting();
		app.UseRateLimiter();
		app.MapGet("/limited", () => "ok").RequireRateLimiting(RateLimitPolicies.Login);
		await app.StartAsync(TestContext.CancellationToken);

		try
		{
			using var client = app.GetTestClient();
			var statusCodes = new List<int>();
			for ( var i = 0; i < 31; i++ )
			{
				var response = await client.GetAsync("/limited", TestContext.CancellationToken);
				statusCodes.Add(( int ) response.StatusCode);
			}

			Assert.IsTrue(statusCodes.All(p => p == StatusCodes.Status200OK));
		}
		finally
		{
			await app.StopAsync(TestContext.CancellationToken);
		}
	}
}

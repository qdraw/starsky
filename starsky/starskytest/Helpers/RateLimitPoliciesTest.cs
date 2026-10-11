using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky;
using starsky.Controllers;
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
		Startup.AddRateLimiting(builder.Services);
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
}

using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.platform.Tenancy;

namespace starskytest.starsky.foundation.accountmanagement.Tenancy;

[TestClass]
public sealed class NoAccountTenantTest
{
	private sealed class StubAuthentication : IAuthenticationService
	{
		public int SignIns;

		public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
		{
			return Task.FromResult(AuthenticateResult.NoResult());
		}

		public Task ChallengeAsync(HttpContext context, string? scheme,
			AuthenticationProperties? properties)
		{
			return Task.CompletedTask;
		}

		public Task ForbidAsync(HttpContext context, string? scheme,
			AuthenticationProperties? properties)
		{
			return Task.CompletedTask;
		}

		public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal,
			AuthenticationProperties? properties)
		{
			SignIns++;
			return Task.CompletedTask;
		}

		public Task SignOutAsync(HttpContext context, string? scheme,
			AuthenticationProperties? properties)
		{
			return Task.CompletedTask;
		}
	}

	private static async Task<int> SignIns(string tenantId)
	{
		await using var setup = await TenancyTestSetup.Create();
		await using var dbContext = setup.ContextFor(tenantId);
		var authentication = new StubAuthentication();
		var tenantContext = new TenantContext();
		tenantContext.SetTenant(tenantId);

		var services = new ServiceCollection();
		services.AddSingleton<IUserManager>(setup.UserManagerFor(dbContext, tenantId));
		services.AddSingleton<ITenantContext>(tenantContext);
		services.AddSingleton<IAuthenticationService>(authentication);
		await using var provider = services.BuildServiceProvider();

		var context = new DefaultHttpContext { RequestServices = provider };
		context.Request.Path = "/";
		context.Connection.RemoteIpAddress = IPAddress.Loopback;
		context.Connection.LocalIpAddress = IPAddress.Loopback;

		await new NoAccountMiddleware(_ => Task.CompletedTask, setup.AppSettings).Invoke(context);
		return authentication.SignIns;
	}

	[TestMethod]
	public async Task Localhost_DefaultTenant_SignsInTheNoAccountUser()
	{
		Assert.AreEqual(1, await SignIns("default"));
	}

	[TestMethod]
	public async Task Localhost_OtherTenant_DoesNotSignIn()
	{
		// the no-account user would get a role in a tenant it was never added to
		Assert.AreEqual(0, await SignIns("acme"));
	}
}

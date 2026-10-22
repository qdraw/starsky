using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Middleware;

/// <summary>
///     The auth cookie lives 60 days and there is no security stamp. The only server-side check
///     that the user still exists is this middleware, and it only runs for five paths.
/// </summary>
[TestClass]
public sealed class CheckIfAccountExistMiddlewareDeletedUserTest : DatabaseTest
{
	private ServiceProvider BuildServices()
	{
		var services = new ServiceCollection();
		services.AddSingleton(DbContext);
		services.AddSingleton<AppSettings>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();
		services.AddSingleton<IUserManager, UserManager>();
		services.AddAuthentication(o =>
		{
			o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			o.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		}).AddCookie();
		services.AddLogging();
		return services.BuildServiceProvider();
	}

	private static ClaimsPrincipal DeletedUserPrincipal()
	{
		var identity = new ClaimsIdentity(
			new List<Claim> { new(ClaimTypes.NameIdentifier, "987654") },
			"Cookies");
		return new ClaimsPrincipal(identity);
	}

	[TestMethod]
	[DataRow("/api/index")]
	[DataRow("/api/delete")]
	[DataRow("/api/disk/mkdir")]
	[DataRow("/api/update")]
	[DataRow("/api/export/create")]
	public async Task DeletedUserCookie_IsRejectedOnEveryApiEndpoint(string path)
	{
		var invoked = false;
		var middleware = new CheckIfAccountExistMiddleware(_ =>
		{
			invoked = true;
			return Task.CompletedTask;
		});

		var httpContext = new DefaultHttpContext
		{
			Request = { Path = path },
			RequestServices = BuildServices(),
			User = DeletedUserPrincipal()
		};

		await middleware.Invoke(httpContext);

		Assert.IsFalse(invoked, $"{path}: request of a deleted user reached the endpoint");
		Assert.AreEqual(401, httpContext.Response.StatusCode);
	}

	[TestMethod]
	[DataRow("/api/account/login")]
	[DataRow("/api/account/logout")]
	[DataRow("/api/account/register")]
	[DataRow("/starsky/api/account/login")]
	public async Task DeletedUserCookie_SignInAndOutCallsAreNotBlocked(string path)
	{
		var invoked = false;
		var middleware = new CheckIfAccountExistMiddleware(_ =>
		{
			invoked = true;
			return Task.CompletedTask;
		});

		var httpContext = new DefaultHttpContext
		{
			Request = { Path = path },
			RequestServices = BuildServices(),
			User = DeletedUserPrincipal()
		};

		await middleware.Invoke(httpContext);

		Assert.IsTrue(invoked, $"{path}: a stale cookie must not block signing in again");
	}
}

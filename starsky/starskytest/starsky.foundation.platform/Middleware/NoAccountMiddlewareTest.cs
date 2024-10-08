using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.accountmanagement.Models;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.JsonConverter;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Middleware;

[TestClass]
public sealed class NoAccountMiddlewareTest
{
	private readonly ServiceProvider _serviceProvider;

	public NoAccountMiddlewareTest()
	{
		var services = new ServiceCollection();
		// IHttpContextAccessor is required for SignInManager, and UserManager

		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase("test");
		var options = builder.Options;
		var context = new ApplicationDbContext(options);
		services.AddSingleton(context);

		services.AddSingleton<AppSettings>();

		services.AddSingleton<IUserManager, UserManager>();
		services.AddSingleton<IWebLogger, FakeIWebLogger>();


		services
			.AddAuthentication(sharedOptions =>
			{
				sharedOptions.DefaultAuthenticateScheme =
					CookieAuthenticationDefaults.AuthenticationScheme;
				sharedOptions.DefaultSignInScheme =
					CookieAuthenticationDefaults.AuthenticationScheme;
				sharedOptions.DefaultChallengeScheme =
					CookieAuthenticationDefaults.AuthenticationScheme;
			}).AddCookie();

		services.AddLogging();

		_serviceProvider = services.BuildServiceProvider();

		var httpContext = new DefaultHttpContext
		{
			Request = { Path = "/" },
			RequestServices = _serviceProvider,
			Connection =
			{
				RemoteIpAddress = IPAddress.Loopback, LocalIpAddress = IPAddress.Loopback
			}
		};

		services.AddSingleton<IHttpContextAccessor>(
			new HttpContextAccessor { HttpContext = httpContext });
		// and rebuild
		_serviceProvider = services.BuildServiceProvider();
	}

	[TestMethod]
	public async Task OnHomePageNotLoginShouldAutoLogin()
	{
		var invoked = false;
		var middleware = new NoAccountMiddleware(_ =>
		{
			invoked = true;
			return Task.CompletedTask;
		}, new AppSettings());

		var services = new ServiceCollection();
		services.AddSingleton<IUserManager, FakeUserManagerActiveUsers>();
		var serviceProvider = services.BuildServiceProvider();

		var httpContext = new DefaultHttpContext
		{
			Request = { Path = "/" },
			Connection =
			{
				RemoteIpAddress = IPAddress.Loopback, LocalIpAddress = IPAddress.Loopback
			},
			RequestServices = serviceProvider
		};
		await middleware.Invoke(httpContext);

		var userManager = serviceProvider.GetService<IUserManager>() as FakeUserManagerActiveUsers;

		Assert.IsTrue(userManager?.Users.Exists(p =>
			p.Credentials!.Any(
				credential => credential.Identifier == NoAccountMiddleware.Identifier)));

		Assert.IsTrue(invoked);
	}

	[TestMethod]
	public async Task OnHomePageNotLoginShouldAutoLogin_DemoModeOn()
	{
		var invoked = false;
		var middleware = new NoAccountMiddleware(_ =>
		{
			invoked = true;
			return Task.CompletedTask;
		}, new AppSettings { DemoUnsafeDeleteStorageFolder = true });

		var services = new ServiceCollection();
		services.AddSingleton<IUserManager, FakeUserManagerActiveUsers>();
		var serviceProvider = services.BuildServiceProvider();

		var httpContext = new DefaultHttpContext
		{
			Request = { Path = "/" },
			RequestServices = serviceProvider,
			Connection =
			{
				RemoteIpAddress = IPAddress.Parse("1.0.0.1"),
				LocalIpAddress = IPAddress.Loopback
			}
		};
		await middleware.Invoke(httpContext);

		var userManager = serviceProvider.GetService<IUserManager>() as FakeUserManagerActiveUsers;
		Assert.IsTrue(userManager?.Users.Exists(p =>
			p.Credentials?.Any(
				credential => credential.Identifier == NoAccountMiddleware.Identifier) == true));

		Assert.IsTrue(invoked);
	}

	[TestMethod]
	public async Task OnApiPageNotLoginShouldIgnore()
	{
		var invoked = false;
		var middleware = new NoAccountMiddleware(_ =>
		{
			invoked = true;
			return Task.CompletedTask;
		}, new AppSettings());

		var services = new ServiceCollection();
		services.AddSingleton<IUserManager, FakeUserManagerActiveUsers>();
		var serviceProvider = services.BuildServiceProvider();

		var httpContext = new DefaultHttpContext
		{
			Request = { Path = "/api/any" },
			Connection =
			{
				RemoteIpAddress = IPAddress.Loopback, LocalIpAddress = IPAddress.Loopback
			},
			RequestServices = serviceProvider
		};
		await middleware.Invoke(httpContext);

		var userManager = serviceProvider.GetService<IUserManager>() as FakeUserManagerActiveUsers;
		Assert.IsFalse(userManager?.Users.Exists(p =>
			p.Credentials?.Any(
				credential => credential.Identifier == NoAccountMiddleware.Identifier) == true));

		Assert.IsTrue(invoked);
	}

	[TestMethod]
	public async Task NullNotLoginShouldCreate()
	{
		var invoked = false;
		var middleware = new NoAccountMiddleware(_ =>
		{
			invoked = true;
			return Task.CompletedTask;
		}, new AppSettings());

		var services = new ServiceCollection();
		services.AddSingleton<IUserManager, FakeUserManagerActiveUsers>();
		var serviceProvider = services.BuildServiceProvider();

		var httpContext = new DefaultHttpContext
		{
			Connection =
			{
				RemoteIpAddress = IPAddress.Loopback, LocalIpAddress = IPAddress.Loopback
			},
			RequestServices = serviceProvider
			// Missing Path
		};

		await middleware.Invoke(httpContext);

		var userManager = serviceProvider.GetService<IUserManager>() as FakeUserManagerActiveUsers;

		Assert.IsTrue(userManager?.Users.Exists(p =>
			p.Credentials?.Any(
				credential => credential.Identifier == NoAccountMiddleware.Identifier) == true));

		Assert.IsTrue(invoked);
	}

	[TestMethod]
	public async Task HasClaim_AndAuthenticated()
	{
		var invoked = false;
		var middleware = new NoAccountMiddleware(_ =>
		{
			invoked = true;
			return Task.CompletedTask;
		}, new AppSettings());

		var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
		var userManager = _serviceProvider.GetRequiredService<IUserManager>();

		var result =
			await userManager.SignUpAsync("test", "email", NoAccountMiddleware.Identifier, "test");

		await userManager.SignIn(httpContextAccessor.HttpContext!, result.User);

		await middleware.Invoke(httpContextAccessor.HttpContext!);

		Assert.IsTrue(invoked);
		Assert.IsTrue(httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated);
	}

	[TestMethod]
	public async Task OnHomePageNotLoginShouldIgnoreDueOffNetwork()
	{
		var invoked = false;
		var middleware = new NoAccountMiddleware(_ =>
		{
			invoked = true;
			return Task.CompletedTask;
		}, new AppSettings());

		var services = new ServiceCollection();
		services.AddSingleton<IUserManager, FakeUserManagerActiveUsers>();
		var serviceProvider = services.BuildServiceProvider();

		var httpContext = new DefaultHttpContext
		{
			Request = { Path = "/" },
			Connection =
			{
				RemoteIpAddress = IPAddress.Parse("8.8.8.8"),
				LocalIpAddress = IPAddress.Loopback
			},
			RequestServices = serviceProvider
		};
		await middleware.Invoke(httpContext);

		var userManager = serviceProvider.GetService<IUserManager>() as FakeUserManagerActiveUsers;
		// false due off network
		Assert.IsFalse(userManager!.Users.Exists(p =>
			p.Credentials!.Any(
				credential => credential.Identifier == NoAccountMiddleware.Identifier)));

		Assert.IsTrue(invoked);
	}

	[TestMethod]
	public async Task CreateOrUpdateNewUsers_NewUser()
	{
		var services = new ServiceCollection();
		services.AddSingleton<IUserManager, FakeUserManagerActiveUsers>();
		var serviceProvider = services.BuildServiceProvider();

		var userManager = serviceProvider.GetRequiredService<IUserManager>();

		await NoAccountMiddleware.CreateOrUpdateNewUsers(userManager);

		var test = userManager.GetUser("email", NoAccountMiddleware.Identifier);

		Assert.IsNotNull(test);
		Assert.AreEqual(NoAccountMiddleware.Identifier, test.Credentials!.First().Identifier);
		Assert.AreEqual(IterationCountType.Iterate100KSha256,
			test.Credentials!.First().IterationCount);
	}

	[DataTestMethod] // [Theory]
	[DataRow(IterationCountType.IterateLegacySha1)]
	[DataRow(IterationCountType.Iterate100KSha256)]
	public async Task CreateOrUpdateNewUsers_UpgradeUser(IterationCountType iterationCountType)
	{
		var userManager = new FakeIUserManger(new UserOverviewModel
		{
			Users =
			[
				new User
				{
					Credentials = new List<Credential>
					{
						new()
						{
							Identifier = NoAccountMiddleware.Identifier,
							CredentialType = new CredentialType { Code = "email" },
							CredentialTypeId = 0,
							IterationCount = iterationCountType,
							Secret = "",
							Extra = "",
							Id = 0
						}
					}
				}
			]
		});
		var beforeCredential = userManager.GetCredentialsByUserId(0).CloneViaJson();

		await userManager.SignUpAsync(string.Empty, "email", NoAccountMiddleware.Identifier,
			"test");

		await NoAccountMiddleware.CreateOrUpdateNewUsers(userManager);

		var test = userManager.GetUser("email", NoAccountMiddleware.Identifier);

		Assert.IsNotNull(test);
		Assert.AreEqual(NoAccountMiddleware.Identifier, test.Credentials!.First().Identifier);
		Assert.AreEqual(IterationCountType.Iterate100KSha256,
			test.Credentials!.First().IterationCount);

		if ( iterationCountType == IterationCountType.Iterate100KSha256 )
		{
			Assert.AreEqual(beforeCredential?.Secret,
				test.Credentials!.FirstOrDefault()!.Secret);
		}
		else
		{
			Assert.AreNotEqual(beforeCredential?.Secret,
				test.Credentials!.FirstOrDefault()!.Secret);
		}
	}
}

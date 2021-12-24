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
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.database.Data;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.platform.Middleware
{
	[TestClass]
	public class NoAccountLocalhostMiddlewareTest
	{
		private readonly ServiceProvider _serviceProvider;

		public NoAccountLocalhostMiddlewareTest()
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

			
			services
				.AddAuthentication(sharedOptions =>
				{
					sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					sharedOptions.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				}).AddCookie();

			services.AddLogging();
			
			_serviceProvider = services.BuildServiceProvider();

			var httpContext = new DefaultHttpContext
			{
				Request = { Path = "/"},
				RequestServices = _serviceProvider,
				Connection = { RemoteIpAddress = IPAddress.Loopback, LocalIpAddress = IPAddress.Loopback}
			};
			
			services.AddSingleton<IHttpContextAccessor>(
				new HttpContextAccessor()
				{
					HttpContext = httpContext,
				});
			// and rebuild
			_serviceProvider = services.BuildServiceProvider();
		}
		
		[TestMethod]
		public async Task OnHomePageNotLoginShouldAutoLogin()
		{
			var invoked = false;
			var middleware = new NoAccountLocalhostMiddleware(next:
				(_) =>
				{
					invoked = true;
					return Task.FromResult(0);
				});
			
			var services = new ServiceCollection();
			services.AddSingleton<IUserManager, FakeUserManagerActiveUsers>();
			var serviceProvider = services.BuildServiceProvider();
			
			var httpContext = new DefaultHttpContext
			{
				Request =
				{
					Path = "/"
				},
				Connection = { RemoteIpAddress = IPAddress.Loopback, LocalIpAddress = IPAddress.Loopback},
				RequestServices = serviceProvider
			};
			await middleware.Invoke(httpContext);

			var userManager = serviceProvider.GetService<IUserManager>() as FakeUserManagerActiveUsers;
			Assert.IsTrue(userManager.Users.Any(p => p.Credentials.Any(p => p.Identifier == NoAccountLocalhostMiddleware.Identifier)));
			
			Assert.IsTrue(invoked);
		}
		
		[TestMethod]
		public async Task OnApiPageNotLoginShouldIgnore()
		{
			var invoked = false;
			var middleware = new NoAccountLocalhostMiddleware(next:
				(_) =>
				{
					invoked = true;
					return Task.FromResult(0);
				});
			
			var services = new ServiceCollection();
			services.AddSingleton<IUserManager, FakeUserManagerActiveUsers>();
			var serviceProvider = services.BuildServiceProvider();
			
			var httpContext = new DefaultHttpContext
			{
				Request =
				{
					Path = "/api/any"
				},
				Connection = { RemoteIpAddress = IPAddress.Loopback, LocalIpAddress = IPAddress.Loopback},
				RequestServices = serviceProvider
			};
			await middleware.Invoke(httpContext);

			var userManager = serviceProvider.GetService<IUserManager>() as FakeUserManagerActiveUsers;
			Assert.IsFalse(userManager.Users.Any(p => p.Credentials.Any(p => p.Identifier == NoAccountLocalhostMiddleware.Identifier)));
			
			Assert.IsTrue(invoked);
		}
		
		[TestMethod]
		public async Task NullNotLoginShouldCreate()
		{
			var invoked = false;
			var middleware = new NoAccountLocalhostMiddleware(next:
				(_) =>
				{
					invoked = true;
					return Task.FromResult(0);
				});
			
			var services = new ServiceCollection();
			services.AddSingleton<IUserManager, FakeUserManagerActiveUsers>();
			var serviceProvider = services.BuildServiceProvider();
			
			var httpContext = new DefaultHttpContext
			{
				Connection = { RemoteIpAddress = IPAddress.Loopback, LocalIpAddress = IPAddress.Loopback},
				RequestServices = serviceProvider
				// Missing Path
			};
			await middleware.Invoke(httpContext);

			var userManager = serviceProvider.GetService<IUserManager>() as FakeUserManagerActiveUsers;
			Assert.IsTrue(userManager.Users.Any(p => p.Credentials.Any(p => p.Identifier == NoAccountLocalhostMiddleware.Identifier)));
			
			Assert.IsTrue(invoked);
		}
		
		[TestMethod]
		public async Task HasClaim_AndAuthenticated()
		{
			var invoked = false;
			var middleware = new NoAccountLocalhostMiddleware(next:
				(_) =>
				{
					invoked = true;
					return Task.FromResult(0);
				});

			var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
			var userManager = _serviceProvider.GetRequiredService<IUserManager>();

			var result = await userManager.SignUpAsync("test", "email", NoAccountLocalhostMiddleware.Identifier, "test");
			
			await userManager.SignIn(httpContextAccessor.HttpContext, result.User);
			
			await middleware.Invoke(httpContextAccessor.HttpContext);
			
			Assert.IsTrue(invoked);
			Assert.IsTrue(httpContextAccessor.HttpContext.User.Identity.IsAuthenticated);

		}
		
		[TestMethod]
		public async Task OnHomePageNotLoginShouldIgnoreDueOffNetwork()
		{
			var invoked = false;
			var middleware = new NoAccountLocalhostMiddleware(next:
				(_) =>
				{
					invoked = true;
					return Task.FromResult(0);
				});
			
			var services = new ServiceCollection();
			services.AddSingleton<IUserManager, FakeUserManagerActiveUsers>();
			var serviceProvider = services.BuildServiceProvider();
			
			var httpContext = new DefaultHttpContext
			{
				Request =
				{
					Path = "/"
				},
				Connection = { RemoteIpAddress = IPAddress.Parse("8.8.8.8"), LocalIpAddress = IPAddress.Loopback},
				RequestServices = serviceProvider
			};
			await middleware.Invoke(httpContext);

			var userManager = serviceProvider.GetService<IUserManager>() as FakeUserManagerActiveUsers;
			// false due off network
			Assert.IsFalse(userManager.Users.Any(p => p.Credentials.Any(p => p.Identifier == NoAccountLocalhostMiddleware.Identifier)));
			
			Assert.IsTrue(invoked);
		}
		
	}
}

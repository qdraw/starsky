using System.Collections.Generic;
using System.Security.Claims;
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

namespace starskytest.starsky.foundation.platform.Middleware
{
	[TestClass]
	public class CheckIfAccountExistMiddlewareTest
	{
		private readonly ServiceProvider _serviceProvider;

		public CheckIfAccountExistMiddlewareTest()
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
				Request = { Path = "/api/index"},
				RequestServices = _serviceProvider
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
		public async Task NotLoggedIn()
		{
			var invoked = false;
			var middleware = new CheckIfAccountExistMiddleware(next:
				(_) =>
				{
					invoked = true;
					return Task.FromResult(0);
				});
			
			var httpContext = new DefaultHttpContext
			{
				Request =
				{
					Path = "/api/index"
				}
			};
			await middleware.Invoke(httpContext);
			Assert.IsTrue(invoked);
		}
		
		[TestMethod]
		public async Task HasClaim_AndAuthenticated()
		{
			var invoked = false;
			var middleware = new CheckIfAccountExistMiddleware(next:
				(_) =>
				{
					invoked = true;
					return Task.FromResult(0);
				});

			var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
			var userManager = _serviceProvider.GetRequiredService<IUserManager>();

			var result = await userManager.SignUpAsync("test", "email", "test", "test");
			
			await userManager.SignIn(httpContextAccessor.HttpContext, result.User);
			
			await middleware.Invoke(httpContextAccessor.HttpContext);
			
			Assert.IsTrue(invoked);
			Assert.IsTrue(httpContextAccessor.HttpContext.User.Identity.IsAuthenticated);

		}
		
				
		[TestMethod]
		public async Task HasClaim_AndAuthenticated_ButRemoved()
		{
			var invoked = false;
			var middleware = new CheckIfAccountExistMiddleware(next:
				(_) =>
				{
					invoked = true;
					return Task.FromResult(0);
				});

			var httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
			var userManager = _serviceProvider.GetRequiredService<IUserManager>();

			var result = await userManager.SignUpAsync("test", "email", "test", "test");

			await userManager.SignIn(httpContextAccessor.HttpContext, result.User);

			// and remove user
			await userManager.RemoveUser("email", "test");

			
			await middleware.Invoke(httpContextAccessor.HttpContext);
			
			Assert.IsFalse(invoked);
			Assert.AreEqual(401,httpContextAccessor.HttpContext.Response.StatusCode);
		}

		[TestMethod]
		public void GetUserTableIdFromClaims_NameIdentifierNull()
		{
			var fromClaims = CheckIfAccountExistMiddleware.GetUserTableIdFromClaims(
				new DefaultHttpContext{User = new ClaimsPrincipal()});
			Assert.AreEqual(0,fromClaims);
		}
		
		[TestMethod]
		public void GetUserTableIdFromClaims_Valid()
		{
			var userId = "2";
			var httpContext = new DefaultHttpContext();
			var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
			httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
			
			var fromClaims = CheckIfAccountExistMiddleware.GetUserTableIdFromClaims(httpContext);
			Assert.AreEqual(2,fromClaims);
		}
	}
}

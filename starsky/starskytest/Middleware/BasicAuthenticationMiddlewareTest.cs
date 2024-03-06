using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.Middleware
{
	[TestClass]
	public sealed class BasicAuthenticationMiddlewareTest
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly Task _onNextResult = Task.FromResult(0);
		private readonly RequestDelegate _onNext;
		private int _requestId;
        
		public BasicAuthenticationMiddlewareTest()
		{
			var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
			services.AddOptions();
			services
				.AddDbContext<ApplicationDbContext>(b =>
					b.UseInMemoryDatabase("test1234").UseInternalServiceProvider(efServiceProvider));
			
			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.AddMvc();
			services.AddSingleton<IAuthenticationService, NoOpAuth>();
			services.AddSingleton<IUserManager, UserManager>();
			services.AddSingleton<AppSettings, AppSettings>();
			services.AddSingleton<IWebLogger, FakeIWebLogger>();

			services.AddLogging();

			// IHttpContextAccessor is required for SignInManager, and UserManager
			var context = new DefaultHttpContext();
			services.AddSingleton<IHttpContextAccessor>(
				new HttpContextAccessor()
				{
					HttpContext = context,
				});

			_serviceProvider = services.BuildServiceProvider();
            
			_onNext = _ =>
			{
				Interlocked.Increment(ref _requestId);
				return _onNextResult;
			};
		}

		[TestMethod]
		public async Task BasicAuthenticationMiddlewareLoginTest()
		{
	        
			// Arrange
			var iUserManager = _serviceProvider.GetRequiredService<IUserManager>();
			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
			if ( httpContext == null )
			{
				throw new WebException("missing httpContext");
			}
            
			const string userId = "TestUserA";
			var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
			httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            
			httpContext.RequestServices = _serviceProvider;
			
			// Arange > new account

			await iUserManager.SignUpAsync("test", "email", "test", "test");

			// base64 dGVzdDp0ZXN0 > test:test
			httpContext.Request.Headers.Authorization = "Basic dGVzdDp0ZXN0";
                
			// Call the middleware app
			var basicAuthMiddleware = new BasicAuthenticationMiddleware(_onNext);
			await basicAuthMiddleware.Invoke(httpContext);
            
			Assert.IsTrue(httpContext.User.Identity?.IsAuthenticated);

		}
        
        
	}
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
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

namespace starskytest.Middleware
{

	[TestClass]
	public sealed class BasicAuthenticationSignInManagerTest
	{
		private readonly IUserManager _userManager;
		public IServiceProvider Services { get; set; }
        
		public BasicAuthenticationSignInManagerTest()
		{

			var serviceCollection = new ServiceCollection();
			serviceCollection
				.AddAuthentication(sharedOptions =>
				{
					sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					sharedOptions.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				}).AddCookie("Cookies");
			serviceCollection.AddLogging();
			
			Services = serviceCollection.BuildServiceProvider();
			Services.GetRequiredService<IServiceProvider>();
			Services.GetRequiredService<IAuthenticationService>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase("test");
			var options = builder.Options;
			var context = new ApplicationDbContext(options);
			_userManager = new UserManager(context,new AppSettings(), new FakeIWebLogger());
		}

		[TestMethod]
		public async Task BasicAuthenticationSignInManager_TrySignInUser_False_Test()
		{
			var httpContext = new DefaultHttpContext
			{
				HttpContext =
				{
					RequestServices = Services
				}
			};
			var authenticationHeaderValue = new BasicAuthenticationHeaderValue("Basic dGVzdDp3cm9uZw==");
			// base64 > test:wrong

			await new BasicAuthenticationSignInManager(
				httpContext, 
				authenticationHeaderValue, 
				_userManager).TrySignInUser();
			// User is not loged in
			Assert.AreEqual(false,httpContext.User.Identity?.IsAuthenticated);
		}
		
        [TestMethod]
        public async Task BasicAuthenticationSignInManager_TrySignInUser_True_Test()
        {

            await _userManager.SignUpAsync(string.Empty, "email", "log", "passs");
            
            var httpContext = new DefaultHttpContext
            {
	            HttpContext =
	            {
		            RequestServices = Services
	            }
            };
            var authService = httpContext.RequestServices.GetService<IAuthenticationService>();
            Assert.IsNotNull(authService);
            
            var authenticationHeaderValue = new BasicAuthenticationHeaderValue("Basic bG9nOnBhc3Nz");
            // base64 > log:passs
            await new BasicAuthenticationSignInManager(
                httpContext, 
                authenticationHeaderValue, 
                _userManager).TrySignInUser();
            // User is not loged in
            Assert.AreEqual(true,httpContext.User.Identity?.IsAuthenticated);
        }
	}
}

using System;
using System.Collections.Generic;
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
using starsky.Controllers;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Middleware;
using starsky.foundation.accountmanagement.Models.Account;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeMocks;

namespace starskytest.Middleware
{
    [TestClass]
    public class BasicAuthenticationMiddlewareTest
    {
        private IUserManager _userManager;
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

            // todo: breaking in net core 3.0
            // services.AddDefaultIdentity<ApplicationUser>() .AddRoles<ApplicationRole>() .AddEntityFrameworkStores<MyApplicationContext>();
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc();
            services.AddSingleton<IAuthenticationService, NoOpAuth>();
            services.AddSingleton<IUserManager, UserManager>();
            services.AddSingleton<AppSettings, AppSettings>();

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
            
            // InMemory
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("123456789");
            var options = builder.Options;
            var context2 = new ApplicationDbContext(options);
            _userManager = new UserManager(context2,new AppSettings());
            
        }

        [TestMethod]
        public async Task BasicAuthenticationMiddlewareLoginTest()
        {
	        
            // Arrange
            var userId = "TestUserA";
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };

            var iUserManager = _serviceProvider.GetRequiredService<IUserManager>();
            var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContext.RequestServices = _serviceProvider;
 
            var schemeProvider = _serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

            var controller =
	            new AccountController(_userManager, new AppSettings(), new FakeAntiforgery())
	            {
		            ControllerContext = {HttpContext = httpContext}
	            };

            // Make new account; 
            var newAccount = new RegisterViewModel
            {
                Password = "test",
                ConfirmPassword = "test",
                Email = "test"
            };
            // Arange > new account

	        iUserManager.SignUp("test", "email", "test", "test");

            // base64 dGVzdDp0ZXN0 > test:test
            httpContext.Request.Headers["Authorization"] = "Basic dGVzdDp0ZXN0";
                
            // Call the middleware app
            var basicAuthMiddleware = new BasicAuthenticationMiddleware(_onNext);
            await basicAuthMiddleware.Invoke(httpContext);
            
            Assert.AreEqual(true, httpContext.User.Identity.IsAuthenticated);

        }
        
        
    }
}

using System;
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
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Services;

namespace starskytest.Middleware
{

    [TestClass]
    public class BasicAuthenticationSignInManagerTest
    {
        private IUserManager _userManager;
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
                });
            
            Services = serviceCollection.BuildServiceProvider();
            Services.GetRequiredService<IServiceProvider>();
            
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _userManager = new UserManager(context,new AppSettings());
        }

        [TestMethod]
        public async Task BasicAuthenticationSignInManager_TrySignInUser_False_Test()
        {
            var httpContext = new DefaultHttpContext();
            var authenticationHeaderValue = new BasicAuthenticationHeaderValue("Basic dGVzdDp0ZXN0");
            // base64 > test:test

            await new BasicAuthenticationSignInManager(
                httpContext, 
                authenticationHeaderValue, 
                _userManager).TrySignInUser();
            // User is not loged in
            Assert.AreEqual(false,httpContext.User.Identity.IsAuthenticated);
        }
        
//        BasicAuthenticationSignInManagerTest.BasicAuthenticationSignInManager_TrySignInUser_True_Test
//    Test method BasicAuthenticationSignInManagerTest.BasicAuthenticationSignInManager_TrySignInUser_True_Test 
//        threw exception:  System.ArgumentNullException: Value cannot be null.
//        Parameter name: provider
//            at ServiceProviderServiceExtensions.GetRequiredService[T](IServiceProvider provider)
//            at AuthenticationHttpContextExtensions.SignInAsync(HttpContext context, String scheme, 
//            ClaimsPrincipal principal, AuthenticationProperties properties)
//            at UserManager.<SignIn>d__11.MoveNext() in /starsky/Services/UserManager.cs:line 203
//        --- End of stack trace from previous location where exception was thrown ---
//       
//        [TestMethod]
//        public async Task BasicAuthenticationSignInManager_TrySignInUser_True_Test()
//        {
//
//            _userManager.SignUp(string.Empty, "email", "log", "passs");
//            
//            var httpContext = new DefaultHttpContext();
//            var authenticationHeaderValue = new BasicAuthenticationHeaderValue("Basic bG9nOnBhc3Nz");
//            // base64 > log:passs
//            await new BasicAuthenticationSignInManager(
//                httpContext, 
//                authenticationHeaderValue, 
//                _userManager).TrySignInUser();
//            // User is not loged in
//            Assert.AreEqual(true,httpContext.User.Identity.IsAuthenticated);
//        }
    }
}

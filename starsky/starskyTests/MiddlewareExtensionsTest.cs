using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Data;
using starsky.Middleware;
using starsky.Models;

namespace starskytests
{

    [TestClass]
    public class MiddlewareExtensionsTest
    {
//        private readonly UserManager<ApplicationUser> _userManager;

        
//            var httpContext = new DefaultHttpContext();
//            
//            var provider = new ServiceCollection()
//                .AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("starsky"))
//                .AddIdentity<ApplicationUser, IdentityRole>(options =>
//                {
//                    // Password settings
//                    options.Password.RequireDigit = false;
//                    options.Password.RequiredLength = 10;
//                    options.Password.RequiredUniqueChars = 0;
//                    options.Password.RequireLowercase = false;
//                    options.Password.RequireNonAlphanumeric = false;
//                    options.Password.RequireUppercase = false;
//                })
//                .AddEntityFrameworkStores<ApplicationDbContext>()
//                .AddDefaultTokenProviders();
//            
//            _userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
//            
//        }
        
        
        [TestMethod]
        public async Task MiddlewareExtensionsBasicAuthenticationMiddlewareNotSignedIn()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var authMiddleware = new BasicAuthenticationMiddleware(next: (innerHttpContext) => Task.FromResult(0));

            // Act
            await authMiddleware.Invoke(httpContext);
        }
        
        
//        [TestMethod]
//        public async Task MiddlewareExtensionsBasicAuthenticationMiddlewareSignedIn()
//        {
////            // Arrange
////            var httpContext = new DefaultHttpContext();
////            var authMiddleware = new BasicAuthenticationMiddleware(next: (innerHttpContext) => Task.FromResult(0));
//
//            var httpContext = new DefaultHttpContext();
//
//            var services = new ServiceCollection()
////                .AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("starsky"))
//                .AddIdentity<ApplicationUser, IdentityRole>(options =>
//                {
//                    // Password settings
//                    options.Password.RequireDigit = false;
//                    options.Password.RequiredLength = 10;
//                    options.Password.RequiredUniqueChars = 0;
//                    options.Password.RequireLowercase = false;
//                    options.Password.RequireNonAlphanumeric = false;
//                    options.Password.RequireUppercase = false;
//                })
//                .AddEntityFrameworkStores<ApplicationDbContext>()
//                .AddDefaultTokenProviders();
//            
//            var serviceProvider = services..BuildServiceProvider();
//            
//            serviceProvider.Services.
////            var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
//
//            var user = new ApplicationUser { UserName = "test", Email = "test" };
//            var result = await userManager.CreateAsync(user, "model.Password");
//            
//            
//            var t = await userManager.FindByNameAsync("test");
//
//            
//            Console.WriteLine();
//            // Act
////            await authMiddleware.Invoke(httpContext);
//        }
        
    }
}
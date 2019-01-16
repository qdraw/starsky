using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Services;
using starskycore.Data;
using starskycore.Interfaces;
using starskycore.Middleware;
using starskycore.Services;
using starskytests.FakeMocks;

namespace starskytests.Middleware
{

    [TestClass]
    public class MiddlewareExtensionsTest
    {
        private IUserManager _userManager;


		public MiddlewareExtensionsTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _userManager = new UserManager(context);
        }

        
        
        [TestMethod]
        public async Task MiddlewareExtensionsBasicAuthenticationMiddlewareNotSignedIn()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var authMiddleware = new BasicAuthenticationMiddleware(next: (innerHttpContext) => Task.FromResult(0));

            // Act
            await authMiddleware.Invoke(httpContext,_userManager);
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

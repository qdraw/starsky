
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.Data;
using starsky.Interfaces;
using starsky.Middleware;
using starsky.Services;
using starsky.ViewModels.Account;

namespace starskytests.Controller
{
    [TestClass]
    public class AccountControllerTest
    {
        private IUserManager _userManager;
        
        public AccountControllerTest()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test");
            var options = builder.Options;
            var context = new ApplicationDbContext(options);
            _userManager = new UserManager(context);
        }

        
        
        [TestMethod]
        public async Task AccountController_Register_newAccount_Test()
        {
            // Arrange
            AccountController controller = new AccountController(_userManager)
            {
                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
            };
            var newAccount = new RegisterViewModel
            {
                Password = "test12345678",
                ConfirmPassword = "test12345678",
                Email = "test@dion.local"
            };
            
            // Act
            var registerResult = await controller.Register(newAccount,true,string.Empty) as JsonResult;
            Assert.AreNotEqual(registerResult,null);
            var accountCreatedString = registerResult.Value as string;
            Assert.AreEqual("Account Created",accountCreatedString);

        }
        
        
        
        
        
        
        
        
//        private UserManager<ApplicationUser> _userManager;
//        private SignInManager<ApplicationUser> _signInManager;
//
//        public AccountControllerTest()
//        {
//            var serviceCollection = new ServiceCollection();
//            
//            serviceCollection.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("db_Test"));
//            
//            serviceCollection.AddIdentity<ApplicationUser, IdentityRole>(options =>
//                {
//                    // Password settings
//                    options.Password.RequireDigit = false;
//                    options.Password.RequiredLength = 5;
//                    options.Password.RequiredUniqueChars = 0;
//                    options.Password.RequireLowercase = false;
//                    options.Password.RequireNonAlphanumeric = false;
//                    options.Password.RequireUppercase = false;
//                })
//                .AddEntityFrameworkStores<ApplicationDbContext>()
//                .AddDefaultTokenProviders();
//            
//            _userManager = serviceCollection.BuildServiceProvider().GetService<UserManager<ApplicationUser>>();
//            _signInManager = serviceCollection.BuildServiceProvider().GetService<SignInManager<ApplicationUser>>();
//
//        }

        



        
//        [TestMethod]
//        public async Task HomeControllerIndexDetailViewTest()
//        {
//            var newAccount = new RegisterViewModel
//            {
//                Password = "test12345678",
//                ConfirmPassword = "test12345678",
//                Email = "test@dion.local"
//            };
//
//            AccountController controller = new AccountController(_userManager, _signInManager)
//            {
//                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
//            };
//            controller.ControllerContext.HttpContext.Request.Headers["device-id"] = "20317";
//            Console.WriteLine(controller.HttpContext);
//            var d = await controller.Register(newAccount) as RegisterViewModel;
//            
//            
//        }
    }
}
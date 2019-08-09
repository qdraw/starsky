using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.ViewModels.Account;
using starskycore.Data;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Models.Account;
using starskycore.Services;
using starskycore.ViewModels.Account;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
    [TestClass]
    public class AccountControllerTest2
    {
        private IUserManager _userManager;
        private readonly IServiceProvider _serviceProvider;

	    private ApplicationDbContext _dbContext;

        public AccountControllerTest2()
        {
            var efServiceProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

			// For URLS
			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
			services.AddScoped<IUrlHelper>(factory =>
			{
				var actionContext = factory.GetService<IActionContextAccessor>()
											   .ActionContext;
				return new UrlHelper(actionContext);
			});


			services.AddOptions();
            services
                .AddDbContext<ApplicationDbContext>(b =>
                    b.UseInMemoryDatabase("test123").UseInternalServiceProvider(efServiceProvider));

            // todo: breaking in net core 3.0 (AddEntityFrameworkStores)
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc();
            services.AddSingleton<IAuthenticationService, NoOpAuth>();
            services.AddSingleton<IUserManager, UserManager>();

            services.AddLogging();

            // IHttpContextAccessor is required for SignInManager, and UserManager
            var context = new DefaultHttpContext();
            services.AddSingleton<IHttpContextAccessor>(
                new HttpContextAccessor()
                {
                    HttpContext = context,
                });

            _serviceProvider = services.BuildServiceProvider();
            
            
            // InMemory
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test123");
            var options = builder.Options;
            _dbContext = new ApplicationDbContext(options);
            _userManager = new UserManager(_dbContext);
            
        }
        
        [TestMethod]
        public async Task AccountController_NoLogin_Login_And_newAccount_Test()
        {
            // Arrange
            var userId = "TestUserA";
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };

            _serviceProvider.GetRequiredService<IUserManager>();

			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContext.RequestServices = _serviceProvider;
 
            var schemeProvider = _serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

			AccountController controller = new AccountController(_userManager);
			controller.ControllerContext.HttpContext = httpContext;

			// Get context for url (netcore3)
			var routeData = new RouteData();
			routeData.Values.Add("key1", "value1");
			var actionDescriptor = new ActionDescriptor();

			var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
			controller.Url = new UrlHelper(actionContext);


			var login = new LoginViewModel
            {
                Email = "shared@dion.local",
                Password = "test"
            };
            
            // Try login > result login false
            await controller.Login(login);
            // Test login
            Assert.AreEqual(false,httpContext.User.Identity.IsAuthenticated);
            
            // Reset the model state, 
            // to avoid errors on RegisterViewModel
            // in a normal session the State is cleared after 1 request
            controller.ModelState.Clear();

            
            // Make new account; 
            var newAccount = new RegisterViewModel
            {
                Password = "test",
                ConfirmPassword = "test",
                Email = "shared@dion.local"
            };
            // Arange > new account
            await controller.Register(newAccount,true,string.Empty);
            
            // Try login again > now it must be succesfull
            await controller.Login(login);
            // Test login
            Assert.AreEqual(true,httpContext.User.Identity.IsAuthenticated);
            
            // The logout is mocked so this will not actual log it out;
            // controller.Logout() not crashing is good enough;
            controller.Logout();
            
        }

        [TestMethod]
        public async Task AccountController_Model_is_not_correct()
        {
            var controller = new AccountController(_userManager);
            var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            controller.ControllerContext.HttpContext = httpContext;

            var reg = new RegisterViewModel{Email = "test", ConfirmPassword = "1", Password = "2"};
            var actionResult = await controller.Register(reg,true) as JsonResult;
            
            Assert.AreEqual("Model is not correct", actionResult.Value as string);
        }

        [TestMethod]
        public void AccountController_LogInGet()
        {
            var controller = new AccountController(_userManager);
            controller.Login();
        }
        
        [TestMethod]
        public void AccountController_RegisterGet()
        {
            var controller = new AccountController(_userManager);
            controller.Register();
        }
     
	    [TestMethod]
	    public void AccountController_IndexGetLoginSuccesfull()
	    {
		    var user = new User() { Name = "JohnDoe", Id = 99 };
		    
		    _dbContext.Users.Add(user);
		    
		    var userManager = new UserManager(_dbContext);
		    var controller = new AccountController(userManager);

		    
		    var claims = new List<Claim>()
		    {
			    new Claim(ClaimTypes.Name, user.Name),
			    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
		    };
		    var identity = new ClaimsIdentity(claims, "Test");
		    var claimsPrincipal = new ClaimsPrincipal(identity);
		    
		    var context = new ControllerContext
		    {
			    HttpContext = new DefaultHttpContext
			    {
				    User = claimsPrincipal
			    }
		    };

		    controller.ControllerContext = context;
		    
		    controller.Index(true);
		    Assert.AreEqual(200,controller.Response.StatusCode);
		    
	    }
	    
	    [TestMethod]
	    public void AccountController_IndexGetLoginFail()
	    {
		    // Used by the warmup script
		    var controller = new AccountController(_userManager);

		    var identity = new ClaimsIdentity();
		    var claimsPrincipal = new ClaimsPrincipal(identity);
		    
		    var context = new ControllerContext
		    {
			    HttpContext = new DefaultHttpContext
			    {
				    User = claimsPrincipal
			    }
		    };

		    controller.ControllerContext = context;
		    
		    // keep 401, is used by the warmup script
		    var index = controller.Index(true) as UnauthorizedResult;
		    Assert.AreEqual(401,index.StatusCode);
		    
	    }
        
    }

}
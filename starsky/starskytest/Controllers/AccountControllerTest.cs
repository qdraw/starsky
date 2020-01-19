using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
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
    public class AccountControllerTest
    {
        private IUserManager _userManager;
        private readonly IServiceProvider _serviceProvider;

	    private ApplicationDbContext _dbContext;
	    private IAntiforgery _antiForgery;
	    private readonly AppSettings _appSettings;

	    public AccountControllerTest()
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

            _appSettings = new AppSettings();

        }

	    private ControllerContext SetTestClaimsSet(string name, string id)
	    {
		    var claims = new List<Claim>()
		    {
			    new Claim(ClaimTypes.Name, name),
			    new Claim(ClaimTypes.NameIdentifier, id),
		    };
		    var identity = new ClaimsIdentity(claims, "Test");
		    var claimsPrincipal = new ClaimsPrincipal(identity);
		    
			return new ControllerContext
		    {
			    HttpContext = new DefaultHttpContext
			    {
				    User = claimsPrincipal
			    }
		    };
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

			AccountController controller = new AccountController(_userManager,_appSettings);
			controller.ControllerContext.HttpContext = httpContext;

			// Get context for url (netcore3)
			var routeData = new RouteData();
			routeData.Values.Add("key1", "value1");
			var actionDescriptor = new ActionDescriptor();

			var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
			controller.Url = new UrlHelper(actionContext);
			// end url context

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
                Email = "shared@dion.local",
                Name = "shared@dion.local",
            };

            // Arange > new account
            controller.Register(newAccount);
            
            // Try login again > now it must be succesfull
            await controller.Login(login);
            // Test login
            Assert.AreEqual(true,httpContext.User.Identity.IsAuthenticated);
            
            // The logout is mocked so this will not actual log it out;
            // controller.Logout() not crashing is good enough;
            controller.Logout();
            
            // And clean afterwards
            var itemWithId = _dbContext.Users.FirstOrDefault(p => p.Name == newAccount.Name);
            _dbContext.Users.Remove(itemWithId);
            _dbContext.SaveChanges();
        }

        [TestMethod]
        public void AccountController_Model_is_not_correct_NoUsersActive()
        {
            var controller = new AccountController(new UserManager(_dbContext), _appSettings);
            var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            controller.ControllerContext.HttpContext = httpContext;

            var registerViewModel = new RegisterViewModel{Email = "test", ConfirmPassword = "1", Password = "2", Name = "test"};
            
            var actionResult = controller.Register(registerViewModel) as JsonResult;
            
            Assert.AreEqual("Model is not correct", actionResult.Value as string);
        }

        [TestMethod]
        public void AccountController_Model_WithUsersActive_GetRegisterPage_Forbid()
        {
	        var controller = new AccountController(new FakeUserManagerActiveUsers(), _appSettings);
	        var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
	        controller.ControllerContext.HttpContext = httpContext;

	        var registerViewModel = new RegisterViewModel{Email = "test", ConfirmPassword = "1", Password = "2", Name = "test"};
            
	        var actionResult = controller.Register(registerViewModel) as ForbidResult;

	        Assert.IsNotNull(actionResult);
	        Assert.AreEqual("Account Register page is closed",actionResult.AuthenticationSchemes.FirstOrDefault());
        }
        
        [TestMethod]
        public void AccountController_Model_WithUsersActive_GetRegisterPage_BlockedByDefault()
        {
	        var controller = new AccountController(new FakeUserManagerActiveUsers(), new AppSettings{IsAccountRegisterOpen = false});
	        var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
	        controller.ControllerContext.HttpContext = httpContext;

	        var registerViewModel = new RegisterViewModel{Email = "test", ConfirmPassword = "1", Password = "2", Name = "test"};
            
	        var actionResult = controller.Register(registerViewModel) as ForbidResult;

	        Assert.IsNotNull(actionResult);
	        Assert.AreEqual("Account Register page is closed",actionResult.AuthenticationSchemes.FirstOrDefault());
        }

        [TestMethod]
        public void AccountController_LoginContext_GetRegisterPage_AccountCreated()
        {
	        var user = new User() { Name = "JohnDoe1"};
		    
	        _dbContext.Users.Add(user);
	        _dbContext.SaveChanges();

	        var controller = new AccountController(new UserManager(_dbContext), _appSettings)
	        {
		        ControllerContext = SetTestClaimsSet(user.Name, user.Id.ToString())
	        };

	        var registerViewModel = new RegisterViewModel{Email = "test", ConfirmPassword = "test123456789", Password = "test123456789", Name = "test"};
            
	        var actionResult = controller.Register(registerViewModel) as JsonResult;

	        Assert.IsNotNull(actionResult);
	        Assert.AreEqual("Account Created",actionResult.Value);

	        // And clean afterwards
	        controller.Logout();
	        
	        var getUser = _dbContext.Users.FirstOrDefault(p => p.Name == user.Name);
	        _dbContext.Users.Remove(getUser);
	        _dbContext.SaveChanges();
        }
        
        
	    [TestMethod]
	    public void AccountController_IndexGetLoginSuccessful()
	    {
		    var user = new User() { Name = "JohnDoe"};
		    
		    _dbContext.Users.Add(user);
		    _dbContext.SaveChanges();

		    var controller = new AccountController(new UserManager(_dbContext), _appSettings)
		    {
			    ControllerContext = SetTestClaimsSet(user.Name, user.Id.ToString())
		    };

		    controller.Status();
		    Assert.AreEqual(200,controller.Response.StatusCode);
		    
		    // And clean afterwards
		    var getUser = _dbContext.Users.FirstOrDefault(p => p.Name == "JohnDoe");
		    _dbContext.Users.Remove(getUser);
		    _dbContext.SaveChanges();
	    }
	    
	    [TestMethod]
	    public void AccountController_WithActiveUsers_IndexGetLoginFail()
	    {
		    // There are users active
		    var controller = new AccountController(new FakeUserManagerActiveUsers(), _appSettings);

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
		    var index = controller.Status() as UnauthorizedObjectResult;
		    Assert.AreEqual(401,index.StatusCode);
		    
	    }

	     [TestMethod]
        public async Task AccountController_newAccount_TryToOverwrite_ButItKeepsTheSamePassword()
        {
            // Arrange
            var userId = "TestUserA";
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };

            _serviceProvider.GetRequiredService<IUserManager>();

			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContext.RequestServices = _serviceProvider;
 
			AccountController controller = new AccountController(_userManager,_appSettings);
			controller.ControllerContext.HttpContext = httpContext;
			
			// Get context for url (netcore3)
			var routeData = new RouteData();
			routeData.Values.Add("key1", "value1");
			var actionDescriptor = new ActionDescriptor();

			var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
			controller.Url = new UrlHelper(actionContext);
			// end url context
			
			var login = new LoginViewModel
            {
                Email = "try_overwrite@dion.local",
                Password = "test"
            };
            
            // Make new account; 
            var newAccount = new RegisterViewModel
            {
                Password = "test",
                ConfirmPassword = "test",
                Email = "try_overwrite@dion.local",
                Name = "try_overwrite@dion.local"
            };

            // Arange > new account
            controller.Register(newAccount);
            
            // login > it must be succesfull
            await controller.Login(login);
            // Test login
            Assert.AreEqual(true,httpContext.User.Identity.IsAuthenticated);
            
            // The logout is mocked so this will not actual log it out;
            // controller.Logout() not crashing is good enough;
            controller.Logout();
            
            var users1111 = _userManager.AllUsers();

            var newAccountDuplicate = new RegisterViewModel
            {
	            Password = "test11234567890", // DIFFERENT
	            ConfirmPassword = "test11234567890",
	            Email = "try_overwrite@dion.local",
	            Name = "should not be updated"
            };
            
            // For security reasons there is no feedback when a account already exist
            controller.Register(newAccountDuplicate);

            // Try login again > now it must be succesfull
            await controller.Login(login);
            // Test login
            Assert.AreEqual(true,httpContext.User.Identity.IsAuthenticated);
            
            // The logout is mocked so this will not actual log it out;
            // controller.Logout() not crashing is good enough;
            controller.Logout();

			// Clean afterwards            
            var user = _dbContext.Users.FirstOrDefault(p => p.Name == "try_overwrite@dion.local");
            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
        }
    }

}

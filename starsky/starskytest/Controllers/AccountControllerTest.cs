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
using starsky.foundation.database.Data;
using starsky.foundation.database.Models;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskycore.ViewModels.Account;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {
        private readonly IUserManager _userManager;
        private readonly IServiceProvider _serviceProvider;

	    private readonly ApplicationDbContext _dbContext;
	    private readonly AppSettings _appSettings;
	    private readonly FakeAntiforgery _antiForgery;

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
            
            _appSettings = new AppSettings();
            
            // InMemory
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseInMemoryDatabase("test123");
            var options = builder.Options;
            _dbContext = new ApplicationDbContext(options);
            _userManager = new UserManager(_dbContext,_appSettings);

            _antiForgery = new FakeAntiforgery();
        }

	    private ClaimsPrincipal SetTestClaimsSet(string name, string id)
	    {
		    var claims = new List<Claim>()
		    {
			    new Claim(ClaimTypes.Name, name),
			    new Claim(ClaimTypes.NameIdentifier, id),
			    new Claim("Permission", "fakePermission"),
		    };
		    var identity = new ClaimsIdentity(claims, "Test");
		    var claimsPrincipal = new ClaimsPrincipal(identity);

		    return claimsPrincipal;
	    }
        
        [TestMethod]
        public async Task AccountController_NoLogin_Login_And_newAccount_Test()
        {
            // Arrange
            var userId = "TestUserA";
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };

			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContext.RequestServices = _serviceProvider;
 
            var schemeProvider = _serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

			AccountController controller = new AccountController(_userManager,_appSettings,_antiForgery);
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
            var controller = new AccountController(new UserManager(_dbContext,_appSettings), _appSettings,_antiForgery);
            var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            controller.ControllerContext.HttpContext = httpContext;

            var registerViewModel = new RegisterViewModel{Email = "test", ConfirmPassword = "1", Password = "2", Name = "NoUsersActive"};
            
            var actionResult = controller.Register(registerViewModel) as JsonResult;
            
            Assert.AreEqual("Model is not correct", actionResult.Value as string);
        }

        [TestMethod]
        public void AccountController_Model_WithUsersActive_GetRegisterPage_Forbid()
        {
	        var controller = new AccountController(new FakeUserManagerActiveUsers(), _appSettings,_antiForgery);
	        var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
	        controller.ControllerContext.HttpContext = httpContext;

	        var registerViewModel = new RegisterViewModel{Email = "test", ConfirmPassword = "1", Password = "2", Name = "Forbid"};
            
	        var actionResult = controller.Register(registerViewModel) as JsonResult;

	        Assert.IsNotNull(actionResult);
	        Assert.AreEqual("Account Register page is closed",actionResult.Value);
        }

        [TestMethod]
        public void AccountController_ChangeSecret_NotLoggedIn()
        {
	        var controller = new AccountController(_userManager, _appSettings,_antiForgery);
	        var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
	        controller.ControllerContext.HttpContext = httpContext;

	        var changePasswordViewModel = new ChangePasswordViewModel{ Password = "oldPassword", ChangedPassword = "newPassword", ChangedConfirmPassword = "newPassword"};
	        var actionResult = controller.ChangeSecret(changePasswordViewModel) as UnauthorizedObjectResult;
			Assert.AreEqual(401,actionResult.StatusCode);
        }
        
        [TestMethod]
        public void AccountController_ChangeSecret_WrongInput()
        {
	        
	        var controller = new AccountController(_userManager, _appSettings, _antiForgery)
	        {
		        ControllerContext = {HttpContext = new DefaultHttpContext
		        {
			        User = SetTestClaimsSet("test", "1")
		        }}
	        };
	        
	        var changePasswordViewModel = new ChangePasswordViewModel{ Password = "oldPassword", ChangedPassword = "newPassword1111", ChangedConfirmPassword = "newPassword"};
            
	        var actionResult =  controller.ChangeSecret(changePasswordViewModel) as BadRequestObjectResult;
	        
	        Assert.AreEqual(400,actionResult.StatusCode);
        }

 
        [TestMethod]
        public void AccountController_ChangeSecret_PasswordChange_Success_Injected()
        {
	        var controller = new AccountController(new FakeUserManagerActiveUsers("test"), _appSettings, _antiForgery)
	        {
		        ControllerContext = {HttpContext = new DefaultHttpContext
		        {
			        User = SetTestClaimsSet("test", "99")
		        }}
	        };

	        var changePasswordViewModel = new ChangePasswordViewModel{ Password = "oldPassword", ChangedPassword = "newPassword", ChangedConfirmPassword = "newPassword"};
            
	        var actionResult =  controller.ChangeSecret(changePasswordViewModel) as JsonResult;
	        var actualResult = actionResult.Value as ChangeSecretResult;
	        
	        Assert.IsTrue(actualResult.Success);
        }

        [TestMethod]
        public void AccountController_ChangeSecret_PasswordChange_Rejected_Injected()
        {
	        var userManager = new FakeUserManagerActiveUsers("reject");

	        var controller = new AccountController(userManager, _appSettings, _antiForgery)
	        {
		        ControllerContext = {HttpContext = new DefaultHttpContext
		        {
			        User = SetTestClaimsSet("reject", "99")
		        }}
	        };

	        var changePasswordViewModel = new ChangePasswordViewModel{ Password = "oldPassword", ChangedPassword = "newPassword", ChangedConfirmPassword = "newPassword"};
            
	        var actionResult = controller.ChangeSecret(changePasswordViewModel) as UnauthorizedObjectResult;
	        Assert.AreEqual(401,actionResult.StatusCode);
        }
        

        [TestMethod]
        public void AccountController_Model_WithUsersActive_GetRegisterPage_BlockedByDefault()
        {
	        var controller = new AccountController(new FakeUserManagerActiveUsers(), new AppSettings{IsAccountRegisterOpen = false}, _antiForgery);
	        var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
	        controller.ControllerContext.HttpContext = httpContext;

	        var registerViewModel = new RegisterViewModel{Email = "test", ConfirmPassword = "1", Password = "2", Name = "blockedByDefault"};
            
	        var actionResult = controller.Register(registerViewModel) as JsonResult;

	        Assert.IsNotNull(actionResult);
	        Assert.AreEqual("Account Register page is closed",actionResult.Value);
        }

        [TestMethod]
        public void AccountController_LoginContext_GetRegisterPage_AccountCreated()
        {
	        var user = new User() { Name = "JohnDoe2"};
   
	        var controller = new AccountController(_userManager, _appSettings, _antiForgery)
	        {
		        ControllerContext = {HttpContext = new DefaultHttpContext
		        {
			        User = SetTestClaimsSet(user.Name, user.Id.ToString())
		        }}
	        };

	        var registerViewModel = new RegisterViewModel{Email = "test", ConfirmPassword = "test123456789", Password = "test123456789", Name = user.Name};
            
	        var actionResult = controller.Register(registerViewModel) as JsonResult;

	        Assert.IsNotNull(actionResult);
	        Assert.AreEqual("Account Created",actionResult.Value);
	        
	        var getUser = _dbContext.Users.FirstOrDefault(p => p.Name == user.Name);
	        _dbContext.Users.Remove(getUser);
	        _dbContext.SaveChanges();
        }
        
        
	    [TestMethod]
	    public void AccountController_IndexGetLoginSuccessful()
	    {
		    var user = new User() { Name = "JohnDoe1"};
		    
		    _dbContext.Users.Add(user);
		    _dbContext.SaveChanges();

		    var controller = new AccountController(_userManager, _appSettings, _antiForgery)
		    {
			    ControllerContext = {HttpContext = new DefaultHttpContext
			    {
				    User = SetTestClaimsSet(user.Name, user.Id.ToString())
			    }}
		    };


		    controller.Status();
		    Assert.AreEqual(200,controller.Response.StatusCode);
		    
		    // And clean afterwards
		    var getUser = _dbContext.Users.FirstOrDefault(p => p.Name == user.Name);
		    _dbContext.Users.Remove(getUser);
		    _dbContext.SaveChanges();
	    }
	    
	    [TestMethod]
	    public void AccountController_WithActiveUsers_IndexGetLoginFail()
	    {
		    // There are users active
		    var controller = new AccountController(new FakeUserManagerActiveUsers(), _appSettings, _antiForgery);

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
	    public void AccountController_LogInGet()
	    {
		    var controller = new AccountController(new FakeUserManagerActiveUsers(), _appSettings, _antiForgery);
		    controller.Login();
	    }


	     [TestMethod]
        public async Task AccountController_newAccount_TryToOverwrite_ButItKeepsTheSamePassword()
        {
            // Arrange
            var userId = "try_overwrite@dion.local";
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };

            _serviceProvider.GetRequiredService<IUserManager>();

			var httpContext = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContext.RequestServices = _serviceProvider;
            
            // needed to have httpContext.User.Identity.IsAuthenticated
            _serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

            var controller =
	            new AccountController(_userManager, _appSettings, _antiForgery)
	            {
		            ControllerContext = {HttpContext = httpContext}
	            };

            // Get context for url (netcore3)
			var routeData = new RouteData();
			routeData.Values.Add("key1", "value1");
			var actionDescriptor = new ActionDescriptor();
			var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
			controller.Url = new UrlHelper(actionContext);
			// end url context
			
			var login = new LoginViewModel
            {
                Email = userId,
                Password = "test"
            };
            
            // Make new account; 
            var newAccount = new RegisterViewModel
            {
                Password = "test",
                ConfirmPassword = "test",
                Email = userId,
                Name = userId
            };

            // Arrange > new account
            controller.Register(newAccount);
            
            // login > it must be succesfull
            await controller.Login(login);
            // Test login
            Assert.AreEqual(true,httpContext.User.Identity.IsAuthenticated);
            
            // The logout is mocked so this will not actual log it out;
            // controller.Logout() not crashing is good enough;
            controller.Logout();
            

            var newAccountDuplicate = new RegisterViewModel
            {
	            Password = "test11234567890", // DIFFERENT
	            ConfirmPassword = "test11234567890",
	            Email = userId,
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
            var user = _dbContext.Users.FirstOrDefault(p => p.Name == userId);
            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
        }

        [TestMethod]
        public void AccountController_RegisterStatus_NoAccounts()
        {
	        var controller =
		        new AccountController(_userManager, _appSettings, _antiForgery)
		        {
			        ControllerContext = {HttpContext = new DefaultHttpContext()}
		        };

	        var actionResult = controller.RegisterStatus() as JsonResult;
            
	        Assert.AreEqual("RegisterStatus open", actionResult.Value as string);
        }
        
        [TestMethod]
        public void AccountController_RegisterStatus_ActiveUsers()
        {
	        var controller =
		        new AccountController(new FakeUserManagerActiveUsers(), _appSettings, _antiForgery)
		        {
			        ControllerContext = {HttpContext = new DefaultHttpContext()}
		        };

	        var actionResult = controller.RegisterStatus() as JsonResult;
            
	        Assert.AreEqual("Account Register page is closed", actionResult.Value as string);
        }
        
        [TestMethod]
        public void AccountController_LoginStatus_NoAccounts()
        {
	        var controller =
		        new AccountController(_userManager, _appSettings, _antiForgery)
		        {
			        ControllerContext = {HttpContext = new DefaultHttpContext()}
		        };

	        var actionResult = controller.Status() as JsonResult;
            
	        Assert.AreEqual("There are no accounts, you must create an account first", actionResult.Value as string);
        }
        
        [TestMethod]
        public void Permissions()
        {

	        var claims = SetTestClaimsSet("test", "1");
	        var controller = new AccountController(_userManager, _appSettings, _antiForgery)
	        {
		        ControllerContext = {HttpContext = new DefaultHttpContext
		        {
			        User = claims
		        }}
	        };

	        var actionResult = controller.Permissions() as JsonResult;
	        var list = actionResult.Value as IEnumerable<string>;

	        var expectedPermission =
		        claims.Claims.Where(p => p.Type == "Permission").FirstOrDefault().Value;
	        Assert.AreEqual(expectedPermission,list.FirstOrDefault());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskyAdminCli.Services;
using starskycore.Data;
using starskycore.Interfaces;
using starskycore.Models;
using starskycore.Services;
using starskytest.FakeMocks;

namespace starskytest.starskyAdminCli.Services
{
	
	[TestClass]
	public class ConsoleAdminTest
	{
		private readonly IUserManager _userManager;
		private readonly IServiceProvider _serviceProvider;

		private readonly ApplicationDbContext _dbContext;
		private readonly AppSettings _appSettings;
		
		public ConsoleAdminTest()
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
					b.UseInMemoryDatabase(nameof(ConsoleAdmin)).UseInternalServiceProvider(efServiceProvider));

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
			builder.UseInMemoryDatabase(nameof(ConsoleAdmin));
			var options = builder.Options;
			_dbContext = new ApplicationDbContext(options);
			_userManager = new UserManager(_dbContext);

			_appSettings = new AppSettings();
            
		}
		
		[TestMethod]
		public void StarskyAdminCliProgramTest_UserDoesNotExist()
		{
			var console = new FakeConsoleWrapper(new List<string>
			{
				"dont@mail.me"
			});
			new ConsoleAdmin(new AppSettings(), new UserManager(_dbContext),console ).Tool();
			
			Assert.AreEqual("User dont@mail.me does not exist", console.WrittenLines.LastOrDefault());
		}
		
		[TestMethod]
		public void StarskyAdminCliProgramTest_Removed()
		{
			//
			_userManager.SignUp("test", "Email", "dont@mail1.me", "secret");
			
			var console = new FakeConsoleWrapper(new List<string>
			{
				"dont@mail1.me",
				"2"
			});
			new ConsoleAdmin(new AppSettings(), new UserManager(_dbContext),console ).Tool();
			
			Assert.AreEqual("User dont@mail1.me is removed", console.WrittenLines.LastOrDefault());
		}
	}
}

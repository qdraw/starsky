using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Data;
using starskycore.Models.Account;
using starskycore.Services;

[assembly: InternalsVisibleTo("starskytest")]
namespace starskytest.Services
{
	[TestClass]
	public class UserManagerTest
	{
		private readonly IMemoryCache _memoryCache;
		private readonly ApplicationDbContext _dbContext;

		public UserManagerTest()
		{
			var provider = new ServiceCollection()
				.AddMemoryCache()
				.BuildServiceProvider();
			_memoryCache = provider.GetService<IMemoryCache>();
            
			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			builder.UseInMemoryDatabase(nameof(UpdateService));
			var options = builder.Options;
			_dbContext = new ApplicationDbContext(options);
		}

		[TestMethod]
		public void UserManager_Test_Non_Exist_Account()
		{
			var userManager = new UserManager(_dbContext, _memoryCache);

			var result = userManager.Validate("email", "test", "test");
			Assert.AreEqual(false, result.Success);
			
		}
		
		[TestMethod]
		public void UserManager_WrongPassword()
		{
			var userManager = new UserManager(_dbContext, _memoryCache);

			userManager.SignUp("user01", "email", "test@google.com", "pass");

			var result = userManager.Validate("email", "test@google.com", "----");
			Assert.AreEqual(false, result.Success);
		}
		
		[TestMethod]
		public void UserManager_LoginPassword()
		{
			var userManager = new UserManager(_dbContext, _memoryCache);

			userManager.SignUp("user01", "email", "dont@mail.us", "pass");

			var result = userManager.Validate("email", "dont@mail.us", "pass");
			Assert.AreEqual(true, result.Success);
		}

		[TestMethod]
		public void UserManager_NoPassword_ExistingAccount()
		{
			var userManager = new UserManager(_dbContext, _memoryCache);
			userManager.SignUp("user02", "email", "dont@mail.us", "pass");
			
			var result = userManager.Validate("email", "dont@mail.us", null);
			Assert.AreEqual(false, result.Success);
			
		}
		
		
		[TestMethod]
		public void UserManager_AllUsers_testCache()
		{
			var userManager = new UserManager(_dbContext, _memoryCache);
			userManager.AddUserToCache(new User{Name = "cachedUser"});

			var user = userManager.AllUsers().FirstOrDefault(p => p.Name == "cachedUser");
			Assert.IsNotNull(user);
		}

	}
}

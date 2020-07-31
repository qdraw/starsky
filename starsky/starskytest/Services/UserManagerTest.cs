using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;
using starskycore.Services;

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
			builder.UseInMemoryDatabase(nameof(MetaUpdateService));
			var options = builder.Options;
			_dbContext = new ApplicationDbContext(options);
		}

		[TestMethod]
		public void UserManager_Test_Non_Exist_Account()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);

			var result = userManager.Validate("email", "test", "test");
			Assert.AreEqual(false, result.Success);
		}
		
		[TestMethod]
		public void UserManager_WrongPassword()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);

			userManager.SignUp("user01", "email", "test@google.com", "pass");

			var result = userManager.Validate("email", "test@google.com", "----");
			Assert.AreEqual(false, result.Success);
		}
		
		[TestMethod]
		public void UserManager_LoginPassword()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(),_memoryCache);

			userManager.SignUp("user01", "email", "dont@mail.us", "pass");

			var result = userManager.Validate("email", "dont@mail.us", "pass");
			Assert.AreEqual(true, result.Success);
		}
		
		[TestMethod]
		public void UserManager_ChangePassword_ChangeSecret()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);

			userManager.SignUp("user01", "email", "dont@mail.us", "pass123456789");

			var result = userManager.ChangeSecret("email", "dont@mail.us", "pass123456789");
			
			Assert.AreEqual(true, result.Success);
		}

		[TestMethod]
		public void UserManager_NoPassword_ExistingAccount()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(),_memoryCache);
			userManager.SignUp("user02", "email", "dont@mail.us", "pass");
			
			var result = userManager.Validate("email", "dont@mail.us", null);
			Assert.AreEqual(false, result.Success);
		}
		
		[TestMethod]
		public void UserManager_AllUsers_testCache()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);
			userManager.AddUserToCache(new User{Name = "cachedUser"});

			var user = userManager.AllUsers().FirstOrDefault(p => p.Name == "cachedUser");
			Assert.IsNotNull(user);
		}
		
		[TestMethod]
		public void UserManager_RemoveUser()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(),_memoryCache);

			userManager.SignUp("to_remove", "email", "to_remove@mail.us", "pass123456789");

			var result = userManager.RemoveUser("email", "to_remove@mail.us");
			
			Assert.AreEqual(true, result.Success);
			
			var user = userManager.AllUsers().FirstOrDefault(p => p.Name == "to_remove");
			Assert.IsNull(user);
		}
		
		[TestMethod]
		public void AddToRole()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);
			userManager.SignUp("AddToRole", "email", "AddToRole@mail.us", "pass123456789");
			
			var user = userManager.GetUser("email", "AddToRole@mail.us");
			
			// Default role is User
			userManager.RemoveFromRole(user, AccountRoles.AppAccountRoles.User.ToString());

			// Now add the Admin role
			userManager.AddToRole(user, AccountRoles.AppAccountRoles.Administrator.ToString());

			var result = userManager.GetRole("email", "AddToRole@mail.us");
			
			Assert.AreEqual(AccountRoles.AppAccountRoles.Administrator.ToString(), result.Code);
		}
		
		
		[TestMethod]
		public void RemoveFromRole()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);
			userManager.SignUp("RemoveFromRole", "email", "RemoveFromRole@mail.us", "pass123456789");
			
			var user = userManager.GetUser("email", "RemoveFromRole@mail.us");
			
			// Default role is User
			userManager.RemoveFromRole(user, AccountRoles.AppAccountRoles.User.ToString());

			var result = userManager.GetRole("email", "RemoveFromRole@mail.us");
			
			Assert.IsNull(result.Code);
		}

		[TestMethod]
		public void GetUser()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);
			userManager.SignUp("GetUser", "email", "GetUser@mail.us", "pass123456789");

			var user = userManager.GetUser("email", "GetUser@mail.us");

			Assert.AreEqual("GetUser",user.Name);
		}
	}
}

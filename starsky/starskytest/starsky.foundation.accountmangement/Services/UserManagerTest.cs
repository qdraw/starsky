using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.accountmangement.Services
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
		public async Task UserManager_Test_Non_Exist_Account()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);

			var result = await userManager.ValidateAsync("email", "test", "test");
			Assert.AreEqual(false, result.Success);
		}
		
		[TestMethod]
		public async Task UserManager_WrongPassword()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);

			await userManager.SignUpAsync("user01", "email", "test@google.com", "pass");

			var result = await userManager.ValidateAsync("email", "test@google.com", "----");
			Assert.AreEqual(false, result.Success);
		}
		
		[TestMethod]
		public async Task UserManager_LoginPassword()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(),_memoryCache);

			await userManager.SignUpAsync("user01", "email", "login@mail.us", "pass");

			var result = await userManager.ValidateAsync("email", "login@mail.us", "pass");
			Assert.AreEqual(true, result.Success);
		}
		
		[TestMethod]
		public void UserManager_ChangePassword_ChangeSecret()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);

			userManager.SignUpAsync("user01", "email", "dont@mail.us", "pass123456789");

			var result = userManager.ChangeSecret("email", "dont@mail.us", "pass123456789");
			
			Assert.AreEqual(true, result.Success);
		}

		[TestMethod]
		public async Task UserManager_NoPassword_ExistingAccount()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(),_memoryCache);
			await userManager.SignUpAsync("user02", "email", "dont@mail.us", "pass");
			
			var result = await userManager.ValidateAsync("email", "dont@mail.us", null);
			Assert.AreEqual(false, result.Success);
		}
		
		[TestMethod]
		public async Task UserManager_AllUsers_testCache()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);
			await userManager.AddUserToCache(new User{Name = "cachedUser"});

			var user = (await userManager.AllUsersAsync()).FirstOrDefault(p => p.Name == "cachedUser");
			Assert.IsNotNull(user);
		}
		
		[TestMethod]
		public async Task UserManager_RemoveUser()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(),_memoryCache);

			await userManager.SignUpAsync("to_remove", "email", "to_remove@mail.us", "pass123456789");

			var result = await userManager.RemoveUser("email", "to_remove@mail.us");
			
			Assert.AreEqual(true, result.Success);
			
			var user = (await userManager.AllUsersAsync()).FirstOrDefault(p => p.Name == "to_remove");
			Assert.IsNull(user);
		}
		
		[TestMethod]
		public void AddToRole()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);
			userManager.SignUpAsync("AddToRole", "email", "AddToRole@mail.us", "pass123456789");
			
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
			userManager.SignUpAsync("RemoveFromRole", "email", "RemoveFromRole@mail.us", "pass123456789");
			
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
			userManager.SignUpAsync("GetUser", "email", "GetUser@mail.us", "pass123456789");

			var user = userManager.GetUser("email", "GetUser@mail.us");

			Assert.AreEqual("GetUser",user.Name);
		}

		[TestMethod]
		public void PreflightValidate_Fail_stringEmpty()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);
			Assert.IsFalse(userManager.PreflightValidate(string.Empty, string.Empty, string.Empty));
		}
		
		[TestMethod]
		public void PreflightValidate_Fail_wrongEmail()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);
			Assert.IsFalse(userManager.PreflightValidate("no_mail", "123456789012345", "123456789012345"));
		}
		
		[TestMethod]
		public void PreflightValidate_Ok()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);
			Assert.IsTrue(userManager.PreflightValidate("dont@mail.me", "123456789012345", "123456789012345"));
		}

		[TestMethod]
		public void GetCurrentUserId_NotLoggedIn()
		{
			var context = new DefaultHttpContext();
			var currentUserId = new UserManager(_dbContext, new AppSettings(), _memoryCache)
				.GetCurrentUserId(context);
			Assert.AreEqual(-1, currentUserId);
		}

		[TestMethod]
		public async Task CachedCredential_CheckCache()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);
			var credType = new CredentialType { Id = 1 };

			_memoryCache.Remove(userManager.CredentialCacheKey(credType,"test123456"));

			// We encrypt secret values
			_dbContext.Credentials.Add(new Credential{ Id = 6, Identifier = "test123456", Secret = "hashed_secret", CredentialType = credType});
			await _dbContext.SaveChangesAsync();
			
			// set cache with values
			userManager.CachedCredential(credType,
				"test123456");

			// Update Database
			var cred =
				_dbContext.Credentials.FirstOrDefault(p =>
					p.Identifier == "test123456");
			cred.Identifier = "test1234567";
			_dbContext.Credentials.Update(cred);
			await _dbContext.SaveChangesAsync();

			// check cache again
			var result= userManager.CachedCredential(credType,
				"test123456");
			
			Assert.IsNotNull(result);
			Assert.AreEqual("hashed_secret", result.Secret);
		}
	}
}

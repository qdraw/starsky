using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.accountmanagement.Interfaces;
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
		public async Task ValidateAsync_CredentialType_NotFound()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);

			var result = await userManager.ValidateAsync("not-found", "test", "test");
			
			Assert.AreEqual(false, result.Success);
			Assert.AreEqual(result.Error, ValidateResultError.CredentialTypeNotFound);
		}

		[TestMethod]
		public async Task SignInNull()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);
			Assert.IsFalse(await userManager.SignIn(new DefaultHttpContext(), null));
		}
		
		
		[TestMethod]
		public async Task SignInNoUserId()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);
			Assert.IsFalse(await userManager.SignIn(new DefaultHttpContext(), new User()));
		}
		
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public async Task SignInSystemArgumentNullException()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);
			// not having SignUpAsync registered
			Assert.IsFalse(await userManager.SignIn(new DefaultHttpContext(), new User{Id = 1}));
		}
		
		[TestMethod]
		public void GetUserClaims_NoId()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);

			var claims = userManager.GetUserClaims(new User());
			Assert.AreEqual(0,claims.Count());
		}
		
		[TestMethod]
		public void GetUserClaims_Null()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);

			var claims = userManager.GetUserClaims(null);
			Assert.AreEqual(0,claims.Count());
		}
		
		
		[TestMethod]
		public async Task ValidateAsync_Credential_NotFound()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);

			if ( !_dbContext.CredentialTypes.Any(p => p.Code == "email") )
			{
				_dbContext.CredentialTypes.Add(
					new CredentialType { Code = "email" });
				await _dbContext.SaveChangesAsync();
			}
			
			var result = await userManager.ValidateAsync("email", "test", "test");
			
			Assert.AreEqual(false, result.Success);
			Assert.AreEqual(result.Error, ValidateResultError.CredentialNotFound);
		}
		
		[TestMethod]
		public async Task ValidateAsync_User_NotFound()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);

			if ( !_dbContext.CredentialTypes.Any(p => p.Code == "email") )
			{
				_dbContext.CredentialTypes.Add(
					new CredentialType { Code = "email" });
				await _dbContext.SaveChangesAsync();
			}

			var credentialTypesCode = _dbContext.CredentialTypes.FirstOrDefault();

			_dbContext.Credentials.Add(new Credential
			{
				Identifier = "test_0005",
				CredentialTypeId = credentialTypesCode.Id,
				Secret = "t5cJrj735BKTx6bNw2snWzkKb5lsXDSreT9Fpz5YLJw=", // "pass123456789"
				Extra = "0kp9rQX22yeGPl3FSyZFlg=="
			});
			await _dbContext.SaveChangesAsync();

			var result = await userManager.ValidateAsync(credentialTypesCode.Code, "test_0005", "pass123456789");
			
			Assert.AreEqual(false, result.Success);
			Assert.AreEqual(result.Error, ValidateResultError.UserNotFound);
		}
		
		[TestMethod]
		public async Task ValidateAsync_LockoutEnabled()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);

			await userManager.SignUpAsync("lockout@google.com", "email", "lockout@google.com", "pass");

			var userObject = _dbContext.Users.FirstOrDefault(p =>
				p.Name == "lockout@google.com");
			
			userObject.LockoutEnabled = true;
			userObject.LockoutEnd = DateTime.UtcNow.AddDays(1);
			await _dbContext.SaveChangesAsync();

			var result = await userManager.ValidateAsync("email", "lockout@google.com", "--does not matter--");
			
			Assert.AreEqual(false, result.Success);
			Assert.AreEqual(result.Error, ValidateResultError.Lockout);
		}
		
		[TestMethod]
		public async Task ValidateAsync_3thTry()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);

			await userManager.SignUpAsync("try3@google.com", "email", "try3@google.com", "pass");

			var userObject = _dbContext.Users.FirstOrDefault(p =>
				p.Name == "try3@google.com");
			
			userObject.AccessFailedCount = 2;
			await _dbContext.SaveChangesAsync();

			var result = await userManager.ValidateAsync("email", "try3@google.com", "--does not matter--");
			
			Assert.AreEqual(false, result.Success);
			Assert.AreEqual(result.Error, ValidateResultError.Lockout);
		}
		
				
		[TestMethod]
		public async Task ValidateAsync_ResetCountAfterSuccessLogin()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);

			await userManager.SignUpAsync("reset@google.com", "email", "reset@google.com", "pass");

			var userObject = _dbContext.Users.FirstOrDefault(p =>
				p.Name == "reset@google.com");
			
			userObject.AccessFailedCount = 2;
			await _dbContext.SaveChangesAsync();

			var result = await userManager.ValidateAsync("email", "reset@google.com", "pass");
			
			Assert.AreEqual(true, result.Success);
			
			userObject = _dbContext.Users.FirstOrDefault(p =>
				p.Name == "reset@google.com");
			
			Assert.AreEqual(0,userObject.AccessFailedCount);
			Assert.AreEqual(DateTime.MinValue,userObject.LockoutEnd);
			Assert.AreEqual(false,userObject.LockoutEnabled);
		}
				
		[TestMethod]
		public async Task ValidateAsync_LockoutExpired()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);

			await userManager.SignUpAsync("lockout2@google.com", "email", "lockout2@google.com", "pass");

			var userObject = _dbContext.Users.FirstOrDefault(p =>
				p.Name == "lockout2@google.com");
			
			userObject.LockoutEnabled = true;
			userObject.LockoutEnd = DateTime.UtcNow.AddDays(-2);
			await _dbContext.SaveChangesAsync();

			var result = await userManager.ValidateAsync("email", "lockout2@google.com", "pass");
			
			Assert.AreEqual(true, result.Success);
		}
		
		[TestMethod]
		public async Task UserManager_WrongPassword()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);

			await userManager.SignUpAsync("user01", "email", "test@google.com", "pass");

			var result = await userManager.ValidateAsync("email", "test@google.com", "----");
			Assert.AreEqual(false, result.Success);
			Assert.AreEqual(result.Error, ValidateResultError.SecretNotValid);
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
		public void GetUser_credentialTypeNull_IdDoesNotExist()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);
			var user = userManager.GetUser("email", "sfkknfdlknsdfl@mail.us");
			Assert.IsNull(user);
		}

				
		[TestMethod]
		public void GetUser_IdDoesNotExist()
		{
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);
			userManager.AddDefaultCredentialType("email");
			
			var user = userManager.GetUser("email", "sfkknfdlknsdfl@mail.us");
			Assert.IsNull(user);
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

			_memoryCache.Remove(userManager.CredentialCacheKey(credType,"test_cache_add"));

			await userManager.SignUpAsync("test", "email", "test_cache_add",
				"secret");

			// set cache with values 
			userManager.CachedCredential(credType,
				"test_cache_add");

			// Update Database
			var cred =
				_dbContext.Credentials.FirstOrDefault(p =>
					p.Identifier == "test_cache_add");
			cred.Identifier = "test_cache_add_1";
			var expectSecret = cred.Secret;
			_dbContext.Credentials.Update(cred);
			await _dbContext.SaveChangesAsync();

			// check cache again
			var result= userManager.CachedCredential(credType,
				"test_cache_add");
			
			Assert.IsNotNull(result);
			Assert.AreEqual(expectSecret, result.Secret);
		}

		[TestMethod]
		public void GetUserPermissionClaims_ShouldGet()
		{
			_dbContext.RolePermissions.Add(new RolePermission
			{
				RoleId = 99,
				PermissionId = 101
			});

			_dbContext.Permissions.Add(new Permission { Id = 101, Code = "test"});
			_dbContext.SaveChanges();
			var userManager = new UserManager(_dbContext, new AppSettings(), _memoryCache);
			var result = userManager.GetUserPermissionClaims(new Role { Id = 99 }).ToList();

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("test", result[0].Value);
		}
		
				
		[TestMethod]
		public async Task Cache_ExistsByUserTableId_HitResult()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);
			await userManager.AddUserToCache(new User{Name = "cachedUser", Id = 1});

			var result = await userManager.Exist(1);
			
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public async Task Cache_ExistsByUserTableId_NotFound()
		{
			var userManager = new UserManager(_dbContext,new AppSettings(), _memoryCache);
			await userManager.AddUserToCache(new User{Name = "cachedUser", Id = 1});

			var result = await userManager.Exist(9822);
			
			Assert.IsNull(result);
		}
		
						
		[TestMethod]
		public async Task Db_ExistsByUserTableId_HitResult()
		{
			var userManager = new UserManager(_dbContext,new AppSettings{AddMemoryCache = false}, _memoryCache);
			var id = await userManager.SignUpAsync(string.Empty, "email", "t", "t");
			Assert.IsNotNull(id);

			var result = await userManager.Exist(id.User.Id);
			
			Assert.IsNotNull(result);
		}
		
		[TestMethod]
		public async Task Db_ExistsByUserTableId_NotFound()
		{
			var userManager = new UserManager(_dbContext,new AppSettings{AddMemoryCache = false}, _memoryCache);

			var result = await userManager.Exist(852);
			
			Assert.IsNull(result);
		}
	}
}

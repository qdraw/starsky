using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.metaupdate.Services;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Services;
using starsky.foundation.database.Data;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.foundation.accountmanagement.Services;

[TestClass]
public sealed class UserManagerTest
{
	private readonly IMemoryCache _memoryCache;
	private readonly ApplicationDbContext _dbContext;

	public UserManagerTest()
	{
		var provider = new ServiceCollection()
			.AddMemoryCache()
			.BuildServiceProvider();
		_memoryCache = provider.GetRequiredService<IMemoryCache>();

		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase(nameof(MetaUpdateService));
		var options = builder.Options;
		_dbContext = new ApplicationDbContext(options);
	}

	[TestMethod]
	public async Task ValidateAsync_CredentialType_NotFound()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var result = await userManager.ValidateAsync("not-found", "test", "test");

		Assert.IsFalse(result.Success);
		Assert.AreEqual(ValidateResultError.CredentialTypeNotFound, result.Error);
	}
		
	[TestMethod]
	public async Task ValidateAsync_CredentialType_stringEmpty()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var result = await userManager.ValidateAsync(string.Empty, "test", string.Empty);

		Assert.IsFalse(result.Success);
		Assert.AreEqual(ValidateResultError.SecretNotValid, result.Error);
	}

	[TestMethod]
	public async Task SignInNull()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		Assert.IsFalse(await userManager.SignIn(new DefaultHttpContext(), null!));
	}


	[TestMethod]
	public async Task SignInNoUserId()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		Assert.IsFalse(await userManager.SignIn(new DefaultHttpContext(), new User()));
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentNullException))]
	public async Task SignInSystemArgumentNullException()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		// not having SignUpAsync registered
		Assert.IsFalse(await userManager.SignIn(new DefaultHttpContext(), new User { Id = 1 }));
	}

	[TestMethod]
	public void GetUserClaims_NoId()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var claims = userManager.GetUserClaims(new User());
		Assert.AreEqual(0, claims.Count());
	}

	[TestMethod]
	public void GetUserClaims_Null()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var claims = userManager.GetUserClaims(null);
		Assert.AreEqual(0, claims.Count());
	}

	[TestMethod]
	public void GetUserClaims_ShouldReturnClaims()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var claims = userManager
			.GetUserClaims(new User { Name = "test", Id = 1 }).ToList();

		Assert.AreEqual(1, claims.Count(p => p.Type == ClaimTypes.Name));
		Assert.AreEqual(1, claims.Count(p => p.Type == ClaimTypes.NameIdentifier));
		Assert.AreEqual(1, claims.Count(p => p.Type == ClaimTypes.Email));
	}

	[TestMethod]
	public void GetUserClaims_ShouldReturnEmail()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var claims = userManager
			.GetUserClaims(new User
			{
				Name = "test",
				Id = 1,
				Credentials =
					new List<Credential> { new Credential { Identifier = "email" } }
			}).ToList();

		Assert.AreEqual("test", claims.Find(p => p.Type == ClaimTypes.Name)?.Value);
		Assert.AreEqual("1", claims.Find(p => p.Type == ClaimTypes.NameIdentifier)?.Value);
		Assert.AreEqual("email", claims.Find(p => p.Type == ClaimTypes.Email)?.Value);
	}

	[TestMethod]
	public void GetUserClaims_ShouldNotFailDueMissingEmail()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var claims = userManager
			.GetUserClaims(new User { Name = "test", Id = 1, Credentials = null }).ToList();

		Assert.AreEqual("test", claims.Find(p => p.Type == ClaimTypes.Name)?.Value);
		Assert.AreEqual("1", claims.Find(p => p.Type == ClaimTypes.NameIdentifier)?.Value);
		Assert.AreEqual("", claims.Find(p => p.Type == ClaimTypes.Email)?.Value);
	}
	
	[TestMethod]
	public async Task ValidateAsync_Credential_NotFound()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var test = await _dbContext.CredentialTypes.AnyAsync(p => p.Code == "email");
		if (!test)
		{
			_dbContext.CredentialTypes.Add(
				new CredentialType { Code = "email", Name = "t" });
			await _dbContext.SaveChangesAsync();
		}

		var result = await userManager.ValidateAsync("email", "test", "test");

		Assert.IsFalse(result.Success);
		Assert.AreEqual(ValidateResultError.CredentialNotFound, result.Error);
	}

	[TestMethod]
	public async Task ValidateAsync_User_NotFound()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var test = await _dbContext.CredentialTypes.AnyAsync(p => p.Code == "email");
		if (!test)
		{
			_dbContext.CredentialTypes.Add(
				new CredentialType { Code = "email", Name = "T" });
			await _dbContext.SaveChangesAsync();
		}

		var credentialTypesCode = await _dbContext.CredentialTypes.FirstOrDefaultAsync();

		_dbContext.Credentials.Add(new Credential
		{
			Identifier = "test_0005",
			CredentialTypeId = credentialTypesCode!.Id,
			Secret = "t5cJrj735BKTx6bNw2snWzkKb5lsXDSreT9Fpz5YLJw=", // "pass123456789" Iterate Legacy
			Extra = "0kp9rQX22yeGPl3FSyZFlg=="
		});
		await _dbContext.SaveChangesAsync();

		var result = await userManager.ValidateAsync(credentialTypesCode.Code!, "test_0005",
			"pass123456789");

		Assert.IsFalse(result.Success);
		Assert.AreEqual(ValidateResultError.UserNotFound, result.Error);
	}

	[TestMethod]
	public async Task ValidateAsync_Transform_To_Iterate100K()
	{
		var test = await _dbContext.CredentialTypes.AnyAsync(p => p.Code == "email");
		if (!test)
		{
			_dbContext.CredentialTypes.Add(
				new CredentialType { Code = "email", Name = "T" });
			await _dbContext.SaveChangesAsync();
		}
		var credentialTypesCode = await _dbContext.CredentialTypes.FirstOrDefaultAsync();

		await _dbContext.Users.AddAsync(new User
		{
			Name = "test_0008",
		});
		await _dbContext.SaveChangesAsync();

		var cred = new Credential
		{
			Identifier = "test_0008",
			CredentialTypeId = credentialTypesCode!.Id,
			CredentialType = credentialTypesCode,
			Secret = "t5cJrj735BKTx6bNw2snWzkKb5lsXDSreT9Fpz5YLJw=", // "pass123456789" (IterateLegacy)
			Extra = "0kp9rQX22yeGPl3FSyZFlg==",
			IterationCount = IterationCountType.IterateLegacySha1,
			User = await _dbContext.Users.FirstOrDefaultAsync(p => p.Name == "test_0008"),
			UserId = ( await _dbContext.Users.FirstOrDefaultAsync(p => p.Name == "test_0008") )!.Id,
			Id = 43579345
		};
		_dbContext.Credentials.Add(cred);

		await _dbContext.SaveChangesAsync();
		_dbContext.Entry(cred).State = EntityState.Detached;

		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		
		var result = await userManager.ValidateAsync(credentialTypesCode.Code!, "test_0008",
			"pass123456789");

		var credAfterTransform = await _dbContext.Credentials
			.FirstOrDefaultAsync(p => p.Identifier == "test_0008");
		
		Assert.IsTrue(result.Success);
		Assert.AreEqual("jCCNdJCtH6h1UBhEHkHawc+zt9PqQaEEubc8yc5CGTw=", credAfterTransform?.Secret);
		Assert.AreEqual(IterationCountType.Iterate100KSha256, credAfterTransform?.IterationCount);
	}

	[TestMethod]
	public async Task ValidateAsync_LockoutEnabled()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		await userManager.SignUpAsync("lockout@google.com", "email", "lockout@google.com",
			"pass");

		var userObject = await _dbContext.Users.FirstOrDefaultAsync(p =>
			p.Name == "lockout@google.com");

		userObject!.LockoutEnabled = true;
		userObject.LockoutEnd = DateTime.UtcNow.AddDays(1);
		await _dbContext.SaveChangesAsync();

		var result =
			await userManager.ValidateAsync("email", "lockout@google.com",
				"--does not matter--");

		Assert.IsFalse(result.Success);
		Assert.AreEqual(ValidateResultError.Lockout, result.Error);
	}

	[TestMethod]
	public async Task ValidateAsync_3thTry()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		await userManager.SignUpAsync("try3@google.com", "email", "try3@google.com", "pass");

		var userObject = await _dbContext.Users.FirstOrDefaultAsync(p =>
			p.Name == "try3@google.com");

		userObject!.AccessFailedCount = 2;
		await _dbContext.SaveChangesAsync();

		var result =
			await userManager.ValidateAsync("email", "try3@google.com", "--does not matter--");

		Assert.IsFalse(result.Success);
		Assert.AreEqual(ValidateResultError.Lockout, result.Error);
	}

	[TestMethod]
	public async Task ValidateAsync_ResetCountAfterSuccessLogin()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		await userManager.SignUpAsync("reset@google.com", "email", "reset@google.com", "pass");

		var userObject = await _dbContext.Users.FirstOrDefaultAsync(p =>
			p.Name == "reset@google.com");

		userObject!.AccessFailedCount = 2;
		await _dbContext.SaveChangesAsync();

		var result = await userManager.ValidateAsync("email", "reset@google.com", "pass");

		Assert.IsTrue(result.Success);

		userObject = await _dbContext.Users.FirstOrDefaultAsync(p =>
			p.Name == "reset@google.com");

		Assert.IsNotNull(userObject);
		Assert.IsNotNull(userObject.AccessFailedCount);
		Assert.AreEqual(0, userObject.AccessFailedCount);
		Assert.AreEqual(DateTime.MinValue, userObject.LockoutEnd);
		Assert.IsFalse(userObject.LockoutEnabled);
	}

	[TestMethod]
	public async Task ValidateAsync_LockoutExpired()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		await userManager.SignUpAsync("lockout2@google.com", "email", "lockout2@google.com",
			"pass");

		var userObject = await _dbContext.Users.FirstOrDefaultAsync(p =>
			p.Name == "lockout2@google.com");

		userObject!.LockoutEnabled = true;
		userObject.LockoutEnd = DateTime.UtcNow.AddDays(-2);
		await _dbContext.SaveChangesAsync();

		var result = await userManager.ValidateAsync("email", "lockout2@google.com", "pass");

		Assert.IsTrue(result.Success);
	}

	[TestMethod]
	public async Task UserManager_WrongPassword()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		await userManager.SignUpAsync("user01", "email", "test1@google.com", "pass");

		var result = await userManager.ValidateAsync("email", "test1@google.com", "----");
		Assert.IsFalse(result.Success);
		Assert.AreEqual(ValidateResultError.SecretNotValid, result.Error);

		await userManager.RemoveUser("email", "test1@google.com");
	}

	[TestMethod]
	public async Task UserManager_LoginPassword_DefaultFlow()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		await userManager.SignUpAsync("user02", "email", "login2@mail.us", "pass");

		var result = await userManager.ValidateAsync("email", "login2@mail.us", "pass");
		Assert.IsTrue(result.Success);

		await userManager.RemoveUser("email", "login2@mail.us");
	}

	[TestMethod]
	public async Task UserManager_LoginPassword_ShouldBeUser()
	{
		var userManager = new UserManager(_dbContext,
			new AppSettings
			{
				AccountRegisterDefaultRole = AccountRoles.AppAccountRoles.User,
				AccountRegisterFirstRoleAdmin = false
			}, new FakeIWebLogger(), _memoryCache);

		await userManager.SignUpAsync("user03", "email", "login3@mail.us", "pass");

		var result = userManager.GetRole("email", "login3@mail.us");
		Assert.IsNotNull(result);
		Assert.IsNotNull(result.Code);
		Assert.AreEqual(AccountRoles.AppAccountRoles.User.ToString(), result.Code);

		await userManager.RemoveUser("email", "login3@mail.us");
	}

	[TestMethod]
	public async Task UserManager_LoginPassword_ShouldBeAdminDueFirstPolicy()
	{
		var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
		builder.UseInMemoryDatabase(nameof(MetaUpdateService) + "_test");
		var options = builder.Options;
		var dbContext = new ApplicationDbContext(options);

		var userManager = new UserManager(dbContext,
			new AppSettings
			{
				AccountRegisterDefaultRole = AccountRoles.AppAccountRoles.User,
				AccountRegisterFirstRoleAdmin = true
			}, new FakeIWebLogger(), _memoryCache);

		await userManager.SignUpAsync("user04", "email", "login@mail.us", "pass");

		var result = userManager.GetRole("email", "login@mail.us");
		Assert.AreEqual(AccountRoles.AppAccountRoles.Administrator.ToString(), result?.Code);
	}

	[TestMethod]
	public async Task UserManager_LoginPassword_ShouldUser_second()
	{
		var userManager = new UserManager(_dbContext,
			new AppSettings
			{
				AccountRegisterDefaultRole = AccountRoles.AppAccountRoles.User,
				AccountRegisterFirstRoleAdmin = true
			}, new FakeIWebLogger(), _memoryCache);

		foreach ( var user in await _dbContext.Users.Include(p => p.Credentials)
			         .Where(p => p.Credentials != null && p.Credentials.Count != 0)
			         .ToListAsync() )
		{
			await userManager.RemoveUser("email",
				user.Credentials!.FirstOrDefault()!.Identifier!);
		}

		await userManager.SignUpAsync("user01", "email", "login@mail.us", "pass");
		await userManager.SignUpAsync("user02", "email", "login@mail2.us", "pass");

		var result = userManager.GetRole("email", "login@mail2.us");

		Assert.IsNotNull(result);
		Assert.IsNotNull(result.Code);
		Assert.AreEqual(AccountRoles.AppAccountRoles.User.ToString(), result.Code);
	}

	[TestMethod]
	public async Task UserManager_LoginNull_Identifier()
	{
		var userManager = new UserManager(_dbContext,
			new AppSettings
			{
				AccountRegisterDefaultRole = AccountRoles.AppAccountRoles.User,
				AccountRegisterFirstRoleAdmin = true
			}, new FakeIWebLogger(), _memoryCache);

		var result = await userManager.SignUpAsync("user01", "email", string.Empty, "pass");

		Assert.AreEqual(SignUpResultError.NullString, result.Error);
	}

	[TestMethod]
	public async Task UserManager_LoginNull_Secret()
	{
		var userManager = new UserManager(_dbContext,
			new AppSettings
			{
				AccountRegisterDefaultRole = AccountRoles.AppAccountRoles.User,
				AccountRegisterFirstRoleAdmin = true
			}, new FakeIWebLogger(), _memoryCache);

		var result =
			await userManager.SignUpAsync("user01", "email", "dont@mail.me", string.Empty);

		Assert.AreEqual(SignUpResultError.NullString, result.Error);
	}

	[TestMethod]
	public async Task UserManager_Login_CredentialTypeNotFound()
	{
		var userManager = new UserManager(_dbContext,
			new AppSettings
			{
				AccountRegisterDefaultRole = AccountRoles.AppAccountRoles.User,
				AccountRegisterFirstRoleAdmin = true
			}, new FakeIWebLogger(), _memoryCache);

		var result =
			await userManager.SignUpAsync("user01", "wrong-type1", "dont@mail.me", "122344");

		Assert.AreEqual(SignUpResultError.CredentialTypeNotFound, result.Error);
	}

	[TestMethod]
	public async Task UserManager_ChangePassword_ChangeSecret()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		await userManager.SignUpAsync("user01", "email", "dont@mail.us", "pass123456789");

		var result = userManager.ChangeSecret("email", "dont@mail.us", "pass123456789");

		Assert.IsTrue(result.Success);
	}

	[TestMethod]
	public void ChangeSecret_Credential_WrongTypeCode()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var result = userManager.ChangeSecret("wrong-type", "dont@mail.us", "pass123456789");

		Assert.AreEqual(ChangeSecretResultError.CredentialTypeNotFound, result.Error);
	}

	[TestMethod]
	public async Task ChangeSecret_Credential_NotFound()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		_dbContext.CredentialTypes.Add(
			new CredentialType { Code = "email", Name = "email", Id = 99 });
		await _dbContext.SaveChangesAsync();

		var result =
			userManager.ChangeSecret("email", "fdksdnfdsfl@sdnklffsd.com", "pass123456789");


		var emailType = await _dbContext.CredentialTypes.FirstOrDefaultAsync(p => p.Code == "email");
		_dbContext.Remove(emailType!);

		await _dbContext.SaveChangesAsync();

		Assert.AreEqual(ChangeSecretResultError.CredentialNotFound, result.Error);
	}

	[TestMethod]
	public async Task UserManager_NoPassword_ExistingAccount()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		await userManager.SignUpAsync("user02", "email", "dont@mail.us", "pass");

		var result = await userManager.ValidateAsync("email", "dont@mail.us", null!);
		Assert.IsFalse(result.Success);
	}

	[TestMethod]
	public async Task UserManager_AllUsers_testCache()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		await userManager.AddUserToCache(new User { Name = "cachedUser" });

		var user =
			( await userManager.AllUsersAsync() ).Users.Find(p => p.Name == "cachedUser");
		Assert.IsNotNull(user);
	}

	private class AppDbContextRetryLimitExceededException(DbContextOptions options)
		: ApplicationDbContext(options)
	{
		public override DbSet<User> Users => throw new RetryLimitExceededException("general");
	}

	[TestMethod]
	public async Task UserManager_AllUsers_RetryException()
	{
		var logger = new FakeIWebLogger();
		var options = new DbContextOptionsBuilder<ApplicationDbContext>()
			.UseInMemoryDatabase(databaseName: "MovieListDatabase")
			.Options;

		var userManager = new UserManager(new AppDbContextRetryLimitExceededException(options),
			new AppSettings(), logger, new FakeMemoryCache());
		var users = ( await userManager.AllUsersAsync() ).Users;

		Assert.AreEqual(0, users.Count);
		Assert.IsTrue(logger.TrackedExceptions.LastOrDefault().Item2
			?.Contains("RetryLimitExceededException") == true);
	}

	[TestMethod]
	public async Task UserManager_RemoveUser()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		await userManager.SignUpAsync("to_remove", "email", "to_remove@mail.us",
			"pass123456789");

		var result = await userManager.RemoveUser("email", "to_remove@mail.us");

		Assert.IsTrue(result.Success);

		var user = ( await userManager.AllUsersAsync() ).Users.Find(p => p.Name == "to_remove");
		Assert.IsNull(user);
	}

	[TestMethod]
	public async Task UserManager_RemoveUser_NonExistsCredType()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);

		var result = await userManager.RemoveUser("___email___", "non_exists@mail.us");

		Assert.IsFalse(result.Success);
	}

	[TestMethod]
	public async Task UserManager_RemoveUser_NonExists()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		await userManager.AddDefaultCredentialType("email");

		var result = await userManager.RemoveUser("email", "non_exists@mail.us");

		Assert.IsFalse(result.Success);
	}

	[TestMethod]
	public async Task AddToRole()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		await userManager.SignUpAsync("AddToRole", "email", "AddToRole@mail.us",
			"pass123456789");

		var user = userManager.GetUser("email", "AddToRole@mail.us");
		Assert.IsNotNull(user);

		// Default role is User
		userManager.RemoveFromRole(user, AccountRoles.AppAccountRoles.User.ToString());

		// Now add the Admin role
		userManager.AddToRole(user, AccountRoles.AppAccountRoles.Administrator.ToString());

		var result = userManager.GetRole("email", "AddToRole@mail.us");

		Assert.IsNotNull(result);
		Assert.AreEqual(AccountRoles.AppAccountRoles.Administrator.ToString(), result.Code);
	}

	[TestMethod]
	public void AddToRole_WrongCode()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		var count = _dbContext.Roles.Count();
		userManager.AddToRole(new User(), "test123");

		Assert.AreEqual(count, _dbContext.Roles.Count());
	}

	[TestMethod]
	public void GetRole_NotExists()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		var result = userManager.GetRole("test12", "test");
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetRoleAsync_NotExists()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		var result = await userManager.GetRoleAsync(453454);
		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task GetRoleAsync_Exists()
	{
		_dbContext.Users.Add(new User { Id = 45475, Name = "test" });
		_dbContext.Roles.Add(new Role
		{
			Code = "test_role_892453", Name = "test", Id = 47583945
		});
		_dbContext.UserRoles.Add(new UserRole { UserId = 45475, RoleId = 47583945 });

		await _dbContext.SaveChangesAsync();
		var role =
			await _dbContext.Roles.FirstOrDefaultAsync(p => p.Code == "test_role_892453");
		var userRole =
			await _dbContext.UserRoles.FirstOrDefaultAsync(p => p.UserId == 45475);
		var user =
			await _dbContext.Users.FirstOrDefaultAsync(p => p.Id == 45475);
		Assert.IsNotNull(role);
		Assert.IsNotNull(userRole);
		Assert.IsNotNull(user);

		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		var result = await userManager.GetRoleAsync(45475);

		Assert.AreEqual("test_role_892453", result?.Code);

		_dbContext.Remove(role);
		_dbContext.Remove(userRole);
		_dbContext.Remove(user);

		await _dbContext.SaveChangesAsync();
	}

	[TestMethod]
	public async Task RemoveFromRole()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		await userManager.SignUpAsync("RemoveFromRole", "email", "RemoveFromRole@mail.us",
			"pass123456789");

		var user = userManager.GetUser("email", "RemoveFromRole@mail.us");
		Assert.IsNotNull(user);

		// Default role is User
		userManager.RemoveFromRole(user, AccountRoles.AppAccountRoles.User.ToString());

		var result = userManager.GetRole("email", "RemoveFromRole@mail.us");

		Assert.IsNotNull(result);
		Assert.IsNull(result.Code);
	}

	[TestMethod]
	public void RemoveFromRole_WrongCode()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		var count = _dbContext.Roles.Count();
		userManager.RemoveFromRole(new User(), "test");

		Assert.AreEqual(count, _dbContext.Roles.Count());
	}

	[TestMethod]
	public async Task GetUser()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		await userManager.SignUpAsync("GetUser", "email", "GetUser@mail.us", "pass123456789");

		var user = userManager.GetUser("email", "GetUser@mail.us");

		Assert.IsNotNull(user);
		Assert.AreEqual("GetUser", user.Name);
	}

	[TestMethod]
	public void GetUser_credentialTypeNull_IdDoesNotExist()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		var user = userManager.GetUser("email", "sfkknfdlknsdfl@mail.us");
		Assert.IsNull(user);
	}

	[TestMethod]
	public async Task GetUser_IdDoesNotExist()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		await userManager.AddDefaultCredentialType("email");

		var user = userManager.GetUser("email", "sfkknfdlknsdfl@mail.us");
		Assert.IsNull(user);
	}


	[TestMethod]
	public void PreflightValidate_Fail_stringEmpty()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		Assert.IsFalse(userManager.PreflightValidate(string.Empty, string.Empty, string.Empty));
	}

	[TestMethod]
	public void PreflightValidate_Fail_wrongEmail()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		Assert.IsFalse(userManager.PreflightValidate("no_mail", "123456789012345",
			"123456789012345"));
	}

	[TestMethod]
	public void PreflightValidate_Ok()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		Assert.IsTrue(userManager.PreflightValidate("dont@mail.me", "123456789012345",
			"123456789012345"));
	}

	[TestMethod]
	public void GetCurrentUserId_NotLoggedIn()
	{
		var context = new DefaultHttpContext();
		var currentUserId = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
				_memoryCache)
			.GetCurrentUserId(context);
		Assert.AreEqual(-1, currentUserId);
	}

	[TestMethod]
	public async Task CachedCredential_CheckCache()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		var credType = new CredentialType { Id = 15, Code = "email1", Name = "1" };

		_memoryCache.Remove(UserManager.CredentialCacheKey(credType, "test_cache_add"));

		await _dbContext.CredentialTypes.AddAsync(credType);
		await _dbContext.SaveChangesAsync();


		await userManager.SignUpAsync("test", "email1", "test_cache_add",
			"secret");

		// set cache with values 
		userManager.CachedCredential(credType,
			"test_cache_add");

		// Update Database
		var cred =
			await _dbContext.Credentials.FirstOrDefaultAsync(p =>
				p.Identifier == "test_cache_add");
		cred!.Identifier = "test_cache_add_1";
		var expectSecret = cred.Secret;
		_dbContext.Credentials.Update(cred);
		await _dbContext.SaveChangesAsync();

		// check cache again
		var result = userManager.CachedCredential(credType,
			"test_cache_add");

		Assert.IsNotNull(result);
		Assert.AreEqual(expectSecret, result.Secret);
	}

	[TestMethod]
	public void GetUserPermissionClaims_ShouldGet()
	{
		_dbContext.RolePermissions.Add(new RolePermission { RoleId = 99, PermissionId = 101 });

		_dbContext.Permissions.Add(new Permission { Id = 101, Code = "test", Name = "t" });
		_dbContext.SaveChanges();
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		var result = userManager.GetUserPermissionClaims(new Role { Id = 99 }).ToList();

		Assert.AreEqual(1, result.Count);
		Assert.AreEqual("test", result[0].Value);
	}


	[TestMethod]
	public async Task Cache_ExistsByUserTableId_HitResult()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		await userManager.AddUserToCache(new User { Name = "cachedUser", Id = 1 });

		var result = await userManager.ExistAsync(1);

		Assert.IsNotNull(result);
	}

	[TestMethod]
	public async Task Cache_ExistsByUserTableId_NotFound()
	{
		var userManager = new UserManager(_dbContext, new AppSettings(), new FakeIWebLogger(),
			_memoryCache);
		await userManager.AddUserToCache(new User { Name = "cachedUser", Id = 1 });

		var result = await userManager.ExistAsync(9822);

		Assert.IsNull(result);
	}


	[TestMethod]
	public async Task Db_ExistsByUserTableId_HitResult()
	{
		var userManager = new UserManager(_dbContext,
			new AppSettings { AddMemoryCache = false }, new FakeIWebLogger(), _memoryCache);
		var id = await userManager.SignUpAsync(string.Empty, "email", "t", "t");
		Assert.IsNotNull(id);
		Assert.IsNotNull(id.User);

		var result = await userManager.ExistAsync(id.User.Id);

		Assert.IsNotNull(result);
	}

	[TestMethod]
	public async Task Db_ExistsByUserTableId_NotFound()
	{
		var userManager = new UserManager(_dbContext,
			new AppSettings { AddMemoryCache = false }, new FakeIWebLogger(), _memoryCache);

		var result = await userManager.ExistAsync(852);

		Assert.IsNull(result);
	}

	[TestMethod]
	public async Task ResetAndSuccessTest()
	{
		var userManager = new UserManager(_dbContext,
			new AppSettings { AddMemoryCache = false }, new FakeIWebLogger(), _memoryCache);
		var result = await userManager.ResetAndSuccess(3, 999, null);
		Assert.IsFalse(result.Success);
	}

	[TestMethod]
	public async Task SetLockIfFailCountIsToHighTest()
	{
		var userManager = new UserManager(_dbContext,
			new AppSettings { AddMemoryCache = false }, new FakeIWebLogger(), _memoryCache);
		var result = await userManager.SetLockIfFailedCountIsToHigh(9999);
		Assert.IsFalse(result.Success);
	}

	[TestMethod]
	public async Task UserManager_GetRoleAddToUser_Administrator()
	{
		var beforeItem = new User() { Name = "test1234567" };
		await _dbContext.Users.AddAsync(beforeItem);
		await _dbContext.SaveChangesAsync();

		const string testEmail = "dont@mail.me";
		var userManager = new UserManager(_dbContext, new AppSettings
		{
			AddMemoryCache = false,
			AccountRegisterFirstRoleAdmin = false,
			AccountRegisterDefaultRole = AccountRoles.AppAccountRoles.User,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
			AccountRolesByEmailRegisterOverwrite = { { testEmail, "Administrator" } }
#pragma warning restore CS8670 // Object or collection initializer implicitly dereferences possibly null member.
		}, new FakeIWebLogger(), _memoryCache);

		var roleAddToUser = userManager.GetRoleAddToUser(testEmail, new User());

		_dbContext.Remove(beforeItem);
		await _dbContext.SaveChangesAsync();

		Assert.IsNotNull(roleAddToUser);
		Assert.AreEqual("Administrator", roleAddToUser);
	}

	[TestMethod]
	public async Task UserManager_GetRoleAddToUser_User()
	{
		var beforeItem = new User() { Name = "27898349abc9487" };
		await _dbContext.Users.AddAsync(beforeItem);
		await _dbContext.SaveChangesAsync();

		const string testEmail = "dont2@mail.me";
		var userManager = new UserManager(_dbContext, new AppSettings
		{
			AddMemoryCache = false,
			AccountRegisterFirstRoleAdmin = false,
			AccountRegisterDefaultRole = AccountRoles.AppAccountRoles.Administrator,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
			AccountRolesByEmailRegisterOverwrite = { { testEmail, "User" } }
#pragma warning restore CS8670 // Object or collection initializer implicitly dereferences possibly null member.
		}, new FakeIWebLogger(), _memoryCache);

		var roleAddToUser = userManager.GetRoleAddToUser(testEmail, new User());

		_dbContext.Remove(beforeItem);
		await _dbContext.SaveChangesAsync();

		Assert.IsNotNull(roleAddToUser);
		Assert.AreEqual("User", roleAddToUser);
	}

	[TestMethod]
	public async Task UserManager_GetRoleAddToUser_BogusRole()
	{
		var beforeItem = new User() { Name = "27898349abc9487" };
		await _dbContext.Users.AddAsync(beforeItem);
		await _dbContext.SaveChangesAsync();

		const string testEmail = "dont2@mail.me";
		var userManager = new UserManager(_dbContext, new AppSettings
		{
			AddMemoryCache = false,
			AccountRegisterFirstRoleAdmin = false,
			AccountRegisterDefaultRole = AccountRoles.AppAccountRoles.User,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
			AccountRolesByEmailRegisterOverwrite = { { testEmail, "BogusRole" } }
#pragma warning restore CS8670 // Object or collection initializer implicitly dereferences possibly null member.
		}, new FakeIWebLogger(), _memoryCache);

		var roleAddToUser = userManager.GetRoleAddToUser(testEmail, new User());

		_dbContext.Remove(beforeItem);
		await _dbContext.SaveChangesAsync();

		Assert.IsNotNull(roleAddToUser);
		// does fallback to default role
		Assert.AreEqual("User", roleAddToUser);
	}

	[TestMethod]
	public async Task UserManager_GetRoleAddToUser_IgnoreItself()
	{
		const string testEmail = "dont3@mail.me";
		const string id = "4859353904354";

		foreach ( var user in await _dbContext.Users.ToListAsync() )
		{
			_dbContext.Users.Remove(user);
		}

		await _dbContext.SaveChangesAsync();

		await _dbContext.Users.AddAsync(new User() { Name = id });
		await _dbContext.SaveChangesAsync();
		var beforeItem =
			await _dbContext.Users.FirstOrDefaultAsync(p => p.Name == id);

		Assert.IsNotNull(beforeItem);
		Assert.AreEqual(id, (await _dbContext.Users.FirstOrDefaultAsync(p => p.Name == id))?.Name);

		var userManager = new UserManager(_dbContext,
			new AppSettings
			{
				AddMemoryCache = false,
				AccountRegisterFirstRoleAdmin = true,
				AccountRegisterDefaultRole = AccountRoles.AppAccountRoles.User,
			}, new FakeIWebLogger(), _memoryCache);

		var roleAddToUser = userManager.GetRoleAddToUser(testEmail, beforeItem);

		// clean user
		var item = await _dbContext.Users.FirstOrDefaultAsync(p => p.Name == id);
		_dbContext.Users.Remove(item!);
		await _dbContext.SaveChangesAsync();

		// check right roles
		Assert.AreEqual("Administrator", roleAddToUser);
	}
}

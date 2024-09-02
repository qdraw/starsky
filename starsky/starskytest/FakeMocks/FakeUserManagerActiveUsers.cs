using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Models;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks;

public class FakeUserManagerActiveUsers : IUserManager
{
	public FakeUserManagerActiveUsers(string identifier = "test", User? currentUser = null)
	{
		CurrentUser = currentUser;
		Credentials = new Credential
		{
			UserId = 1,
			Identifier = identifier,
			Secret = "NNzKymrSy9IkybnFxwVvTRiAYuiOUoPHvXwBJybORrQ=", // test123456789
			Extra = "TgBCDRHGklOMqJ/mAJYqHg==",
			CredentialTypeId = 1
		};
		Role = new Role { Code = AccountRoles.AppAccountRoles.User.ToString() };
	}

	public User? CurrentUser { get; set; }
	public Credential Credentials { get; set; }
	public Role? Role { get; set; }

	public List<User> Users { get; set; } = new();

	public Task<UserOverviewModel> AllUsersAsync()
	{
		// null can be for testing
		return Task.FromResult(new UserOverviewModel(new List<User> { CurrentUser! }));
	}

	public Task<SignUpResult> SignUpAsync(string name, string credentialTypeCode,
		string? identifier, string? secret)
	{
		Users.Add(new User
		{
			Name = name,
			Credentials = new List<Credential>
			{
				new()
				{
					CredentialType = new CredentialType { Code = "email" },
					Identifier = identifier,
					Secret = secret,
					IterationCount = IterationCountType.Iterate100KSha256
				}
			}
		});

		return Task.FromResult(new SignUpResult());
	}

	public void AddToRole(User user, string roleCode)
	{
		AddToRole(user, new Role { Code = roleCode });
	}

	public void AddToRole(User user, Role role)
	{
		Role = role;
	}

	public void RemoveFromRole(User user, string roleCode)
	{
		Role = null;
	}

	public void RemoveFromRole(User user, Role role)
	{
		Role = null;
	}

	public ChangeSecretResult ChangeSecret(string credentialTypeCode, string? identifier,
		string secret)
	{
		return new ChangeSecretResult { Success = true };
	}

#pragma warning disable 1998
	public async Task<ValidateResult> ValidateAsync(string credentialTypeCode, string? identifier,
		string secret)
#pragma warning restore 1998
	{
		return identifier switch
		{
			// this user is rejected
			"reject" => new ValidateResult
			{
				Success = false, Error = ValidateResultError.CredentialNotFound
			},
			"lockout" => new ValidateResult
			{
				Success = false, Error = ValidateResultError.Lockout
			},
			_ => new ValidateResult { Success = true }
		};
	}

	public Task<bool> SignIn(HttpContext httpContext, User? user, bool isPersistent = false)
	{
		return Task.FromResult(true);
	}

	public void SignOut(HttpContext httpContext)
	{
		throw new NotImplementedException();
	}

	public int GetCurrentUserId(HttpContext httpContext)
	{
		throw new NotImplementedException();
	}

	public User? GetCurrentUser(HttpContext httpContext)
	{
		return CurrentUser;
	}

	public User? GetUser(string credentialTypeCode, string identifier)
	{
		if ( CurrentUser != null && !Users.Contains(CurrentUser!) )
			Users.Add(CurrentUser!);

		return Users.Find(p => p.Credentials?.Any(credential =>
			credential.Identifier == identifier) == true);
	}

	public Credential GetCredentialsByUserId(int userId)
	{
		return Credentials;
	}

	public Task<ValidateResult> RemoveUser(string credentialTypeCode,
		string identifier)
	{
		return Task.FromResult(new ValidateResult());
	}

	public User? Exist(string identifier)
	{
		if ( Credentials.Identifier == identifier ) return CurrentUser;
		return null;
	}

	public Task<User?> ExistAsync(int userTableId)
	{
		return Task.FromResult(CurrentUser);
	}

	public Role? GetRole(string credentialTypeCode, string identifier)
	{
		return Role;
	}

	public Task<Role?> GetRoleAsync(int userId)
	{
		return Task.FromResult(Role);
	}

	public bool PreflightValidate(string userName, string password, string confirmPassword)
	{
		return password != "false";
	}

	public CredentialType? GetCachedCredentialType(string email)
	{
		return new CredentialType { Code = "email" };
	}

	public void AddUserToCache(User user)
	{
		throw new NotImplementedException();
	}
}

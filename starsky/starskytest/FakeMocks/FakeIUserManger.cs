using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Models;
using starsky.foundation.database.Models.Account;

namespace starskytest.FakeMocks;

public class FakeIUserManger : IUserManager
{
	private readonly UserOverviewModel _userOverviewModel;

	public FakeIUserManger(UserOverviewModel userOverviewModel)
	{
		_userOverviewModel = userOverviewModel;
	}

	public Task<UserOverviewModel> AllUsersAsync()
	{
		return Task.FromResult(_userOverviewModel);
	}

	public Task<SignUpResult> SignUpAsync(string name, string credentialTypeCode,
		string? identifier,
		string? secret)
	{
		_userOverviewModel.Users.Add(new User
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
		throw new NotImplementedException();
	}

	public void AddToRole(User user, Role role)
	{
		throw new NotImplementedException();
	}

	public void RemoveFromRole(User user, string roleCode)
	{
		throw new NotImplementedException();
	}

	public void RemoveFromRole(User user, Role role)
	{
		throw new NotImplementedException();
	}

	public ChangeSecretResult ChangeSecret(string credentialTypeCode, string? identifier,
		string secret)
	{
		var result = _userOverviewModel.Users.Find(p =>
			p.Credentials?.FirstOrDefault()?.Identifier == identifier);
		if ( result == null )
			return new ChangeSecretResult(false, ChangeSecretResultError.CredentialNotFound);
		result.Credentials!.FirstOrDefault()!.IterationCount = IterationCountType.Iterate100KSha256;
		result.Credentials!.FirstOrDefault()!.Secret = secret;
		
		return new ChangeSecretResult(true);
	}

	public Task<ValidateResult> ValidateAsync(string credentialTypeCode, string? identifier,
		string secret)
	{
		var validateResult = new ValidateResult();
		var result = _userOverviewModel.Users.Find(p =>
			p.Credentials?.FirstOrDefault()?.Identifier == identifier);

		if ( result?.Credentials?.FirstOrDefault()?.Secret == secret )
			validateResult.Success = true;
		else
			validateResult.Error = ValidateResultError.SecretNotValid;
		return Task.FromResult(validateResult);
	}

	public Task<bool> SignIn(HttpContext httpContext, User? user, bool isPersistent = false)
	{
		// should contain salt value to be successful!
		return Task.FromResult(!string.IsNullOrEmpty(user?.Credentials?.FirstOrDefault()?.Extra));
	}

	public void SignOut(HttpContext httpContext)
	{
		throw new NotImplementedException();
	}

	public int GetCurrentUserId(HttpContext httpContext)
	{
		throw new NotImplementedException();
	}

	public User GetCurrentUser(HttpContext httpContext)
	{
		throw new NotImplementedException();
	}

	public User? GetUser(string credentialTypeCode, string identifier)
	{
		return _userOverviewModel.Users.Find(p => p.Credentials?.Any(credential =>
			credential.Identifier == identifier) == true);
	}

	public Credential? GetCredentialsByUserId(int userId)
	{
		return _userOverviewModel.Users.Find(p => p.Credentials?.Any(credential =>
			credential.Id == 0) == true)?.Credentials?.FirstOrDefault(p =>
			p.Id == 0);
	}

	public Task<ValidateResult> RemoveUser(string credentialTypeCode, string identifier)
	{
		throw new NotImplementedException();
	}

	public User Exist(string identifier)
	{
		throw new NotImplementedException();
	}

	public Task<User?> ExistAsync(int userTableId)
	{
		throw new NotImplementedException();
	}

	public Role GetRole(string credentialTypeCode, string identifier)
	{
		throw new NotImplementedException();
	}

	public Task<Role?> GetRoleAsync(int userId)
	{
		throw new NotImplementedException();
	}

	public bool PreflightValidate(string userName, string password,
		string confirmPassword)
	{
		throw new NotImplementedException();
	}

	public CredentialType? GetCachedCredentialType(string email)
	{
		return new CredentialType { Code = "email" };
	}
}

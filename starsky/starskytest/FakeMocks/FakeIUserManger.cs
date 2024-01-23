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

	public Task<SignUpResult> SignUpAsync(string name, string credentialTypeCode, string? identifier,
		string? secret)
	{
		throw new System.NotImplementedException();
	}

	public void AddToRole(User user, string roleCode)
	{
		throw new System.NotImplementedException();
	}

	public void AddToRole(User user, Role role)
	{
		throw new System.NotImplementedException();
	}

	public void RemoveFromRole(User user, string roleCode)
	{
		throw new System.NotImplementedException();
	}

	public void RemoveFromRole(User user, Role role)
	{
		throw new System.NotImplementedException();
	}

	public ChangeSecretResult ChangeSecret(string credentialTypeCode, string? identifier,
		string secret)
	{
		throw new System.NotImplementedException();
	}

	public Task<ValidateResult> ValidateAsync(string credentialTypeCode, string? identifier, string secret)
	{
		var validateResult = new ValidateResult();
		var result = _userOverviewModel.Users.Find(p => p.Credentials?.FirstOrDefault()?.Identifier == identifier);
		if ( result?.Credentials?.FirstOrDefault()?.Secret == secret )
		{
			validateResult.Success = true;
		}
		return Task.FromResult(validateResult);
	}

	public Task<bool> SignIn(HttpContext httpContext, User? user, bool isPersistent = false)
	{
		// should contain salt value to be successful!
		return Task.FromResult(!string.IsNullOrEmpty(user?.Credentials?.FirstOrDefault()?.Extra));
	}

	public void SignOut(HttpContext httpContext)
	{
		throw new System.NotImplementedException();
	}

	public int GetCurrentUserId(HttpContext httpContext)
	{
		throw new System.NotImplementedException();
	}

	public User GetCurrentUser(HttpContext httpContext)
	{
		throw new System.NotImplementedException();
	}

	public User GetUser(string credentialTypeCode, string identifier)
	{
		throw new System.NotImplementedException();
	}

	public Credential GetCredentialsByUserId(int userId)
	{
		throw new System.NotImplementedException();
	}

	public Task<ValidateResult> RemoveUser(string credentialTypeCode, string identifier)
	{
		throw new System.NotImplementedException();
	}

	public User Exist(string identifier)
	{
		throw new System.NotImplementedException();
	}

	public Task<User?> ExistAsync(int userTableId)
	{
		throw new System.NotImplementedException();
	}

	public Role GetRole(string credentialTypeCode, string identifier)
	{
		throw new System.NotImplementedException();
	}

	public bool PreflightValidate(string userName, string password,
		string confirmPassword)
	{
		throw new System.NotImplementedException();
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.accountmanagement.Models;
using starsky.foundation.database.Models.Account;
using starsky.foundation.platform.Models;

namespace starskytest.FakeMocks
{
	public class FakeUserManagerActiveUsers : IUserManager
	{

		public FakeUserManagerActiveUsers(string identifier = "test", User currentUser = null )
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
			Role = new Role {Code = AccountRoles.AppAccountRoles.User.ToString()};
		}
		public User CurrentUser { get; set; }
		public Credential Credentials { get; set; }
		public Role Role { get; set; }

		public Task<List<User>> AllUsers()
		{
			return Task.FromResult(new List<User>{CurrentUser});
		}

		public void AddUserToCache(User user)
		{
			throw new System.NotImplementedException();
		}

		public Task<SignUpResult> SignUpAsync(string name, string credentialTypeCode,
			string identifier, string secret)
		{
			return Task.FromResult(new SignUpResult());
		}

		public void AddToRole(User user, string roleCode)
		{
			AddToRole(user,new Role{Code = roleCode});
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

		public ChangeSecretResult ChangeSecret(string credentialTypeCode, string identifier, string secret)
		{
			return new ChangeSecretResult{Success = true};
		}

#pragma warning disable 1998
		public async Task<ValidateResult> Validate(string credentialTypeCode, string identifier, string secret)
#pragma warning restore 1998
		{
			// this user is rejected
			if(identifier == "reject") return new ValidateResult{Success = false};
			
			return new ValidateResult{Success = true, Error = ValidateResultError.CredentialTypeNotFound};
		}

		public Task SignIn(HttpContext httpContext, User user, bool isPersistent = false)
		{
			throw new System.NotImplementedException();
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
			return CurrentUser;
		}

		public User GetUser(string credentialTypeCode, string identifier)
		{
			return CurrentUser;
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

		public User Exist(string identifier)
		{
			if ( Credentials.Identifier == identifier )
			{
				return CurrentUser;
			}
			return null;
		}

		public Role GetRole(string credentialTypeCode, string identifier)
		{
			return Role;
		}

		public bool PreflightValidate(string userName, string password, string confirmPassword)
		{
			return password != "false";
		}
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starskycore.Interfaces;
using starskycore.Models.Account;

namespace starskytest.FakeMocks
{
	public class FakeUserManagerActiveUsers : IUserManager
	{

		public FakeUserManagerActiveUsers(string identifier = "test" )
		{
			Credentials = new Credential
			{
				UserId = CurrentUser.Id,
				Identifier = identifier,
				Secret = "NNzKymrSy9IkybnFxwVvTRiAYuiOUoPHvXwBJybORrQ=", // test123456789
				Extra = "TgBCDRHGklOMqJ/mAJYqHg==",
				CredentialTypeId = 1
			};
		}
		
		public User CurrentUser => new User {Name = "t1", Id = 99};
		public Credential Credentials { get; set; }

		public List<User> AllUsers()
		{
			return new List<User>{CurrentUser};
		}

		public void AddUserToCache(User user)
		{
			throw new System.NotImplementedException();
		}

		public SignUpResult SignUp(string name, string credentialTypeCode, string identifier, string secret)
		{
			throw new System.NotImplementedException();
		}

		public ChangeSecretResult ChangeSecret(string credentialTypeCode, string identifier, string secret)
		{
			return new ChangeSecretResult{Success = true};
		}

		public ValidateResult Validate(string credentialTypeCode, string identifier, string secret)
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

		public Credential GetCredentialsByUserId(int userId)
		{
			return Credentials;
		}

		public ValidateResult RemoveUser(string credentialTypeCode, string identifier)
		{
			return new ValidateResult();
		}

		public User Exist(string identifier)
		{
			if ( Credentials.Identifier == identifier )
			{
				return CurrentUser;
			}
			return null;
		}
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using starskycore.Interfaces;
using starskycore.Models.Account;

namespace starskytest.FakeMocks
{
	public class FakeUserManagerActiveUsers : IUserManager
	{
		public List<User> AllUsers()
		{
			return new List<User>{new User
			{
				Name = "t1"
			}};
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
			throw new System.NotImplementedException();
		}

		public ValidateResult Validate(string credentialTypeCode, string identifier, string secret)
		{
			throw new System.NotImplementedException();
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
			throw new System.NotImplementedException();
		}
	}
}

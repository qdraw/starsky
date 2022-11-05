using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Interfaces;
using starsky.foundation.database.Models.Account;

namespace starskytest.Interfaces
{
	[TestClass]
	public sealed class UserManagerTest
	{
		[TestMethod]
		public void UserManagerTestSuccessFalse()
		{
			var error = new ChangeSecretResultError();
			var secretResult = new ChangeSecretResult(false,error);
			Assert.AreEqual(false,secretResult.Success);
		}
		
		[TestMethod]
		public void UserManagerTestChangeSecretResult()
		{
			var error = new ChangeSecretResultError();
			var secretResult = new ChangeSecretResult(false,error);
			Assert.AreEqual(error,secretResult.Error);
		}

		[TestMethod]
		public void UserManagerTestSignUpResult()
		{
			var result = new SignUpResult(new User{Name = "test"});
			Assert.AreEqual("test",result.User.Name);
		}
		
		[TestMethod]
		public void UserManagerTestSignUpResultFalse()
		{
			var result = new SignUpResult(new User{Name = "test"},false, new SignUpResultError());
			Assert.IsFalse(result.Success);
		}
		
		[TestMethod]
		public void UserManagerTestSignUpResultError()
		{
			var result = new SignUpResult(null,false, new SignUpResultError());
			Assert.AreEqual(new SignUpResultError(),result.Error);
		}
	}
}

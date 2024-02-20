using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;

namespace starskytest.starsky.foundation.database.Models.Account
{
	[TestClass]
	public sealed class CredentialTest
	{
		[TestMethod]
		public void CredentialSetupTest()
		{
			//        public int Id
			//        public int UserId 
			//        public int CredentialTypeId 
			//        public string Identifier 
			//        public string Secret 
			//        public string Extra 
			//        public User User 
			//        public CredentialType CredentialType 

			var credential = new Credential
			{
				Id = 0,
				UserId = 0,
				CredentialTypeId = 0,
				Identifier = string.Empty,
				Secret = string.Empty,
				Extra = string.Empty,
				User = new User(),
				CredentialType = new CredentialType()
			};

			Assert.AreEqual(0, credential.Id);
			Assert.AreEqual(0, credential.UserId);
			Assert.AreEqual(0, credential.CredentialTypeId);
			Assert.AreEqual(string.Empty, credential.Identifier);
			Assert.AreEqual(new User().Id, credential.User.Id);
			Assert.AreEqual(new CredentialType().Code, credential.CredentialType.Code);
		}
	}
}

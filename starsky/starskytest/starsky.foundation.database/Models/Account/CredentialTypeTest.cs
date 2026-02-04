using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;

namespace starskytest.starsky.foundation.database.Models.Account
{
	[TestClass]
	public sealed class CredentialTypeTest
	{
		[TestMethod]
		public void CredentialType_Name_Credentials()
		{
			var rolePermission = new CredentialType();

			Assert.IsNull(rolePermission.Name);
			Assert.IsNull(rolePermission.Credentials);
		}

		[TestMethod]
		public void CredentialTypeSetup_Test()
		{
			var credentialType = new CredentialType
			{
				Id = 0,
				Code = string.Empty,
				Name = string.Empty,
				Position = 0,
				Credentials = new List<Credential>()
			};
			Assert.AreEqual(0, credentialType.Id);
			Assert.AreEqual(0, credentialType.Position);
			Assert.AreEqual(string.Empty, credentialType.Code);
		}
	}
}

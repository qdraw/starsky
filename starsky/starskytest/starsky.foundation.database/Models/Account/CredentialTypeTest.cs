using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;

namespace starskytest.starsky.foundation.database.Models.Account
{
	[TestClass]
	public class CredentialTypeTest
	{
		[TestMethod]
		public void CredentialType_Name_Credentials()
		{
			var rolePermission = new CredentialType();
			
			Assert.IsNull(rolePermission.Name);
			Assert.IsNull(rolePermission.Credentials);
		}
	}
}

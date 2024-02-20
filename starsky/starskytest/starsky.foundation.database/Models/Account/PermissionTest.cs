using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;

namespace starskytest.starsky.foundation.database.Models.Account
{
	[TestClass]
	public sealed class PermissionTest
	{
		[TestMethod]
		public void CredentialSetupTest()
		{
			var permission = new Permission
			{
				Id = 0, Code = string.Empty, Name = string.Empty, Position = 0
			};
			Assert.AreEqual(0, permission.Id);
			Assert.AreEqual(0, permission.Position);
			Assert.AreEqual(string.Empty, permission.Code);
		}
	}
}

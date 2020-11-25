using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;

namespace starskytest.starsky.foundation.database.Models.Account
{
	[TestClass]
	public class UserRoleTest
	{
		[TestMethod]
		public void UserRole_User_Role()
		{
			var rolePermission = new UserRole();
			
			Assert.IsNull(rolePermission.Role);
			Assert.IsNull(rolePermission.User);
		}
	}
}

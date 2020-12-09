using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;

namespace starskytest.starsky.foundation.database.Models.Account
{
	[TestClass]
	public class RolePermissionTest
	{
		[TestMethod]
		public void RolePermission_Role_Permission()
		{
			var rolePermission = new RolePermission();
			
			Assert.IsNull(rolePermission.Role);
			Assert.IsNull(rolePermission.Permission);
		}
	}
}

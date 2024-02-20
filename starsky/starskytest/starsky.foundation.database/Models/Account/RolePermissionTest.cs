using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;

namespace starskytest.starsky.foundation.database.Models.Account
{
	[TestClass]
	public sealed class RolePermissionTest
	{
		[TestMethod]
		public void RolePermission_Role_Permission()
		{
			var rolePermission = new RolePermission();

			Assert.IsNull(rolePermission.Role);
			Assert.IsNull(rolePermission.Permission);
		}

		[TestMethod]
		public void RolePermissionSetupTest()
		{
			// RoleId +  PermissionId
			var rolePermission = new RolePermission
			{
				RoleId = 0, PermissionId = 0, Role = new Role(), Permission = new Permission()
			};
			Assert.AreEqual(0, rolePermission.RoleId);
			Assert.AreEqual(0, rolePermission.PermissionId);
		}
	}
}

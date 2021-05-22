﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;

namespace starskytest.Models.Account
{
	[TestClass]
	public class UserRoleTest
	{
		[TestMethod]
		public void UserRoleTest_SetupTest()
		{
			var role = new UserRole()
			{
				UserId = 0,
				RoleId = 0,
				User = new User(),
				Role = new Role()
			};
			Assert.AreEqual(0, role.UserId);
			Assert.AreEqual(0, role.RoleId);
		}
	}
}

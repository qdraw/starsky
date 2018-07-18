﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models.Account;

namespace starskytests.Models.Account
{
    [TestClass]
    public class RolePermissionTest
    {
        [TestMethod]
        public void RolePermissionSetupTest()
        {
            // RoleId +  PermissionId
            var creds = new RolePermission
            {
                RoleId = 0,
                PermissionId = 0,
            };
            Assert.AreEqual(0, creds.RoleId);
            Assert.AreEqual(0, creds.PermissionId);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Models.Account;

namespace starskytest.Models.Account
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
                Role = new Role(),
                Permission = new Permission()
            };
            Assert.AreEqual(0, creds.RoleId);
            Assert.AreEqual(0, creds.PermissionId);
        }
    }
}
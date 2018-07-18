using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models.Account;

namespace starskytests.Models.Account
{
    [TestClass]
    public class RoleTest
    {
        [TestMethod]
        public void RoleSetupTest()
        {
           
            var role = new Role
            {
                Id = 0,
                Code = string.Empty,
                Name = string.Empty,
                Position = 0
            };
            Assert.AreEqual(0, role.Id);
            Assert.AreEqual(0, role.Position);
            Assert.AreEqual(string.Empty, role.Code);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Models.Account;

namespace starskytests.Models.Account
{
    [TestClass]
    public class PermissionTest
    {
        [TestMethod]
        public void CredentialSetupTest()
        {
            var creds = new Permission
            {
                Id = 0,
                Code = string.Empty,
                Name = string.Empty,
                Position = 0
            };
            Assert.AreEqual(0, creds.Id);
            Assert.AreEqual(0, creds.Position);
            Assert.AreEqual(string.Empty, creds.Code);

        }
    }
}
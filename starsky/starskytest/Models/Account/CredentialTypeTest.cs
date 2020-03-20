using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.database.Models.Account;

namespace starskytest.Models.Account
{
    [TestClass]
    public class CredentialTypeTest
    {
        [TestMethod]
        public void CredentialTypeSetup_Test()
        {
            var creds = new CredentialType
            {
                Id = 0,
                Code = string.Empty,
                Name = string.Empty,
                Position = 0,
                Credentials = new List<Credential>()
            };
            Assert.AreEqual(0, creds.Id);
            Assert.AreEqual(0, creds.Position);
            Assert.AreEqual(string.Empty, creds.Code);
            
        }
    }
}
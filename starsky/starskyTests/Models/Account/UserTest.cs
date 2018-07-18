using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models.Account;

namespace starskytests.Models.Account
{
    [TestClass]
    public class UserTest
    {
        [TestMethod]
        public void UserTestSetupTest()
        {
            var role = new User()
            {
                Id = 0,
                Name = string.Empty,
                Created = DateTime.Now,
                Credentials = new List<Credential>()
            };
            Assert.AreEqual(0, role.Id);
            Assert.AreEqual(string.Empty, role.Name);
        }
    }
}
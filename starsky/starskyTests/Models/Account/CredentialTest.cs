using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models.Account;

namespace starskytests.Models.Account
{
    [TestClass]
    public class CredentialTest
    {
        [TestMethod]
        public void CredentialSetupTest()
        {
            
        //        public int Id
        //        public int UserId 
        //        public int CredentialTypeId 
        //        public string Identifier 
        //        public string Secret 
        //        public string Extra 
        //        public User User 
        //        public CredentialType CredentialType 

            var creds = new Credential
            {
                Id = 0,
                UserId = 0,
                CredentialTypeId = 0,
                Identifier = string.Empty,
                Secret = string.Empty,
                Extra = string.Empty,
                User = new User(),
                CredentialType = new CredentialType()
            };
           
            Assert.AreEqual(0, creds.Id);
            Assert.AreEqual(0, creds.UserId);
            Assert.AreEqual(0, creds.CredentialTypeId);

        }
    }
}
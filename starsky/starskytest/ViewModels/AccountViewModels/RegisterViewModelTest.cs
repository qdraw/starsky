using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.accountmanagement.Models.Account;

namespace starskytest.ViewModels.AccountViewModels
{
    [TestClass]
    public class RegisterViewModelTest
    {
        [TestMethod]
        public void RegisterViewModelLoadAll()
        {
            var model = new RegisterViewModel
            {
                Email = "dont@mail.us",
                Password = "123456",
                ConfirmPassword = "123456"
            };
            Assert.AreEqual("123456",model.ConfirmPassword);
            Assert.AreEqual("123456",model.Password);
            Assert.AreEqual("dont@mail.us",model.Email);
        }
    }
}
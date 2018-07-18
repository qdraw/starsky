﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.ViewModels.Account;

namespace starskytests.ViewModels.AccountViewModels
{
    [TestClass]
    public class LoginViewModelTest
    {
        [TestMethod]
        public void LoginViewModelTestLoadAll()
        {
            var model = new LoginViewModel
            {
                Email = "dont@mail.us",
                Password = "123456",
                RememberMe = true
            };
            Assert.AreEqual("123456",model.Password);
            Assert.AreEqual("dont@mail.us",model.Email);
            Assert.AreEqual(true,model.RememberMe);
        }
    }
}
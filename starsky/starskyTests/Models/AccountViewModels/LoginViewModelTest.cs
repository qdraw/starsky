﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models.AccountViewModels;

namespace starskytests.Models.AccountViewModels
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
            };
            Assert.AreEqual("123456",model.Password);
            Assert.AreEqual("dont@mail.us",model.Email);
        }
    }
}
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;

namespace starskytests.Models
{
    [TestClass]
    public class ImportSettingsModelTest
    {
        [TestMethod]
        public void ImportSettingsModel_toDefaults_Test()
        {
            var context = new DefaultHttpContext();

            var importSettings = new ImportSettingsModel(context.Request);
            Assert.AreEqual(string.Empty,importSettings.Structure);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ImportSettingsModel_FailingInput_Test()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Structure"] = "wrong";
            new ImportSettingsModel(context.Request);
        }
        
    }
}
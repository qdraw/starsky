using System;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Models;

namespace starskytest.Models
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

	    [TestMethod]
	    public void ImportSettingsModel_IndexMode_Test()
	    {
		    var context = new DefaultHttpContext();
		    // false
		    context.Request.Headers["IndexMode"] = "false";
		    var model = new ImportSettingsModel(context.Request);
			Assert.AreEqual(false, model.IndexMode);
		    
		    // now true
		    context.Request.Headers["IndexMode"] = "true";
		    model = new ImportSettingsModel(context.Request);
		    Assert.AreEqual(true, model.IndexMode);
	    }

	    [TestMethod]
	    public void ImportSettingsModel_AgeFileFilter_Test()
	    {
		    var context = new DefaultHttpContext();
		    // false
		    context.Request.Headers["AgeFileFilter"] = "false";
		    var model = new ImportSettingsModel(context.Request);
		    Assert.AreEqual(false, model.AgeFileFilterDisabled);
		    
		    // now true
		    context.Request.Headers["AgeFileFilter"] = "true";
		    model = new ImportSettingsModel(context.Request);
		    Assert.AreEqual(true, model.AgeFileFilterDisabled);
		    
	    }

    }
}
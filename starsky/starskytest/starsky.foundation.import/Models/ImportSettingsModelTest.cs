using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Models;

namespace starskytest.starsky.foundation.import.Models;

[TestClass]
public sealed class ImportSettingsModelTest
{
	[TestMethod]
	public void ImportSettingsModel_toDefaults_Test()
	{
		var context = new DefaultHttpContext();

		var importSettings = new ImportSettingsModel(context.Request);
		Assert.AreEqual(string.Empty, importSettings.Structure);
	}

	[TestMethod]
	public void ImportSettingsModel_FailingInput_Test()
	{
		var context = new DefaultHttpContext();
		context.Request.Headers["Structure"] = "wrong";
		var result = new ImportSettingsModel(context.Request);

		Assert.AreEqual("Structure 'wrong' is not valid", result.StructureErrors[0]);
	}

	[TestMethod]
	public void ImportSettingsModel_IndexMode_Test()
	{
		var context = new DefaultHttpContext();
		// false
		context.Request.Headers["IndexMode"] = "false";
		var model = new ImportSettingsModel(context.Request);
		Assert.IsFalse(model.IndexMode);

		// now true
		context.Request.Headers["IndexMode"] = "true";
		model = new ImportSettingsModel(context.Request);
		Assert.IsTrue(model.IndexMode);
	}
	
	[TestMethod]
	public void ImportSettingsModel_ImportOrigin_Test()
	{
		var context = new DefaultHttpContext();
		context.Request.Headers["ImportOrigin"] = "test";
		var model = new ImportSettingsModel(context.Request);
		Assert.AreEqual("test", model.Origin);
	}
}

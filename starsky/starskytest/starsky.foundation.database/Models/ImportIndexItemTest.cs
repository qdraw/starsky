using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.import.Models;

namespace starskytest.starsky.foundation.database.Models;

[TestClass]
public sealed class ImportIndexItemTest
{
	[TestMethod]
	public void ImportIndexItem_CtorRequest_ColorClass()
	{
		var context = new DefaultHttpContext();
		context.Request.Headers["ColorClass"] = "1";
		var model = new ImportSettingsModel(context.Request);
		Assert.AreEqual(1, model.ColorClass);
	}

	[TestMethod]
	public void ImportFileSettingsModel_DefaultsToIgnore_Test()
	{
		var importSettings = new ImportSettingsModel { ColorClass = 999 };
		Assert.AreEqual(-1, importSettings.ColorClass);
	}
}

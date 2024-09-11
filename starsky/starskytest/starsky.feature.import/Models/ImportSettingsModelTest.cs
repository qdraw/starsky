﻿using System;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.import.Models;

namespace starskytest.starsky.feature.import.Models;

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

		Assert.ThrowsException<ArgumentException>(() => new ImportSettingsModel(context.Request));
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
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
using starsky.project.web.ViewModels;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public class AppSettingsFeaturesControllerTest
{
	[TestMethod]
	public void FeaturesViewTest()
	{
		// Arrange
		var fakeIMoveToTrashService = new FakeIMoveToTrashService(new List<FileIndexItem>());
		var appSettingsFeaturesController = new AppSettingsFeaturesController(
			fakeIMoveToTrashService, new AppSettings());

		// Act
		var result = appSettingsFeaturesController.FeaturesView() as JsonResult;
		var json = result?.Value as EnvFeaturesViewModel;
		Assert.IsNotNull(json);

		// Assert
		Assert.IsNotNull(result);
	}

	[TestMethod]
	public void FeaturesViewTest_Disabled()
	{
		// Arrange
		var fakeIMoveToTrashService = new FakeIMoveToTrashService(new List<FileIndexItem>(), false);
		var appSettingsFeaturesController = new AppSettingsFeaturesController(
			fakeIMoveToTrashService, new AppSettings { UseLocalDesktop = false });

		// Act
		var result = appSettingsFeaturesController.FeaturesView() as JsonResult;
		var json = result?.Value as EnvFeaturesViewModel;
		Assert.IsNotNull(json);

		// Assert
		Assert.IsFalse(json.UseLocalDesktop);
		Assert.IsFalse(json.SystemTrashEnabled);
	}

	[TestMethod]
	public void FeaturesViewTest_Enabled()
	{
		// Arrange
		var fakeIMoveToTrashService = new FakeIMoveToTrashService(new List<FileIndexItem>());
		var appSettingsFeaturesController = new AppSettingsFeaturesController(
			fakeIMoveToTrashService, new AppSettings { UseLocalDesktop = true });

		// Act
		var result = appSettingsFeaturesController.FeaturesView() as JsonResult;
		var json = result?.Value as EnvFeaturesViewModel;
		Assert.IsNotNull(json);

		// Assert
		Assert.IsTrue(json.UseLocalDesktop);
		Assert.IsTrue(json.SystemTrashEnabled);
	}
}

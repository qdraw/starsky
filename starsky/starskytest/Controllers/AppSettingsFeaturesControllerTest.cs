using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Models;
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
		var result = appSettingsFeaturesController.FeaturesView();
		
		// Assert
		Assert.IsNotNull(result);
	}
}

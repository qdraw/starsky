using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.database.Models;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public class AppSettingsFeaturesControllerTest
{
	[TestMethod]
	public void FeaturesViewTest()
	{
		// Arrange
		var appSettingsFeaturesController = new AppSettingsFeaturesController(new FakeIMoveToTrashService(new List<FileIndexItem>()));
		
		// Act
		var result = appSettingsFeaturesController.FeaturesView();
		
		// Assert
		Assert.IsNotNull(result);
	}
}

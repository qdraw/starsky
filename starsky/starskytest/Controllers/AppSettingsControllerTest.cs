using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Models;

namespace starskytest.Controllers
{
	[TestClass]
	public class AppSettingsControllerTest
	{

		[TestMethod]
		public void ENV_StarskyTestEnv()
		{
			var controller = new AppSettingsController(new AppSettings(),null);
			var actionResult = controller.Env() as JsonResult;
			var resultAppSettings = actionResult.Value as AppSettings;
			Assert.AreEqual("Starsky", resultAppSettings.Name);
		}
		
		[TestMethod]
		public void T()
		{
			
		}

	}
}

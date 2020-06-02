using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starskytest.FakeMocks;

namespace starskytest.Controllers
{
	[TestClass]
	public class AppSettingsControllerTest
	{

		[TestMethod]
		public void ENV_StarskyTestEnv()
		{
			var controller = new AppSettingsController(new AppSettings(),new FakeSelectorStorage());
			var actionResult = controller.Env() as JsonResult;
			var resultAppSettings = actionResult.Value as AppSettings;
			Assert.AreEqual("Starsky", resultAppSettings.Name);
		}
		
		[TestMethod]
		public async Task UpdateAppSettings_Verbose()
		{
			var controller = new AppSettingsController(new AppSettings(), new FakeSelectorStorage());
			var actionResult = await controller.UpdateAppSettings(new AppSettingsTransferObject {Verbose = true}) as JsonResult;
			var result = actionResult.Value as AppSettings;
			Assert.IsTrue(result.Verbose);
		}
		
		[TestMethod]
		public async Task UpdateAppSettings_StorageFolder()
		{
			var controller = new AppSettingsController(new AppSettings(), new FakeSelectorStorage());
			var actionResult = await controller.UpdateAppSettings(new AppSettingsTransferObject {Verbose = true, StorageFolder = "test"}) as JsonResult;
			var result = actionResult.Value as AppSettings;
			Assert.IsTrue(result.Verbose);
			Assert.AreEqual(PathHelper.AddBackslash("test"),result.StorageFolder);
		}

	}
}

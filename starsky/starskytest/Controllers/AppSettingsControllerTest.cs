using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.platform.Services;
using starsky.foundation.storage.Helpers;
using starsky.foundation.storage.Storage;
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
			var storage = new FakeIStorage(new List<string> { "test" });
			Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);
			
			var controller = new AppSettingsController(new AppSettings(), new FakeSelectorStorage(storage));
			var actionResult = await controller.UpdateAppSettings(new AppSettingsTransferObject
			{
				Verbose = true, StorageFolder = "test"
			}) as JsonResult;
			var result = actionResult.Value as AppSettings;
			Assert.IsTrue(result.Verbose);
			Assert.AreEqual(PathHelper.AddBackslash("test"),result.StorageFolder);
		}

		[TestMethod]
		public async Task UpdateAppSettingsTest_IgnoreWhenEnvIsSet()
		{
			var storage = new FakeIStorage(new List<string> { "test" });
			
			Environment.SetEnvironmentVariable("app__storageFolder",
				"any_value");

			var appSettings = new AppSettings();
			var controller = new AppSettingsController(appSettings, new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			await controller.UpdateAppSettings(
				new AppSettingsTransferObject
				{
					StorageFolder = "test"
				});

			Assert.AreEqual(403, controller.Response.StatusCode);
		}
		
		[TestMethod]
		public async Task UpdateAppSettingsTest_DirNotFound()
		{
			var storage = new FakeIStorage(new List<string> { "test" });
			
			Environment.SetEnvironmentVariable("app__storageFolder",string.Empty);

			var appSettings = new AppSettings();
			var controller = new AppSettingsController(appSettings, new FakeSelectorStorage(storage));
			controller.ControllerContext.HttpContext = new DefaultHttpContext();
			var actionResult = (await controller.UpdateAppSettings(
				new AppSettingsTransferObject
				{
					StorageFolder = "not_found"
				})) as NotFoundObjectResult;

			Assert.AreEqual(404, actionResult.StatusCode);
		}
		
		[TestMethod]
		public async Task UpdateAppSettingsTest_StorageFolder_JsonCheck()
		{
			var storage = new FakeIStorage(new List<string> { "test" });
			Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);
			
			var appSettings = new AppSettings
			{
				AppSettingsPath = $"{Path.DirectorySeparatorChar}temp{Path.DirectorySeparatorChar}appsettings.json"
			};
			var controller = new AppSettingsController(appSettings, new FakeSelectorStorage(storage));
			await controller.UpdateAppSettings(
				new AppSettingsTransferObject
				{
					Verbose = true, StorageFolder = "test"
				});

			Assert.IsTrue(storage.ExistFile(appSettings.AppSettingsPath));

			var jsonContent= await new PlainTextFileHelper().StreamToStringAsync(
				storage.ReadStream(appSettings.AppSettingsPath));

			Assert.IsTrue(jsonContent.Contains("app\": {"));
			Assert.IsTrue(jsonContent.Contains("\"StorageFolder\": \""));
		}
	}
}

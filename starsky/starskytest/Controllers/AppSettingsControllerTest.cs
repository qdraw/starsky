using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Controllers;
using starsky.feature.settings.Models;
using starsky.feature.settings.Services;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starsky.foundation.storage.Helpers;
using starskytest.FakeMocks;

namespace starskytest.Controllers;

[TestClass]
public sealed class AppSettingsControllerTest
{
	[TestMethod]
	public void ENV_StarskyTestEnv()
	{
		var controller =
			new AppSettingsController(new AppSettings(), new FakeIUpdateAppSettingsByPath());
		var actionResult = controller.Env() as JsonResult;
		var resultAppSettings = actionResult?.Value as AppSettings;
		Assert.AreEqual("Starsky", resultAppSettings?.Name);
	}

	[TestMethod]
	public void ENV_StarskyTestEnv_ForceHtml()
	{
		var controller = new AppSettingsController(new AppSettings(),
			new FakeIUpdateAppSettingsByPath());
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		controller.ControllerContext.HttpContext.Request.Headers.Append("x-force-html", "true");
		var actionResult = controller.Env() as JsonResult;
		var resultAppSettings = actionResult?.Value as AppSettings;
		Assert.AreEqual("Starsky", resultAppSettings?.Name);
		Assert.AreEqual("text/html; charset=utf-8",
			controller.ControllerContext.HttpContext.Response.Headers.ContentType.ToString());
	}

	[TestMethod]
	public async Task UpdateAppSettings_Verbose()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage(new List<string> { "/" });
		var controller = new AppSettingsController(appSettings,
			new UpdateAppSettingsByPath(appSettings, new FakeSelectorStorage(storage)));

		var actionResult =
			await controller.UpdateAppSettings(new AppSettingsTransferObject { Verbose = true })
				as JsonResult;
		var result = actionResult?.Value as AppSettings;
		Assert.IsTrue(result?.Verbose);
	}

	[TestMethod]
	public async Task UpdateAppSettings_StorageFolder()
	{
		const string testFolder = "test-update-settings-storage";
		var appSettings = new AppSettings();
		var controller = new AppSettingsController(appSettings, new UpdateAppSettingsByPath(
			appSettings,
			new FakeSelectorStorage(
				new FakeIStorage(new List<string>
				{
					$"{Path.DirectorySeparatorChar}{testFolder}"
				}))));

		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		var actionResult = await controller.UpdateAppSettings(new AppSettingsTransferObject
		{
			Verbose = true, StorageFolder = $"{Path.DirectorySeparatorChar}{testFolder}"
		}) as JsonResult;

		var result = actionResult?.Value as AppSettings;
		Assert.IsTrue(result?.Verbose);

		Assert.AreEqual(Path.DirectorySeparatorChar + PathHelper.AddBackslash(testFolder),
			result?.StorageFolder);
	}

	[TestMethod]
	public async Task UpdateAppSettingsTest_IgnoreWhenEnvIsSet()
	{
		Environment.SetEnvironmentVariable("app__storageFolder",
			"any_value");

		var appSettings = new AppSettings();
		var controller = new AppSettingsController(appSettings,
			new FakeIUpdateAppSettingsByPath(
				new UpdateAppSettingsStatusModel { StatusCode = 403 }));
		controller.ControllerContext.HttpContext = new DefaultHttpContext();
		await controller.UpdateAppSettings(
			new AppSettingsTransferObject { StorageFolder = "test" });

		Assert.AreEqual(403, controller.Response.StatusCode);
	}

	[TestMethod]
	public async Task UpdateAppSettingsTest_DirNotFound()
	{
		var appSettings = new AppSettings();
		var controller = new AppSettingsController(appSettings,
			new FakeIUpdateAppSettingsByPath(
				new UpdateAppSettingsStatusModel { StatusCode = 404 }));
		controller.ControllerContext.HttpContext = new DefaultHttpContext();

		await controller.UpdateAppSettings(
			new AppSettingsTransferObject { StorageFolder = "not_found" });

		Assert.AreEqual(404, controller.Response.StatusCode);
	}

	[TestMethod]
	public async Task UpdateAppSettingsTest_StorageFolder_JsonCheck()
	{
		var storage = new FakeIStorage(new List<string> { "test" });
		Environment.SetEnvironmentVariable("app__storageFolder", string.Empty);

		var appSettings = new AppSettings
		{
			AppSettingsPath =
				$"{Path.DirectorySeparatorChar}temp{Path.DirectorySeparatorChar}appsettings.json"
		};
		var controller = new AppSettingsController(appSettings,
			new UpdateAppSettingsByPath(appSettings, new FakeSelectorStorage(storage)));
		await controller.UpdateAppSettings(
			new AppSettingsTransferObject { Verbose = true, StorageFolder = "test" });

		Assert.IsTrue(storage.ExistFile(appSettings.AppSettingsPath));

		var jsonContent = await StreamToStringHelper.StreamToStringAsync(
			storage.ReadStream(appSettings.AppSettingsPath));

		Assert.IsTrue(jsonContent.Contains("app\": {"));
		Assert.IsTrue(jsonContent.Contains("\"StorageFolder\": \""));
	}

	[TestMethod]
	public async Task UpdateAppSettings_UseLocalDesktop()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage(new List<string> { "/" });
		var controller = new AppSettingsController(appSettings,
			new UpdateAppSettingsByPath(appSettings, new FakeSelectorStorage(storage)));

		var actionResult =
			await controller.UpdateAppSettings(
				new AppSettingsTransferObject { UseLocalDesktop = true }) as JsonResult;
		var result = actionResult?.Value as AppSettings;
		Assert.IsTrue(result?.UseLocalDesktop);
	}

	[TestMethod]
	public async Task UpdateAppSettings_UseSystemTrash()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage(new List<string> { "/" });
		var controller = new AppSettingsController(appSettings,
			new UpdateAppSettingsByPath(appSettings, new FakeSelectorStorage(storage)));

		var actionResult =
			await controller.UpdateAppSettings(
				new AppSettingsTransferObject { UseSystemTrash = true }) as JsonResult;
		var result = actionResult?.Value as AppSettings;
		Assert.IsTrue(result?.UseSystemTrash);
	}

	[TestMethod]
	public async Task UpdateAppSettings_Verbose_IgnoreSystemTrashValue()
	{
		var appSettings = new AppSettings();
		var storage = new FakeIStorage(new List<string> { "/" });
		var controller = new AppSettingsController(appSettings,
			new UpdateAppSettingsByPath(appSettings, new FakeSelectorStorage(storage)));

		var actionResult =
			await controller.UpdateAppSettings(new AppSettingsTransferObject { Verbose = true })
				as JsonResult;
		var result = actionResult?.Value as AppSettings;

		Assert.AreEqual(appSettings.UseSystemTrash, result?.UseSystemTrash);
	}

	[TestMethod]
	public async Task UpdateAppSettings_AllowedTypesThumb_ReturnsBadRequest()
	{
		// Arrange
		var appSettings = new AppSettings();
		var controller = new AppSettingsController(appSettings,
			new UpdateAppSettingsByPath(appSettings, new FakeSelectorStorage(new FakeIStorage())));

		controller.ModelState.AddModelError("Key", "ErrorMessage");

		// Act
		var result = await controller.UpdateAppSettings(null!);

		// Assert
		Assert.IsInstanceOfType<BadRequestObjectResult>(result);
	}
}

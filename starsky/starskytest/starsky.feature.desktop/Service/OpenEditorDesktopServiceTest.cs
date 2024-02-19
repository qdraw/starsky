using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.desktop.Models;
using starsky.feature.desktop.Service;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.desktop.Service;

[TestClass]
public class OpenEditorDesktopServiceTest
{
	[TestMethod]
	public async Task OpenAsync_stringInput_HappyFlow()
	{
		var fakeService =
			new FakeIOpenApplicationNativeService(new List<string> { "/test.jpg" }, "test");

		var appSettings = new AppSettings
		{
			UseLocalDesktop = true,
			DefaultDesktopEditor = new List<AppSettingsDefaultEditorApplication>
			{
				new AppSettingsDefaultEditorApplication
				{
					ApplicationPath = "app",
					ImageFormats = new List<ExtensionRolesHelper.ImageFormat>
					{
						ExtensionRolesHelper.ImageFormat.jpg
					}
				}
			}
		};

		var preflight = new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>
		{
			new PathImageFormatExistsAppPathModel
			{
				AppPath = "test",
				Status = FileIndexItem.ExifStatus.Ok,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
				SubPath = "/test.jpg",
				FullFilePath = "/test.jpg"
			}
		});

		var service =
			new OpenEditorDesktopService(appSettings, fakeService, preflight);

		var (success, status, list) =
			await service.OpenAsync("/test.jpg;/test2.jpg", true);

		Assert.IsTrue(success);
		Assert.AreEqual("Opened", status);
		Assert.AreEqual(1, list.Count);
		Assert.AreEqual("/test.jpg", list[0].SubPath);
		Assert.AreEqual("test", list[0].AppPath);
	}
	
	
	[TestMethod]
	public async Task OpenAsync_ListInput_HappyFlow()
	{
		var fakeService =
			new FakeIOpenApplicationNativeService(new List<string> { "/test.jpg" }, "test");

		var appSettings = new AppSettings
		{
			UseLocalDesktop = true,
			DefaultDesktopEditor = new List<AppSettingsDefaultEditorApplication>
			{
				new AppSettingsDefaultEditorApplication
				{
					ApplicationPath = "app",
					ImageFormats = new List<ExtensionRolesHelper.ImageFormat>
					{
						ExtensionRolesHelper.ImageFormat.jpg
					}
				}
			}
		};

		var preflight = new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>
		{
			new PathImageFormatExistsAppPathModel
			{
				AppPath = "test",
				Status = FileIndexItem.ExifStatus.Ok,
				ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
				SubPath = "/test.jpg",
				FullFilePath = "/test.jpg"
			}
		});

		var service =
			new OpenEditorDesktopService(appSettings, fakeService, preflight);

		var (success, status, list) =
			await service.OpenAsync(new List<string> { "/test.jpg" }, true);

		Assert.IsTrue(success);
		Assert.AreEqual("Opened", status);
		Assert.AreEqual(1, list.Count);
		Assert.AreEqual("/test.jpg", list[0].SubPath);
		Assert.AreEqual("test", list[0].AppPath);
	}

	[TestMethod]
	public async Task OpenAsync_ListInput_NoFilesSelected()
	{
		var fakeService =
			new FakeIOpenApplicationNativeService(new List<string>(), string.Empty);

		var appSettings = new AppSettings { UseLocalDesktop = true };

		var preflight = new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>());

		var service =
			new OpenEditorDesktopService(appSettings, fakeService, preflight);

		var (success, status, list) =
			( await service.OpenAsync(new List<string> { "/test.jpg" }, true) );

		Assert.IsFalse(success);
		Assert.AreEqual("No files selected", status);
		Assert.AreEqual(0, list.Count);
	}

	[TestMethod]
	public async Task OpenAsync_ListInput_UseLocalDesktop_Null()
	{
		var fakeService =
			new FakeIOpenApplicationNativeService(new List<string>(), string.Empty);

		var appSettings = new AppSettings { UseLocalDesktop = false };

		var preflight = new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>());

		var service =
			new OpenEditorDesktopService(appSettings, fakeService, preflight);

		var (success, status, list) =
			( await service.OpenAsync(new List<string> { "/test.jpg" }, true) );

		Assert.IsNull(success);
		Assert.AreEqual("UseLocalDesktop feature toggle is disabled", status);
		Assert.AreEqual(0, list.Count);
	}
}

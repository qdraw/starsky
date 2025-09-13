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
		var fakeService = new FakeIOpenApplicationNativeService(
			new List<string> { "/test.jpg" }, "test");

		var appSettings = new AppSettings
		{
			UseLocalDesktop = true,
			DefaultDesktopEditor = new List<AppSettingsDefaultEditorApplication>
			{
				new()
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
			new()
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
		Assert.HasCount(1, list);
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
				new()
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
			new()
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
		Assert.HasCount(1, list);
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
			await service.OpenAsync(new List<string> { "/test.jpg" }, true);

		Assert.IsFalse(success);
		Assert.AreEqual("No files selected", status);
		Assert.IsEmpty(list);
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
			await service.OpenAsync(new List<string> { "/test.jpg" }, true);

		Assert.IsNull(success);
		Assert.AreEqual("UseLocalDesktop feature toggle is disabled", status);
		Assert.IsEmpty(list);
	}

	[TestMethod]
	public async Task OpenAsync_ListInput_UnSupportedPlatform()
	{
		var fakeService = new FakeIOpenApplicationNativeService(new List<string>(),
			string.Empty, false);

		var appSettings = new AppSettings { UseLocalDesktop = true };

		var preflight = new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>());

		var service = new OpenEditorDesktopService(appSettings, fakeService, preflight);

		var (success, status, list) =
			await service.OpenAsync(new List<string> { "/test.jpg" }, true);

		Assert.IsNull(success);
		Assert.AreEqual("OpenEditor is not supported on this configuration", status);
		Assert.IsEmpty(list);
	}

	[TestMethod]
	public void OpenAmountConfirmationChecker_6Files()
	{
		var appSettings = new AppSettings { DesktopEditorAmountBeforeConfirmation = 5 };

		var service = new OpenEditorDesktopService(appSettings,
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>()));

		var result =
			service.OpenAmountConfirmationChecker(
				"/test.jpg;/test2.jpg;/test3.jpg;/test4.jpg;/test5.jpg;/test6.jpg");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void OpenAmountConfirmationChecker_6Files_Null()
	{
		var appSettings = new AppSettings { DesktopEditorAmountBeforeConfirmation = null };

		var service = new OpenEditorDesktopService(appSettings,
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>()));

		var result =
			service.OpenAmountConfirmationChecker(
				"/test.jpg;/test2.jpg;/test3.jpg;/test4.jpg;/test5.jpg;/test6.jpg");

		// Assumes that the default value is 5
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void OpenAmountConfirmationChecker_4Files()
	{
		var appSettings = new AppSettings { DesktopEditorAmountBeforeConfirmation = 4 };

		var service = new OpenEditorDesktopService(appSettings,
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>()));

		var result =
			service.OpenAmountConfirmationChecker("/test.jpg;/test2.jpg;/test3.jpg;/test4.jpg");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void OpenAmountConfirmationChecker_1File()
	{
		var appSettings = new AppSettings
		{
			DesktopEditorAmountBeforeConfirmation = -90 // invalid value
		};

		var service = new OpenEditorDesktopService(appSettings,
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>()));

		var result = service.OpenAmountConfirmationChecker("/test.jpg");
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsEnabled_FalseDueFeatureFlag()
	{
		var appSettings = new AppSettings { UseLocalDesktop = false };
		var service = new OpenEditorDesktopService(appSettings,
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>()));
		var result = service.IsEnabled();
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void IsEnabled_True()
	{
		var appSettings = new AppSettings
		{
			UseLocalDesktop = true // feature flag enabled
		};
		var service = new OpenEditorDesktopService(appSettings,
			// Default is supported in mock service
			new FakeIOpenApplicationNativeService(new List<string>(), "test"),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>()));
		var result = service.IsEnabled();
		Assert.IsTrue(result);
	}

	[TestMethod]
	public void IsEnabled_FalseDuePlatformNotSupported()
	{
		var appSettings = new AppSettings { UseLocalDesktop = true };
		var service = new OpenEditorDesktopService(appSettings,
			// Is supported false! =>
			new FakeIOpenApplicationNativeService(new List<string>(), "test", false),
			new FakeIOpenEditorPreflight(new List<PathImageFormatExistsAppPathModel>()));
		var result = service.IsEnabled();
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void FilterListOpenDefaultEditorAndSpecificEditor_Test()
	{
		// Arrange
		var inputList = new List<PathImageFormatExistsAppPathModel>
		{
			new()
			{
				FullFilePath = "file1.txt",
				Status = FileIndexItem.ExifStatus.Ok,
				AppPath = string.Empty
			},
			new()
			{
				FullFilePath = "file2.txt",
				Status = FileIndexItem.ExifStatus.Ok,
				AppPath = "editor.exe"
			},
			new()
			{
				FullFilePath = "file3.txt",
				Status = FileIndexItem.ExifStatus.OperationNotSupported,
				AppPath = string.Empty
			},
			new()
			{
				FullFilePath = "file4.txt",
				Status = FileIndexItem.ExifStatus.Ok,
				AppPath = string.Empty
			}
		};

		// Act
		var result =
			OpenEditorDesktopService.FilterListOpenDefaultEditorAndSpecificEditor(inputList);

		// Assert
		Assert.HasCount(2, result.Item1); // Expected number of files without AppPath
		Assert.Contains("file1.txt",
			result.Item1); // Make sure file1.txt is in the list without AppPath
		Assert.DoesNotContain("file2.txt",
			result.Item1); // Make sure file2.txt is not in the list without AppPath
		Assert.HasCount(1, result.Item2); // Expected number of files with AppPath
		Assert.IsTrue(result.Item2.Exists(x =>
			x.FullFilePath == "file2.txt" &&
			x.AppPath ==
			"editor.exe")); // Make sure file2.txt is in the list with AppPath and has correct editor
	}

	[TestMethod]
	public void FilterListOpenSpecificEditor_Test()
	{
		// Arrange
		var inputList = new List<PathImageFormatExistsAppPathModel>
		{
			new()
			{
				FullFilePath = "file1.txt",
				Status = FileIndexItem.ExifStatus.Ok,
				AppPath = ""
			},
			new()
			{
				FullFilePath = "file2.txt",
				Status = FileIndexItem.ExifStatus.Ok,
				AppPath = "editor.exe"
			},
			new()
			{
				FullFilePath = "file3.txt",
				Status = FileIndexItem.ExifStatus.NotFoundNotInIndex,
				AppPath = ""
			},
			new()
			{
				FullFilePath = "file4.txt",
				Status = FileIndexItem.ExifStatus.Ok,
				AppPath = string.Empty
			}
		};

		// Act
		var result =
			OpenEditorDesktopService.FilterListOpenDefaultEditorAndSpecificEditor(inputList);

		// Assert
		Assert.HasCount(1, result.Item2); // Expected number of files with AppPath
		Assert.IsTrue(result.Item2.Exists(x =>
			x is { FullFilePath: "file2.txt", AppPath: "editor.exe" }));
		// Make sure file2.txt is in the list with AppPath and has correct editor
	}
}

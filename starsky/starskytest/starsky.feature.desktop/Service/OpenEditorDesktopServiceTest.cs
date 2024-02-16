using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.desktop.Service;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;
using starskytest.FakeMocks;

namespace starskytest.starsky.feature.desktop.Service;

[TestClass]
public class OpenEditorDesktopServiceTest
{
	[TestMethod]
	public void TEst()
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
		var service =
			new OpenEditorDesktopService(appSettings, fakeService,
				new FakeSelectorStorage(new FakeIStorage(new List<string> { "/test.jpg" })));

		service.Open(new List<string> { "/test.jpg" });
	}
}

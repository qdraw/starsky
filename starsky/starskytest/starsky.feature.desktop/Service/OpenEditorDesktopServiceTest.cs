using System.Collections.Generic;
using System.Threading.Tasks;
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
	public async Task TEst()
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
				new FakeIOpenEditorPreflight());

		await service.OpenAsync(new List<string> { "/test.jpg" }, true);
	}
}

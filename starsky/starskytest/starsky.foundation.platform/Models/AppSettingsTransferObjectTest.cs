using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Helpers;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public class AppSettingsTransferObjectTest
{
	[TestMethod]
	public void AppSettingsTransferObject_Verbose()
	{
		var appSettingsTransferObject = new AppSettingsTransferObject
		{
			Verbose = true,
			StorageFolder = "test",
			UseSystemTrash = true,
			UseLocalDesktop = true,
			DefaultDesktopEditor =
			[
				new AppSettingsDefaultEditorApplication
				{
					ApplicationPath = "app",
					ImageFormats = [ExtensionRolesHelper.ImageFormat.bmp]
				}
			]
		};

		Assert.IsTrue(appSettingsTransferObject.Verbose);
		Assert.AreEqual("test", appSettingsTransferObject.StorageFolder);
		Assert.IsTrue(appSettingsTransferObject.UseSystemTrash);
		Assert.IsTrue(appSettingsTransferObject.UseLocalDesktop);
		Assert.AreEqual("app", appSettingsTransferObject.DefaultDesktopEditor[0].ApplicationPath);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.bmp,
			appSettingsTransferObject.DefaultDesktopEditor[0].ImageFormats[0]);
	}
}

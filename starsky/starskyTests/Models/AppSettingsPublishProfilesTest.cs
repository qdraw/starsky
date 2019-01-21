using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Models;

namespace starskytests.Models
{
	[TestClass]
	public class AppSettingsPublishProfilesTest
	{
		[TestMethod]
		public void AppSettingsPublishProfilesTest_OverlayMaxWidthTestLessThan100()
		{
			var model = new AppSettingsPublishProfiles();
			model.OverlayMaxWidth = 1;
			Assert.AreEqual(100, model.OverlayMaxWidth);
		}

		[TestMethod]
		public void AppSettingsPublishProfilesTest_OverlayMaxWidthTestMoreThan100()
		{
			var model = new AppSettingsPublishProfiles();
			model.OverlayMaxWidth = 101;
			Assert.AreEqual(101, model.OverlayMaxWidth);
		}

		[TestMethod]
		public void AppSettingsPublishProfilesTest_FolderNull()
		{
			var model = new AppSettingsPublishProfiles();
			model.Folder = null;

			var getFolder = model.Folder;

			Assert.AreEqual(string.Empty, getFolder);
		}

		[TestMethod]
		public void AppSettingsPublishProfilesTest_AddSlash()
		{
			var model = new AppSettingsPublishProfiles();
			model.Folder = "/test";
			var getFolder = model.Folder;

			Assert.AreEqual("/test/", getFolder);

		}
	}
}

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starskycore.Models;

namespace starskytest.Models
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


		[TestMethod]
		public void AppSettingsPublishProfilesTest_Path_AssemblyDirectory()
		{
			var appSettings = new AppSettings();
			var model = new AppSettingsPublishProfiles
			{
				Path = "{AssemblyDirectory}" + Path.DirectorySeparatorChar + "test.jpg"
			};

			// in real world this is not always BaseDirectoryProject
			Assert.AreEqual(AppDomain.CurrentDomain.BaseDirectory + "test.jpg", model.Path);
			
		}

		[TestMethod]
		public void AppSettingsPublishProfilesTest_Path_null()
		{
			var model = new AppSettingsPublishProfiles
			{
				Path = null
			};
			
			Assert.AreEqual(string.Empty, model.Path);

		}
	}
}

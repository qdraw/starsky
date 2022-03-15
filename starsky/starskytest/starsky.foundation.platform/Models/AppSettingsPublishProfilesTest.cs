using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models
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
			
			Assert.IsTrue(model.Path.Contains("default.png"));
		}

		[TestMethod]
		public void GetExtensionWithDot_Nothing()
		{
			var model = new AppSettingsPublishProfiles
			{
			};

			var result = model.GetExtensionWithDot("");
			Assert.AreEqual("",result);
		}
		
		
		[TestMethod]
		public void GetExtensionWithDot_Jpeg()
		{
			var model = new AppSettingsPublishProfiles
			{
				ContentType = TemplateContentType.Jpeg
			};

			var result = model.GetExtensionWithDot("test.png");
			Assert.AreEqual(".jpg",result);
		}
				
		[TestMethod]
		public void GetExtensionWithDot_Fallback()
		{
			var model = new AppSettingsPublishProfiles();

			var result = model.GetExtensionWithDot("test.png");
			Assert.AreEqual(".png",result);
		}

		[TestMethod]
		public void AppSettingsPublishProfilesString()
		{
			var data = new AppSettingsPublishProfiles().ToString();
			Assert.IsTrue(data.Contains("ContentType"));
			Assert.IsTrue(data.Contains("SourceMaxWidth"));
			Assert.IsTrue(data.Contains("OverlayMaxWidth"));
			Assert.IsTrue(data.Contains("Path"));
			Assert.IsTrue(data.Contains("Folder"));
			Assert.IsTrue(data.Contains("Append"));
			Assert.IsTrue(data.Contains("OverlayMaxWidth"));
			Assert.IsTrue(data.Contains("Template"));
			Assert.IsTrue(data.Contains("Prepend"));
			Assert.IsTrue(data.Contains("MetaData"));
			Assert.IsTrue(data.Contains("Copy"));
		}
	}
}

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.platform.Models;

namespace starskytest.starsky.foundation.platform.Models;

[TestClass]
public sealed class AppSettingsPublishProfilesTest
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
		model.Folder = null!;

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
		var model = new AppSettingsPublishProfiles { Path = null! };

		Assert.Contains("default.png", model.Path);
	}

	[TestMethod]
	public void GetExtensionWithDot_Nothing()
	{
		var model = new AppSettingsPublishProfiles();

		var result = model.GetExtensionWithDot("");
		Assert.AreEqual("", result);
	}


	[TestMethod]
	public void GetExtensionWithDot_Jpeg()
	{
		var model = new AppSettingsPublishProfiles { ContentType = TemplateContentType.Jpeg };

		var result = model.GetExtensionWithDot("test.png");
		Assert.AreEqual(".jpg", result);
	}

	[TestMethod]
	public void GetExtensionWithDot_Fallback()
	{
		var model = new AppSettingsPublishProfiles();

		var result = model.GetExtensionWithDot("test.png");
		Assert.AreEqual(".png", result);
	}

	[TestMethod]
	public void AppSettingsPublishProfilesString()
	{
		var data = new AppSettingsPublishProfiles().ToString();
		Assert.Contains("ContentType", data);
		Assert.Contains("SourceMaxWidth", data);
		Assert.Contains("OverlayMaxWidth", data);
		Assert.Contains("Path", data);
		Assert.Contains("Folder", data);
		Assert.Contains("Append", data);
		Assert.Contains("OverlayMaxWidth", data);
		Assert.Contains("Template", data);
		Assert.Contains("Prepend", data);
		Assert.Contains("MetaData", data);
		Assert.Contains("Copy", data);
	}
}

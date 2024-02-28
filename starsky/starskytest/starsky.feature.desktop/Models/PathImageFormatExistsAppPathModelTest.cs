using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.feature.desktop.Models;
using starsky.foundation.database.Models;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.feature.desktop.Models;

[TestClass]
public class PathImageFormatExistsAppPathModelTest
{
	[TestMethod]
	public void PathImageFormatExistsAppPathModelTest_Default()
	{
		var model = new PathImageFormatExistsAppPathModel();
		Assert.AreEqual(string.Empty, model.SubPath);
		Assert.AreEqual(string.Empty, model.FullFilePath);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.notfound, model.ImageFormat);
		Assert.AreEqual(FileIndexItem.ExifStatus.Default, model.Status);
		Assert.AreEqual(string.Empty, model.AppPath);
	}

	[TestMethod]
	public void PathImageFormatExistsAppPathModelTest_Set()
	{
		var model = new PathImageFormatExistsAppPathModel
		{
			AppPath = "test",
			Status = FileIndexItem.ExifStatus.Ok,
			ImageFormat = ExtensionRolesHelper.ImageFormat.jpg,
			SubPath = "/test.jpg",
			FullFilePath = "/test.jpg"
		};

		Assert.AreEqual("/test.jpg", model.SubPath);
		Assert.AreEqual("/test.jpg", model.FullFilePath);
		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, model.ImageFormat);
		Assert.AreEqual(FileIndexItem.ExifStatus.Ok, model.Status);
		Assert.AreEqual("test", model.AppPath);
	}
}

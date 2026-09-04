using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.optimisation.Models;
using starsky.foundation.platform.Helpers;

namespace starskytest.starsky.foundation.optimisation.Models;

[TestClass]
public sealed class ImageOptimisationItemTest
{
	[TestMethod]
	public void Constructor_Default_ImageFormatIsUnknown()
	{
		var item = new ImageOptimisationItem
		{
			InputPath = "/input.jpg", OutputPath = "/output.jpg"
		};

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.unknown, item.ImageFormat);
	}

	[TestMethod]
	public void Constructor_SetsInputAndOutputPath()
	{
		var item = new ImageOptimisationItem
		{
			InputPath = "/input.jpg", OutputPath = "/output.jpg"
		};

		Assert.AreEqual("/input.jpg", item.InputPath);
		Assert.AreEqual("/output.jpg", item.OutputPath);
	}

	[TestMethod]
	public void ImageFormat_CanBeOverwritten()
	{
		var item = new ImageOptimisationItem
		{
			InputPath = "/input.jpg",
			OutputPath = "/output.jpg",
			ImageFormat = ExtensionRolesHelper.ImageFormat.jpg
		};

		Assert.AreEqual(ExtensionRolesHelper.ImageFormat.jpg, item.ImageFormat);
	}
}
